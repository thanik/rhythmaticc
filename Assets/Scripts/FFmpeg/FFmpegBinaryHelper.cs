using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class FFmpegBinaryHelper
{
    internal static void RegisterFFmpegBinaries()
    {
        var current = Environment.CurrentDirectory;
        var probe = Path.Combine("FFmpeg");
        while (current != null)
        {
            var ffmpegBinaryPath = Path.Combine(current, probe);
            if (Directory.Exists(ffmpegBinaryPath))
            {
                Debug.Log($"FFmpeg binaries found in: {ffmpegBinaryPath}");
                ffmpeg.RootPath = ffmpegBinaryPath;
                return;
            }

            current = Directory.GetParent(current)?.FullName;
        }
    }
}
