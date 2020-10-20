using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class PostBuild
{
    [PostProcessBuildAttribute(0)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        string buildDirPath = Path.GetDirectoryName(pathToBuiltProject);
        if (File.Exists(buildDirPath + @"\UserData\config.json"))
        {
            File.Delete(buildDirPath + @"\UserData\config.json");
        }
        if (File.Exists(buildDirPath + @"\UserData\genReport.log"))
        {
            File.Delete(buildDirPath + @"\UserData\genReport.log");
        }
        if (File.Exists(buildDirPath + @"\UserData\latestSurvey.json"))
        {
            File.Delete(buildDirPath + @"\UserData\latestSurvey.json");
        }
        if (File.Exists(buildDirPath + @"\UserData\timeDiffLog.log"))
        {
            File.Delete(buildDirPath + @"\UserData\timeDiffLog.log");
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(buildDirPath + @"\UserData\BeatCache");
        foreach (System.IO.FileInfo file in directoryInfo.GetFiles()) file.Delete();
        foreach (System.IO.DirectoryInfo subDirectory in directoryInfo.GetDirectories()) subDirectory.Delete(true);
    }
}
