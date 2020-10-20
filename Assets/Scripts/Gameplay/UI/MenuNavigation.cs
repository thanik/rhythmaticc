using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class MenuNavigation : MonoBehaviour
{
    public List<GameObject> menus;
    private int currentMenuIndex = 0;
    public void navigateToMenu(int index)
    {
        menus[index].SetActive(true);
        menus[currentMenuIndex].SetActive(false);
        //menus[currentMenuIndex].GetComponent<RectTransform>().DOMove(offScreenLeftPosition, 2f).SetEase(Ease.InOutSine);
        //menus[index].SetActive(true);
        //menus[index].GetComponent<RectTransform>().DOMove(new Vector3(), 2f).SetEase(Ease.InOutSine).ChangeStartValue(offScreenRightPosition);

        currentMenuIndex = index;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
