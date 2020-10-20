using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed unsafe class WAVEEncoder : IDisposable
    {
        private readonly AVCodec* _codec;
        private readonly AVCodecContext* _codecContext;
        private readonly AVFormatContext* _audioFormatContext;
        private readonly AVOutputFormat* _outputFormat;
        private readonly AVStream* _outputAVStream;
        private readonly SwrContext* _swrContext;
        private readonly AVFrame* _sampledFrame;
        private readonly AVRational _inputStreamTimeBase;

        private readonly Stream _stream;
        public WAVEEncoder(AudioStreamDecoder decoder, Stream stream)
        {
            _outputFormat = ffmpeg.av_guess_format("wav", null, null);
            if (_outputFormat == null)
            {
                throw new InvalidOperationException("OutputFormat is invalid.");
            }

            _audioFormatContext = ffmpeg.avformat_alloc_context();
            _audioFormatContext->oformat = _outputFormat;

            var codecId = AVCodecID.AV_CODEC_ID_PCM_S16LE;
            _codec = ffmpeg.avcodec_find_encoder(codecId);
            if (_codec == null) throw new InvalidOperationException("Codec not found.");

            _outputAVStream = ffmpeg.avformat_new_stream(_audioFormatContext, _codec);
            if (_outputAVStream == null)
            {
                throw new InvalidOperationException("Can't create output stream.");
            }

            _codecContext = _outputAVStream->codec;
            _codecContext->codec_id = codecId;
            _codecContext->codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO;
            _codecContext->time_base = new AVRational { num = 1, den = decoder.SampleRate };
            _codecContext->sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_S16;
            _codecContext->sample_rate = decoder.SampleRate;
            _codecContext->channel_layout = (ulong)ffmpeg.av_get_default_channel_layout(2);
            _codecContext->channels = decoder.ChannelCount;

            _inputStreamTimeBase = decoder.StreamTimeBase;

            if ((_audioFormatContext->oformat->flags & ffmpeg.AVFMT_GLOBALHEADER) != 0)
            {
                _audioFormatContext->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;
            }

            // setup avio
            ffmpeg.avcodec_parameters_from_context(_outputAVStream->codecpar, _codecContext);
            ffmpeg.av_dump_format(_audioFormatContext, 0, "stream", 1);

            ffmpeg.avio_open_dyn_buf(&_audioFormatContext->pb);

            ffmpeg.avformat_write_header(_audioFormatContext, null);
            ffmpeg.avcodec_open2(_codecContext, _codec, null).ThrowExceptionIfError();

            // setup resampling
            _swrContext = ffmpeg.swr_alloc();
            ffmpeg.av_opt_set_int(_swrContext, "in_channel_layout", ffmpeg.av_get_default_channel_layout(decoder.ChannelCount), 0);
            ffmpeg.av_opt_set_int(_swrContext, "out_channel_layout", ffmpeg.av_get_default_channel_layout(2), 0);
            ffmpeg.av_opt_set_int(_swrContext, "in_sample_rate", decoder.SampleRate, 0);
            ffmpeg.av_opt_set_int(_swrContext, "out_sample_rate", decoder.SampleRate, 0);
            ffmpeg.av_opt_set_sample_fmt(_swrContext, "in_sample_fmt", decoder.SampleFormat, 0);
            ffmpeg.av_opt_set_sample_fmt(_swrContext, "out_sample_fmt", AVSampleFormat.AV_SAMPLE_FMT_S16, 0);
            ffmpeg.swr_init(_swrContext).ThrowExceptionIfError();

            _stream = stream;

            _sampledFrame = ffmpeg.av_frame_alloc();
            if (_sampledFrame == null)
            {
                throw new InvalidOperationException("Can't allocate frame for resampling.");
            }
            _sampledFrame->nb_samples = _codecContext->frame_size;
            _sampledFrame->format = (int)_codecContext->sample_fmt;
            _sampledFrame->channel_layout = _codecContext->channel_layout;
            _sampledFrame->channels = _codecContext->channels;
            _sampledFrame->sample_rate = _codecContext->sample_rate;
            if (_sampledFrame->nb_samples <= 0)
            {
                _sampledFrame->nb_samples = 32;
            }


        }

        public void ResampleAndWrite(AVFrame* decodedFrame, ref int frameFinished)
        {

            // Convert

            byte* convertedData = null;

            if (ffmpeg.av_samples_alloc(&convertedData,
                         null,
                         _codecContext->channels,
                         _sampledFrame->nb_samples,
                         _codecContext->sample_fmt, 0) < 0)
            {
                throw new InvalidOperationException("Can't allocate samples.");
            }

            int outSamples = 0;
            fixed (byte** tmp = (byte*[])decodedFrame->data)
            {
                outSamples = ffmpeg.swr_convert(_swrContext, null, 0,
                             //&convertedData,
                             //audioFrameConverted->nb_samples,
                             tmp,
                     decodedFrame->nb_samples);
            }
            if (outSamples < 0)
            {
                throw new InvalidOperationException("Can't resample.");
            }

            for (; ; )
            {
                outSamples = ffmpeg.swr_get_out_samples(_swrContext, 0);
                if ((outSamples < _codecContext->frame_size * _codecContext->channels) || _codecContext->frame_size == 0 && (outSamples < _sampledFrame->nb_samples * _codecContext->channels))
                {
                    break; // see comments, thanks to @dajuric for fixing this
                }

                outSamples = ffmpeg.swr_convert(_swrContext,
                                         &convertedData,
                                         _sampledFrame->nb_samples, null, 0);

                int buffer_size = ffmpeg.av_samples_get_buffer_size(null,
                               _codecContext->channels,
                               _sampledFrame->nb_samples,
                               _codecContext->sample_fmt,
                               0);
                if (buffer_size < 0)
                {
                    throw new InvalidOperationException("Invalid buffer size.");
                }

                if (ffmpeg.avcodec_fill_audio_frame(_sampledFrame,
                         _codecContext->channels,
                         _codecContext->sample_fmt,
                         convertedData,
                         buffer_size,
                         0) < 0)
                {
                    throw new InvalidOperationException("Can't fill audio frame.");
                }

                AVPacket outPacket;
                ffmpeg.av_init_packet(&outPacket);
                outPacket.data = null;
                outPacket.size = 0;
                if (Encode(&outPacket, _sampledFrame, ref frameFinished) < 0)
                {
                    throw new InvalidOperationException("Can't encode audio frame.");
                }


                //outPacket.flags |= ffmpeg.AV_PKT_FLAG_KEY;
                outPacket.stream_index = _outputAVStream->index;
                //outPacket.data = audio_outbuf;
                outPacket.dts = decodedFrame->pkt_dts;
                outPacket.pts = decodedFrame->pkt_pts;
                ffmpeg.av_packet_rescale_ts(&outPacket, _inputStreamTimeBase, _outputAVStream->time_base);

                if (frameFinished != 0)
                {


                    if (ffmpeg.av_interleaved_write_frame(_audioFormatContext, &outPacket) != 0)
                    {
                        throw new InvalidOperationException("Can't write audio frame.");
                    }

                    ffmpeg.av_packet_unref(&outPacket);
                }
            }
        }

        public void FinalizeStream()
        {
            byte* _pAVIOBuf;
            ffmpeg.av_write_trailer(_audioFormatContext);
            int buf_size = ffmpeg.avio_close_dyn_buf(_audioFormatContext->pb, &_pAVIOBuf);
            using (var packetStream = new UnmanagedMemoryStream(_pAVIOBuf, buf_size))
            {

                packetStream.CopyTo(_stream);
            }
        }

        public int EncodeNext(AVPacket* avpkt, AVFrame* frame, ref int got_packet_ptr)
        {
            int ret = 0;
            got_packet_ptr = 0;
            if ((ret = ffmpeg.avcodec_receive_packet(_codecContext, avpkt)) == 0)
            {
                got_packet_ptr = 1;
                //0 on success, otherwise negative error code
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                //output is not available in the current state - user must try to send input
                return Encode(avpkt, frame, ref got_packet_ptr);
            }
            else if (ret == ffmpeg.AVERROR_EOF)
            {
                throw new InvalidOperationException("AVERROR_EOF: the encoder has been fully flushed, and there will be no more output packets");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
            {
                throw new InvalidOperationException("AVERROR(EINVAL) codec not opened, or it is an encoder other errors: legitimate decoding errors");
            }
            else
            {
                throw new InvalidOperationException("unknown");
            }
            return ret;//ffmpeg.avcodec_encode_audio2(audioCodecContext, &outPacket, audioFrameConverted, &frameFinished)
        }
        public int Encode(AVPacket* avpkt, AVFrame* frame, ref int got_packet_ptr)
        {
            int ret = 0;
            got_packet_ptr = 0;
            if ((ret = ffmpeg.avcodec_send_frame(_codecContext, frame)) == 0)
            {
                //0 on success, otherwise negative error code
                return EncodeNext(avpkt, frame, ref got_packet_ptr);
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                throw new InvalidOperationException("input is not accepted in the current state - user must read output with avcodec_receive_packet() (once all output is read, the packet should be resent, and the call will not fail with EAGAIN)");
            }
            else if (ret == ffmpeg.AVERROR_EOF)
            {
                throw new InvalidOperationException("AVERROR_EOF: the decoder has been flushed, and no new packets can be sent to it (also returned if more than 1 flush packet is sent");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
            {
                throw new InvalidOperationException("AVERROR(ffmpeg.EINVAL) codec not opened, refcounted_frames not set, it is a decoder, or requires flush");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.ENOMEM))
            {
                throw new InvalidOperationException("AVERROR(ENOMEM) failed to add packet to internal queue, or similar other errors: legitimate decoding errors");
            }
            else
            {
                throw new InvalidOperationException("unknown");
            }
            //return ret;//ffmpeg.avcodec_encode_audio2(audioCodecContext, &outPacket, audioFrameConverted, &frameFinished)
        }
        public int EncodeFlush()
        {
            return ffmpeg.avcodec_send_frame(_codecContext, null);
        }

        public void Dispose()
        {

            ffmpeg.swr_close(_swrContext);
            fixed (SwrContext** swrContext = &_swrContext)
            {
                ffmpeg.swr_free(swrContext);
            }
            fixed (AVFrame** sampledFrame = &_sampledFrame)
            {
                ffmpeg.av_frame_free(sampledFrame);
            }

            fixed (AVCodecContext** codecContext = &_codecContext)
            {
                ffmpeg.avcodec_close(_codecContext);
                ffmpeg.avcodec_free_context(codecContext);
            }

            fixed (AVFormatContext** audioFormatContext = &_audioFormatContext)
            {
                ffmpeg.avformat_close_input(audioFormatContext);
            }

        }
    }
