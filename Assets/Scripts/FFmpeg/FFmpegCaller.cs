using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpeg.AutoGen;
using System.IO;

public class FFmpegCaller : MonoBehaviour
{

    private MemoryStream ms;
    public delegate void Callback(Dictionary<string, string> metadata);
    void Start()
    {
        DontDestroyOnLoad(this);
        FFmpegBinaryHelper.RegisterFFmpegBinaries();
        Debug.Log($"FFmpeg version info: {ffmpeg.av_version_info()}");

    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    public IEnumerator LoadAudio(string filename, AudioSource targetAudioSource, Callback callback)
    {
        ms = new MemoryStream();
        Dictionary<string, string> metadata;
        DecodeAudioToStream(filename, ms, out metadata);
        long oldSize = 0;
        while (true)
        {
            if (oldSize != ms.Length)
            {
                oldSize = ms.Length;
                yield return new WaitForEndOfFrame();
            }
            else
            {
                break;
            }
        }
        targetAudioSource.clip = OpenWavParser.ByteArrayToAudioClip(ms.ToArray());
        callback?.Invoke(metadata);
    }

    private static unsafe void DecodeAudioToStream(string filename, Stream stream, out Dictionary<string, string> metadata)
    {
        var asd = new AudioStreamDecoder(filename);
        var we = new WAVEEncoder(asd, stream);

        metadata = (Dictionary<string, string>) asd.GetMetadata();
        AVFrame* decodedFrame = ffmpeg.av_frame_alloc();
        if (decodedFrame == null)
        {
            throw new InvalidOperationException("Can't allocate frame for decoding.");
        }

        decodedFrame->format = (int)asd.SampleFormat;
        decodedFrame->channel_layout = asd.ChannelLayout;
        decodedFrame->channels = asd.ChannelCount;
        decodedFrame->sample_rate = asd.SampleRate;

        AVPacket inPacket;
        ffmpeg.av_init_packet(&inPacket);
        inPacket.data = null;
        inPacket.size = 0;
        int frameFinished = 0;

        for (; ; )
        {
            if (asd.IsThereNewFrame(&inPacket))
            {
                break;
            }

            if (asd.GetDecodedFrame(&inPacket, decodedFrame, ref frameFinished) == ffmpeg.AVERROR_EOF)
            {
                break;
            }

            if (frameFinished != 0)
            {

                we.ResampleAndWrite(decodedFrame, ref frameFinished);


            }
        }
        we.EncodeFlush();
        asd.DecodeFlush(&inPacket);
        we.FinalizeStream();

        ffmpeg.av_packet_unref(&inPacket);
        ffmpeg.av_frame_unref(decodedFrame);
        ffmpeg.av_free(decodedFrame);
    }
}
