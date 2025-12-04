using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIButtonCardboardController : MonoBehaviour
{
    private Button currentButton;
    private bool isInitialized = false;

    public void Initialize(Button button)
    {
        currentButton = button;
        if (currentButton != null)
        {
            isInitialized = true;
        }
    }

    public void OnPointerEnter()
    {
        if (!isInitialized || currentButton == null || !currentButton.interactable) return;

        //Written to make the button use the highlighted color when needed but doesn't work
        ColorBlock colors = currentButton.colors;
        Image buttonImage = currentButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = colors.highlightedColor;
        }
    }

    public void OnPointerExit()
    {
        if (!isInitialized || currentButton == null) return;

        //Written to make the button use the normal color back but can't check if it works
        ColorBlock colors = currentButton.colors;
        Image buttonImage = currentButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = colors.normalColor;
        }
    }

    public void OnPointerClick()
    {
        if (!isInitialized || currentButton == null || !currentButton.interactable) return;

        ColorBlock colors = currentButton.colors;
        Image buttonImage = currentButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = colors.pressedColor;
        }

        currentButton.onClick.Invoke();

        Invoke("ResetButtonColor", 0.2f);
    }

    void ResetButtonColor()
    {
        if (currentButton != null)
        {
            ColorBlock colors = currentButton.colors;
            Image buttonImage = currentButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = colors.normalColor;
            }
        }
    }
}