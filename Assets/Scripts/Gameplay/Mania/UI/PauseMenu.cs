using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public List<Button> buttons;
    public RectTransform selector;
    public int selectedIndex;

    public TMP_Text speedModText;

    private ManiaGameController gc;
    public void ResetMenu()
    {
        selectedIndex = 0;
        selector.anchoredPosition = new Vector2(0, buttons[selectedIndex].GetComponent<RectTransform>().anchoredPosition.y);
    }
    void Start()
    {
        gc = FindObjectOfType<ManiaGameController>();
        selector.anchoredPosition = new Vector2(0, buttons[selectedIndex].GetComponent<RectTransform>().anchoredPosition.y);
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.leftArrowKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame))
        {
            gc.decreaseSpeedMod();
            speedModText.text = gc.speedMod.ToString("0.00");
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame))
        {
            gc.increaseSpeedMod();
            speedModText.text = gc.speedMod.ToString("0.00");
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame))
        {
            if (selectedIndex > 0)
            {
                selectedIndex--;
            }
            selector.DOAnchorPos(new Vector2(0, buttons[selectedIndex].GetComponent<RectTransform>().anchoredPosition.y), 0.5f);
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.dpad.down.wasPressedThisFrame))
        {
            if (selectedIndex < buttons.Count - 1)
            {
                selectedIndex++;
            }
            selector.DOAnchorPos(new Vector2(0, buttons[selectedIndex].GetComponent<RectTransform>().anchoredPosition.y), 0.5f);
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame || (Gamepad.current != null && (Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame)))
        {
            buttons[selectedIndex].onClick.Invoke();
        }
    }

    public void backToMainMenu()
    {
        gameObject.SetActive(false);
        SceneManagement.Instance.transitionToMainMenu(false);
    }
}
