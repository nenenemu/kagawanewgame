using UnityEngine;
using TMPro;

public class PlayerLobbyUI : MonoBehaviour
{
    public enum UIState
    {
        Waiting,
        Selecting,
        Ready
    }


    [Header("UI")]
    public GameObject pressR;
    public GameObject characterSelect;
    public GameObject readyUI;


    [Header("îCà”ÅFèÛë‘ï\é¶")]
    public TMP_Text stateText;


    public void SetState(
        UIState state
    )
    {
        if (pressR != null)
        {
            pressR.SetActive(
                state == UIState.Waiting
            );
        }


        if (characterSelect != null)
        {
            characterSelect.SetActive(
                state == UIState.Selecting
            );
        }


        if (readyUI != null)
        {
            readyUI.SetActive(
                state == UIState.Ready
            );
        }


        if (stateText != null)
        {
            switch (state)
            {
                case UIState.Waiting:

                    stateText.text =
                        "PRESS R";

                    break;


                case UIState.Selecting:

                    stateText.text =
                        "SELECT";

                    break;


                case UIState.Ready:

                    stateText.text =
                        "READY";

                    break;
            }
        }
    }
}