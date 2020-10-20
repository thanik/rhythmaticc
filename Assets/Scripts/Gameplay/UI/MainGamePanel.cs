using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleFileBrowser;
using System;
using UnityEngine.SceneManagement;

public class MainGamePanel : MonoBehaviour
{
    private FFmpegCaller ffmpegCaller;
    private GameConfigLoader cfgLoader;
    private void Awake()
    {
        ffmpegCaller = FindObjectOfType<FFmpegCaller>();
        cfgLoader = FindObjectOfType<GameConfigLoader>();
    }

    public void btnExit()
    {
        // reset some game pref
        cfgLoader.saveConfig();
        Application.Quit();
    }

    //public IEnumerator ShowLoadDialogCoroutine()
    //{
    //    yield return FileBrowser.WaitForLoadDialog(false, false, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Select Music", "Select");
    //    if (FileBrowser.Success)
    //    {
    //        SceneManager.LoadScene("ManiaGame", LoadSceneMode.Additive);
    //        byte[] SoundFile = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
    //        yield return SoundFile;
    //        FindObjectOfType<BeatOnsetManager>().loadBeatsAndOnsets(FileBrowser.Result[0]);
    //        StartCoroutine(ffmpegCaller.LoadAudio(FileBrowser.Result[0], FindObjectOfType<ManiaGameController>().song, delegate () {
    //            //isAudioLoaded = true;
    //            GameObject.Find("MainMenu").SetActive(false);
    //        }));
            
    //    }
    //}
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
