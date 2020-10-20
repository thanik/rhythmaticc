using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

    public sealed unsafe class AudioStreamDecoder : IDisposable
    {
        private readonly AVCodecContext* _fileCodecContext;
        private readonly AVFormatContext* _fileFormatContext;
        private readonly AVStream* _inputStream;
        private readonly AVPacket* _packet;

        public string CodecName { get; }
        public int SampleRate { get; }
        public ulong ChannelLayout { get; }
        public AVSampleFormat SampleFormat { get; }
        public int ChannelCount { get; }
        public AVRational StreamTimeBase { get; }
        public int StreamId { get; }

        public AudioStreamDecoder(string filename)
        {
            fixed (AVFormatContext** fileFormatContext = &_fileFormatContext)
            {
                ffmpeg.avformat_open_input(fileFormatContext, filename, null, null).ThrowExceptionIfError();

            }
            ffmpeg.avformat_find_stream_info(_fileFormatContext, null).ThrowExceptionIfError();

            AVCodec* codec;
            StreamId = ffmpeg.av_find_best_stream(_fileFormatContext, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, &codec, 0);

            _fileCodecContext = ffmpeg.avcodec_alloc_context3(codec);
            AVCodecParameters* cp = null;
            ffmpeg.avcodec_parameters_to_context(_fileCodecContext, _fileFormatContext->streams[StreamId]->codecpar);
            ffmpeg.avcodec_open2(_fileCodecContext, codec, null).ThrowExceptionIfError();

            _inputStream = _fileFormatContext->streams[StreamId];

            CodecName = ffmpeg.avcodec_get_name(codec->id);
            SampleRate = _fileCodecContext->sample_rate;
            ChannelLayout = _fileCodecContext->channel_layout;
            SampleFormat = _fileCodecContext->sample_fmt;
            ChannelCount = _fileCodecContext->channels;
            StreamTimeBase = _inputStream->time_base;

            _packet = ffmpeg.av_packet_alloc();

            ffmpeg.av_dump_format(_fileFormatContext, StreamId, filename, 0);
        }

        public int GetDecodedFrame(AVPacket* incomingPacket, AVFrame* frame, ref int frameFinished)
        {
            if (incomingPacket->stream_index == StreamId)
            {
                return Decode(frame, ref frameFinished, incomingPacket);

            }
            else
            {
                return 0;
            }
        }

        public int DecodeNext(AVFrame* frame, ref int got_frame_ptr, AVPacket* avpkt)
        {
            int ret = 0;
            got_frame_ptr = 0;
            if ((ret = ffmpeg.avcodec_receive_frame(_fileCodecContext, frame)) == 0)
            {
                //0 on success, otherwise negative error code
                got_frame_ptr = 1;
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                //AVERROR(EAGAIN): input is not accepted in the current state - user must read output with avcodec_receive_packet()
                //(once all output is read, the packet should be resent, and the call will not fail with EAGAIN)
                ret = Decode(frame, ref got_frame_ptr, avpkt);
            }
            else if (ret == ffmpeg.AVERROR_EOF)
            {
                throw new InvalidOperationException("AVERROR_EOF: the encoder has been flushed, and no new frames can be sent to it");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
            {
                throw new InvalidOperationException("AVERROR(EINVAL): codec not opened, refcounted_frames not set, it is a decoder, or requires flush");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.ENOMEM))
            {
                throw new InvalidOperationException("Failed to add packet to internal queue, or similar other errors: legitimate decoding errors");
            }
            else
            {
                throw new InvalidOperationException("unknown");
            }
            return ret;
        }

        internal bool IsThereNewFrame(AVPacket* inPacket)
        {
            return ffmpeg.av_read_frame(_fileFormatContext, inPacket) < 0;
        }

        public int Decode(AVFrame* frame, ref int got_frame_ptr, AVPacket* avpkt)
        {
            int ret = 0;
            got_frame_ptr = 0;
            if ((ret = ffmpeg.avcodec_send_packet(_fileCodecContext, avpkt)) == 0)
            {
                //0 on success, otherwise negative error code
                if (avpkt->side_data_elems > 0 && avpkt->side_data->type == AVPacketSideDataType.AV_PKT_DATA_SKIP_SAMPLES)
                {
                    ffmpeg.av_read_frame(_fileFormatContext, avpkt);
                }
                return DecodeNext(frame, ref got_frame_ptr, avpkt);
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                throw new InvalidOperationException("input is not accepted in the current state - user must read output with avcodec_receive_frame()(once all output is read, the packet should be resent, and the call will not fail with EAGAIN");
            }
            else if (ret == ffmpeg.AVERROR_EOF)
            {
                throw new InvalidOperationException("AVERROR_EOF: the decoder has been flushed, and no new packets can be sent to it (also returned if more than 1 flush packet is sent");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
            {
                throw new InvalidOperationException("codec not opened, it is an encoder, or requires flush");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.ENOMEM))
            {
                throw new InvalidOperationException("Failed to add packet to internal queue, or similar other errors: legitimate decoding errors");
            }
            else
            {
                throw new InvalidOperationException("unknown");
            }
            //return ret;//ffmpeg.avcodec_decode_audio4(fileCodecContext, audioFrameDecoded, &frameFinished, &inPacket);
        }
        public int DecodeFlush(AVPacket* avpkt)
        {
            avpkt->data = null;
            avpkt->size = 0;
            return ffmpeg.avcodec_send_packet(_fileCodecContext, avpkt);
        }

        public void Dispose()
        {

            ffmpeg.av_packet_unref(_packet);
            ffmpeg.av_free(_packet);

            ffmpeg.avcodec_close(_fileCodecContext);
            fixed (AVFormatContext** fileFormatContext = &_fileFormatContext)
            {
                ffmpeg.avformat_close_input(fileFormatContext);
            }
        }

    public IReadOnlyDictionary<string, string> GetMetadata()
    {
        AVDictionaryEntry* tag = null;
        var result = new Dictionary<string, string>();
        try
        {
            while ((tag = ffmpeg.av_dict_get(_fileFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                result.Add(key, value);
            }
        }
        catch (Exception)
        {
            return new Dictionary<string, string>();
        }

        return result;
    }
}
