using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenreEntry : MonoBehaviour
{
    public string text;
    public bool isSearchResult;
    public Button button;
    public TMP_Text txtEntry;

    private GenreSurveyController gCtrl;
    void Start()
    {
        gCtrl = GetComponentInParent<GenreSurveyController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateText(string text)
    {
        this.text = text;
        txtEntry.text = text;
    }

    public void addGenre()
    {
        gCtrl.AddGenre(text);
    }

    public void removeGenre()
    {
        gCtrl.RemoveGenre(text); 
    }
}
