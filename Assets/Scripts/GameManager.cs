using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // =========================================================
    // ゲーム状態
    // =========================================================

    public enum GameState
    {
        Select,
        Countdown,
        Playing,
        Result
    }


    [Header("現在のゲーム状態")]
    public GameState currentState = GameState.Select;


    // =========================================================
    // 各Manager
    // =========================================================

    [Header("Manager")]
    public PlayerManager playerManager;


    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    public GameObject selectPanel;
    public GameObject countdownPanel;
    public GameObject resultPanel;


    [Header("メインカメラ")]
    public Camera mainCamera;

    [Header("カウントダウン")]
    public TMP_Text countdownText;

    [Tooltip("3 → 2 → 1 の1つあたりの秒数")]
    public float countdownTime = 1f;


    // =========================================================
    // 初期化
    // =========================================================

    void Start()
    {
        // PlayerManagerが設定されていなければ探す
        if (playerManager == null)
        {
            playerManager =
                FindFirstObjectByType<PlayerManager>();
        }

        ChangeState(GameState.Select);
    }


    // =========================================================
    // 状態変更
    // =========================================================

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        Debug.Log(
            "GameState : " +
            currentState
        );


        // =====================================================
        // UIを全部OFF
        // =====================================================

        if (selectPanel != null)
            selectPanel.SetActive(false);

        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);


        // =====================================================
        // カメラ
        // =====================================================

        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(
                currentState == GameState.Select ||
                currentState == GameState.Result
            );
        }


        // =====================================================
        // 状態ごとの処理
        // =====================================================

        switch (currentState)
        {
            case GameState.Select:

                if (selectPanel != null)
                    selectPanel.SetActive(true);

                break;


            case GameState.Countdown:

                if (countdownPanel != null)
                    countdownPanel.SetActive(true);

                break;


            case GameState.Playing:

                break;


            case GameState.Result:

                if (resultPanel != null)
                    resultPanel.SetActive(true);

                break;
        }
    }


    // =========================================================
    // 従来のSTARTボタン用
    // =========================================================
    // 今回の展示では基本的に使わない。
    // 残しておいてもOK。
    // =========================================================

    public void StartGame()
    {
        // Select状態以外では開始しない
        if (currentState != GameState.Select)
            return;


        // PlayerManager確認
        if (playerManager == null)
        {
            Debug.LogError(
                "GameManager : PlayerManagerが設定されていません。"
            );

            return;
        }


        // -----------------------------------------------------
        // 参加人数確認
        // -----------------------------------------------------

        int joinedCount = 0;

        for (int i = 0; i < playerManager.players.Length; i++)
        {
            if (playerManager.players[i].joined)
            {
                joinedCount++;
            }
        }


        // 参加者0人なら開始しない
        if (joinedCount == 0)
        {
            Debug.LogWarning(
                "参加プレイヤーがいないため、ゲームを開始できません。"
            );

            return;
        }


        Debug.Log(
            "===== GAME START REQUEST ====="
        );

        Debug.Log(
            "参加人数 : " +
            joinedCount
        );


        // カウントダウン開始
        StartCoroutine(
            CountdownCoroutine()
        );
    }


    // =========================================================
    // ロビーから呼ばれるゲーム開始
    // =========================================================

    public void StartPlayingFromLobby()
    {
        // すでにゲーム中なら何もしない
        if (currentState != GameState.Select)
            return;


        // PlayerManager確認
        if (playerManager == null)
        {
            Debug.LogError(
                "GameManager : PlayerManagerが設定されていません。"
            );

            return;
        }


        // -----------------------------------------------------
        // 参加人数確認
        // -----------------------------------------------------

        int joinedCount = 0;

        for (int i = 0; i < playerManager.players.Length; i++)
        {
            if (playerManager.players[i].joined)
            {
                joinedCount++;
            }
        }


        if (joinedCount == 0)
        {
            Debug.LogWarning(
                "参加プレイヤーがいないため、ゲームを開始できません。"
            );

            return;
        }


        Debug.Log(
            "===== LOBBY TIMER END ====="
        );

        Debug.Log(
            "参加人数 : " +
            joinedCount
        );


        // -----------------------------------------------------
        // 3 → 2 → 1 → GO
        // -----------------------------------------------------

        StartCoroutine(
            CountdownCoroutine()
        );
    }


    // =========================================================
    // カウントダウン
    // =========================================================

    IEnumerator CountdownCoroutine()
    {
        ChangeState(GameState.Countdown);


        // -----------------------------------------------------
        // 3
        // -----------------------------------------------------

        if (countdownText != null)
        {
            countdownText.text = "3";
        }

        yield return new WaitForSeconds(
            countdownTime
        );


        // -----------------------------------------------------
        // 2
        // -----------------------------------------------------

        if (countdownText != null)
        {
            countdownText.text = "2";
        }

        yield return new WaitForSeconds(
            countdownTime
        );


        // -----------------------------------------------------
        // 1
        // -----------------------------------------------------

        if (countdownText != null)
        {
            countdownText.text = "1";
        }

        yield return new WaitForSeconds(
            countdownTime
        );


        // -----------------------------------------------------
        // GO!
        // -----------------------------------------------------

        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }


        // -----------------------------------------------------
        // ここで卵を生成
        // -----------------------------------------------------

        if (playerManager != null)
        {
            playerManager.StartGame();
        }


        yield return new WaitForSeconds(
            0.5f
        );


        // -----------------------------------------------------
        // ゲーム開始
        // -----------------------------------------------------

        StartPlaying();
    }


    // =========================================================
    // PLAYING開始
    // =========================================================

    public void StartPlaying()
    {
        Debug.Log(
            "===== PLAYING START ====="
        );


        ChangeState(
            GameState.Playing
        );
    }


    // =========================================================
    // ゲーム終了
    // =========================================================

    public void EndGame()
    {
        // 既にResultなら何もしない
        if (currentState == GameState.Result)
            return;


        Debug.Log(
            "===== GAME END ====="
        );


        ChangeState(
            GameState.Result
        );
    }


    // =========================================================
    // 選択画面に戻る
    // =========================================================

    public void ReturnToSelect()
    {
        Debug.Log(
            "===== RETURN TO SELECT ====="
        );


        // -----------------------------------------------------
        // 実行中のカウントダウンを止める
        // -----------------------------------------------------

        StopAllCoroutines();


        // -----------------------------------------------------
        // PlayerManagerをリセット
        // -----------------------------------------------------

        if (playerManager != null)
        {
            playerManager.ReturnToSelect();
        }


        // -----------------------------------------------------
        // カウントダウン文字を消す
        // -----------------------------------------------------

        if (countdownText != null)
        {
            countdownText.text = "";
        }


        // -----------------------------------------------------
        // 選択画面へ
        // -----------------------------------------------------

        ChangeState(
            GameState.Select
        );
    }
}