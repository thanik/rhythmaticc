using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.IO;
using UnityEngine.Networking;
using System;

public class SceneManagement : Singleton<SceneManagement>
{
    public Image blackScreen;
    public LoadingScreen loadingScreen;
    public LoadingScreen uploadingScreen;
    public ErrorDialog errorDialog;

    private SongMetadata currentSongMetadata;
    private GameConfig cfg;
    private int retriesCount = 10;
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        GameConfigLoader cfgLoader = GameConfigLoader.Instance;
        cfg = cfgLoader.GetGameConfig();
    }


    void Update()
    {
        
    }

    public void transitionToGame()
    {
        blackScreen.gameObject.SetActive(true);
        blackScreen.DOFade(1.0f, 1.0f);
        StartCoroutine(transitioningToGame());
    }

    IEnumerator transitioningToGame()
    {
        GamePreference gPre = cfg.gamePreference;
        GenerationParam genParam = cfg.genParam;

        yield return new WaitForSeconds(1.0f);
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.updateText(gPre.analyse);
        loadingScreen.StartFadeIn();
        loadingScreen.progressbarFill.fillAmount = 0f;
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        while (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(1))
        {
            yield return new WaitForEndOfFrame();
        }

        // setup everything
        SkinManager sm = FindObjectOfType<SkinManager>();
        ManiaGameController gm = FindObjectOfType<ManiaGameController>();
        BeatOnsetManager bm = FindObjectOfType<BeatOnsetManager>();
        FFmpegCaller ffmpegCaller = FindObjectOfType<FFmpegCaller>();

        currentSongMetadata = new SongMetadata("", "", 0, Path.GetFileName(gPre.lastSongPath), 0, null, 0);
        StartCoroutine(ffmpegCaller.LoadAudio(gPre.lastSongPath, gm.song, delegate (Dictionary<string, string> metadata)
        {
            //foreach (string key in metadata.Keys)
            //{
            //    Debug.Log(key + ": " + metadata[key]);
            //}

            if (metadata.ContainsKey("title"))
            {
                currentSongMetadata.songName = metadata["title"];
            }

            if (metadata.ContainsKey("artist"))
            {
                currentSongMetadata.songArtist = metadata["artist"];
            }
            currentSongMetadata.songLength = gm.song.clip.length / 2;

            if (gPre.analyse)
            {
                // analyse
                currentSongMetadata.difficulty = (Difficulty)gPre.difficulty;
                currentSongMetadata.genParam = genParam;
                bm.loadBeatsAndOnsets(gPre.lastSongPath, genParam.onsetThreshold, gm.song.clip.length);
            }
            else
            {
                loadingScreen.StartProgressbar(2f);
                DoneAnalyse();
            }
        }));

    }

    public void DoneAnalyse()
    {
        loadingScreen.FinishProgressbar();
        ManiaGameController gm = FindObjectOfType<ManiaGameController>();
        BeatOnsetManager bm = FindObjectOfType<BeatOnsetManager>();
        GamePreference gPre = cfg.gamePreference;
        currentSongMetadata.avgBPM = bm.avgBPM;
        if (gPre.analyse)
        {
            gm.generateNoteObjects();
        }
        else
        {
            gm.importBeatmap();
        }
    }

    public void showGameScreen()
    {
        StartCoroutine(showingGameScreen());
    }

    IEnumerator showingGameScreen()
    {
        yield return new WaitForEndOfFrame();
        blackScreen.DOFade(0.0f, 1.0f);
        loadingScreen.StartFadeOut();
        yield return new WaitForSeconds(1.2f);
        loadingScreen.gameObject.SetActive(false);
    }

    public void transitionToMainMenu(bool openGameMenu)
    {
        loadingScreen.StartFadeOut();
        blackScreen.gameObject.SetActive(true);
        blackScreen.DOFade(1.0f, 1.0f);
        StartCoroutine(transitioningToMainMenu(openGameMenu));
    }

    IEnumerator transitioningToMainMenu(bool openGameMenu)
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        while (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(0))
        {
            yield return new WaitForEndOfFrame();
        }
        if (openGameMenu)
        {
            FindObjectOfType<MenuNavigation>().navigateToMenu(3);
        }
        blackScreen.DOFade(0.0f, 1.0f);
        loadingScreen.gameObject.SetActive(false);
        uploadingScreen.gameObject.SetActive(false);
    }

    public void transitionToResult(bool generated, bool autoplay, int keyCount, List<JudgmentRecord> judgeRecords, List<NotePoint> notes, int maxCombo, float percentage, float analysisTime)
    {
        blackScreen.gameObject.SetActive(true);
        blackScreen.DOFade(1.0f, 1.0f);
        StartCoroutine(transitioningToResult(ResultPackDataBuilder.Build(generated, autoplay, keyCount, currentSongMetadata, judgeRecords, notes, maxCombo, percentage, analysisTime)));
    }

    IEnumerator transitioningToResult(ResultPackData resultData)
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadSceneAsync(2, LoadSceneMode.Single);
        while (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(2))
        {
            yield return new WaitForEndOfFrame();
        }
        FindObjectOfType<ResultPanel>().showResult(resultData);
        blackScreen.DOFade(0.0f, 1.0f);

    }

    public void transitionToCalibration()
    {
        blackScreen.gameObject.SetActive(true);
        blackScreen.DOFade(1.0f, 1.0f);
        StartCoroutine(transitioningToCalibration());
    }

    IEnumerator transitioningToCalibration()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadSceneAsync(3, LoadSceneMode.Single);
        while (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(3))
        {
            yield return new WaitForEndOfFrame();
        }
        blackScreen.DOFade(0.0f, 1.0f);
    }
    
    public void finishUploading()
    {
        uploadingScreen.FinishProgressbar();
        uploadingScreen.StartFadeOut();
        transitionToMainMenu(true);
    }

    public void ShowErrorDialog(string errorMsg)
    {
        errorDialog.gameObject.SetActive(true);
        errorDialog.errorText.text = errorMsg;
    }

    public void uploadSurveyData(string postData)
    {
        StartCoroutine(Upload(postData));
    }

    IEnumerator Upload(string postData)
    {
        blackScreen.gameObject.SetActive(true);
        blackScreen.DOFade(1.0f, 1.0f);
        uploadingScreen.gameObject.SetActive(true);
        uploadingScreen.StartFadeIn();
        uploadingScreen.progressbarFill.fillAmount = 0f;
        yield return new WaitForSeconds(1f);
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("payload", postData));
        if (!string.IsNullOrEmpty(cfg.machineID))
        {
            formData.Add(new MultipartFormDataSection("machineID", cfg.machineID));
        }

        int retries = 0;
        while (retries < retriesCount)
        {
            retries++;
            SceneManagement.Instance.uploadingScreen.progressbarFill.fillAmount = (float)retries / (retriesCount + 1);
            UnityWebRequest www = UnityWebRequest.Post("https://rhythmati.cc/api/v1/postPlayData", formData);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    APIResponse res = JsonUtility.FromJson<APIResponse>(www.downloadHandler.text);
                    if (!string.IsNullOrEmpty(res.machineID))
                    {
                        cfg.machineID = res.machineID;
                        GameConfigLoader.Instance.saveConfig();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                SceneManagement.Instance.finishUploading();
                break;
            }
            else if (retries == retriesCount)
            {

                SceneManagement.Instance.ShowErrorDialog("There's an error while sending survey data.\n" + www.error);
            }
        }
    }

}
