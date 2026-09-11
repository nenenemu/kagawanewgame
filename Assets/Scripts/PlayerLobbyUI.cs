using UnityEngine;
using UnityEngine.UI;
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

    [Header("キャラ名")]
    public TMP_Text characterNameText;

    [Header("ステータス画像")]
    public Image statusImage;

    [Header("キャラごとのステータス画像")]
    public Sprite[] statusImages;

    [Header("状態表示（任意）")]
    public TMP_Text stateText;


    //==================================================
    // 起動時
    //==================================================

    private void Awake()
    {
        // 起動したら必ず「未参加」に戻す
        SetState(UIState.Waiting);
    }


    //==================================================
    // UI状態変更
    //==================================================

    public void SetState(UIState state)
    {
        switch (state)
        {
            //==========================================
            // 未参加
            //==========================================

            case UIState.Waiting:

                // PRESS LEFTだけ表示
                if (pressR != null)
                    pressR.SetActive(true);

                if (characterSelect != null)
                    characterSelect.SetActive(false);

                if (readyUI != null)
                    readyUI.SetActive(false);

                if (stateText != null)
                    stateText.text = "";

                ClearCharacterUI();

                break;


            //==========================================
            // キャラ選択中
            //==========================================

            case UIState.Selecting:

                if (pressR != null)
                    pressR.SetActive(false);

                if (characterSelect != null)
                    characterSelect.SetActive(true);

                if (readyUI != null)
                    readyUI.SetActive(false);

                if (stateText != null)
                    stateText.text = "SELECT";

                break;


            //==========================================
            // READY
            //==========================================

            case UIState.Ready:

                if (pressR != null)
                    pressR.SetActive(false);

                // キャラ選択UIは消す
                if (characterSelect != null)
                    characterSelect.SetActive(false);

                if (readyUI != null)
                    readyUI.SetActive(true);

                if (stateText != null)
                    stateText.text = "READY";

                break;
        }
    }


    //==================================================
    // キャラ名変更
    //==================================================

    public void SetCharacterName(string characterName)
    {
        if (characterNameText == null)
            return;

        characterNameText.text = characterName;
    }


    //==================================================
    // ステータス画像変更
    //==================================================

    public void SetStatusImage(int eggIndex)
    {
        if (statusImage == null)
            return;

        if (statusImages == null ||
            eggIndex < 0 ||
            eggIndex >= statusImages.Length)
        {
            statusImage.sprite = null;
            statusImage.enabled = false;
            return;
        }

        statusImage.sprite = statusImages[eggIndex];

        statusImage.enabled =
            statusImages[eggIndex] != null;
    }


    //==================================================
    // 未参加状態に戻す
    //==================================================

    private void ClearCharacterUI()
    {
        if (characterNameText != null)
            characterNameText.text = "";

        if (statusImage != null)
        {
            statusImage.sprite = null;
            statusImage.enabled = false;
        }
    }
}