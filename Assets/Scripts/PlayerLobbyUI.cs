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

    [Header("キャラ画像")]
    public Image characterImage;

    [Header("キャラごとの画像")]
    public Sprite[] characterImages;

    [Header("ステータス画像")]
    public Image statusImage;

    [Header("キャラごとのステータス画像")]
    public Sprite[] statusImages;

    [Header("状態表示（任意）")]
    public TMP_Text stateText;

    [Header("選択画面に入ったときにアルファを変更する画像")]
    public Image selectedImage;

    [Header("エントリー時に表示する固定画像")]
    public Image entryImage;


    //==================================================
    // 起動時
    //==================================================

    private void Awake()
    {
        // 起動時はアルファ60%
        SetImageOpacity(0.6f);

        // 起動したら必ず「未参加」に戻す
        SetState(UIState.Waiting);
    }


    //==================================================
    // 画像のアルファ変更
    //==================================================

    private void SetImageOpacity(float opacity)
    {
        if (selectedImage == null)
            return;

        Color color = selectedImage.color;

        color.a = opacity;

        selectedImage.color = color;
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

                // PRESS Rだけ表示
                if (pressR != null)
                    pressR.SetActive(true);

                if (characterSelect != null)
                    characterSelect.SetActive(false);

                if (readyUI != null)
                    readyUI.SetActive(false);

                if (entryImage != null)
                    entryImage.gameObject.SetActive(false);

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

                if (entryImage != null)
                    entryImage.gameObject.SetActive(true);

                if (stateText != null)
                    stateText.text = "SELECT";


                // ★ キャラ選択画面に入った瞬間
                // ★ アルファを255（100%）にする
                SetImageOpacity(1.0f);

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

                if (entryImage != null)
                    entryImage.gameObject.SetActive(true);

                if (stateText != null)
                    stateText.text = "READY";

                break;
        }
    }


    //==================================================
    // キャラ名変更
    //==================================================

    //==================================================
    // キャラ画像変更
    //==================================================

    public void SetCharacterImage(int eggIndex)
    {
        if (characterImage == null)
            return;

        if (characterImages == null ||
            eggIndex < 0 ||
            eggIndex >= characterImages.Length)
        {
            characterImage.sprite = null;
            characterImage.enabled = false;
            return;
        }

        characterImage.sprite =
            characterImages[eggIndex];

        characterImage.enabled =
            characterImages[eggIndex] != null;
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
        if (characterImage != null)
        {
            characterImage.sprite = null;
            characterImage.enabled = false;
        }

        if (statusImage != null)
        {
            statusImage.sprite = null;
            statusImage.enabled = false;
        }
    }
}