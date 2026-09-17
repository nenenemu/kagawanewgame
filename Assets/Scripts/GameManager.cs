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

    [Tooltip("現在は使用しません。カウントダウンは必ず1秒固定です。")]
    public float countdownTime = 1f;


    [Header("展示ロビー")]
    public ExhibitionLobbyManager exhibitionLobbyManager;


    // =========================================================
    // 内部状態
    // =========================================================

    private Coroutine countdownCoroutine;


    // =========================================================
    // 初期化
    // =========================================================

    void Start()
    {
        if (playerManager == null)
        {
            playerManager =
                FindFirstObjectByType<PlayerManager>();
        }

        if (exhibitionLobbyManager == null)
        {
            exhibitionLobbyManager =
                FindFirstObjectByType<ExhibitionLobbyManager>();
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
                currentState == GameState.Countdown ||
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

                // ★ SELECT画面を残したまま
                if (selectPanel != null)
                    selectPanel.SetActive(true);

                // ★ カウントダウンだけ上から重ねる
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

    public void StartGame()
    {
        if (currentState != GameState.Select)
            return;


        if (playerManager == null)
        {
            Debug.LogError(
                "GameManager : PlayerManagerが設定されていません。"
            );

            return;
        }


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
            "===== GAME START REQUEST ====="
        );

        Debug.Log(
            "参加人数 : " +
            joinedCount
        );


        StartCountdown();
    }


    // =========================================================
    // ロビーからゲーム開始
    // =========================================================

    public void StartPlayingFromLobby()
    {
        if (currentState != GameState.Select)
            return;


        if (playerManager == null)
        {
            Debug.LogError(
                "GameManager : PlayerManagerが設定されていません。"
            );

            return;
        }


        int joinedCount = 0;

        for (int i = 0; i < playerManager.players.Length; i++)
        {
            if (playerManager.players[i].joined)
            {
                joinedCount++;
            }
        }


        if (joinedCount < 2)
        {
            Debug.LogWarning(
                "参加人数が2人未満なので開始できません。"
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


        StartCountdown();
    }


    // =========================================================
    // カウントダウン開始
    // =========================================================

    public void StartCountdown()
    {
        // 既にカウントダウン中なら二重起動しない
        if (countdownCoroutine != null)
        {
            return;
        }


        // カウントダウン表示を最初にリセット
        if (countdownText != null)
        {
            countdownText.text = "10";
        }


        ChangeState(GameState.Countdown);


        countdownCoroutine =
            StartCoroutine(
                CountdownCoroutine()
            );
    }


    // =========================================================
    // カウントダウン
    // =========================================================

    private IEnumerator CountdownCoroutine()
    {
        // ★必ず10秒
        int count = 10;


        while (count > 0)
        {
            // カウントダウン中でなくなったら終了
            if (currentState != GameState.Countdown)
            {
                countdownCoroutine = null;
                yield break;
            }


            if (countdownText != null)
            {
                countdownText.text =
                    count.ToString();
            }


            Debug.Log(
                "COUNTDOWN : " +
                count
            );


            // =================================================
            // ★重要
            // InspectorのcountdownTimeは使用しない
            // 必ず1秒待つ
            // =================================================

            yield return new WaitForSeconds(1f);


            count--;
        }


        // =====================================================
        // GO!
        // =====================================================

        if (currentState != GameState.Countdown)
        {
            countdownCoroutine = null;
            yield break;
        }


        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }


        Debug.Log(
            "===== GO! ====="
        );


        // =====================================================
        // プレイヤー生成
        // =====================================================

        if (playerManager != null)
        {
            playerManager.StartGame();
        }


        // GO!を0.5秒表示
        yield return new WaitForSeconds(0.5f);


        // =====================================================
        // プレイ開始
        // =====================================================

        if (currentState != GameState.Countdown)
        {
            countdownCoroutine = null;
            yield break;
        }


        countdownCoroutine = null;

        StartPlaying();
    }


    // =========================================================
    // ★ カウントダウンキャンセル
    // =========================================================

    public void CancelCountdown()
    {
        if (currentState != GameState.Countdown)
        {
            return;
        }


        Debug.Log(
            "===== COUNTDOWN CANCEL ====="
        );


        // コルーチン停止
        if (countdownCoroutine != null)
        {
            StopCoroutine(
                countdownCoroutine
            );

            countdownCoroutine = null;
        }


        // 数字を消す
        if (countdownText != null)
        {
            countdownText.text = "";
        }


        // SELECTへ戻す
        ChangeState(
            GameState.Select
        );
    }


    // =========================================================
    // PLAYING開始
    // =========================================================

    private void StartPlaying()
    {
        ChangeState(GameState.Playing);

        MatchScoreManager scoreManager =
            FindFirstObjectByType<MatchScoreManager>();

        if (scoreManager != null)
        {
            scoreManager.StartMatch();
        }

        MatchUI matchUI =
        FindFirstObjectByType<MatchUI>();

        if (matchUI != null)
        {
            matchUI.StartMatchUI();
        }
    }


    // =========================================================
    // ゲーム終了
    // =========================================================

    public void EndGame()
    {
        if (currentState == GameState.Result)
            return;


        Debug.Log(
            "===== GAME END ====="
        );


        ChangeState(
            GameState.Result
        );

        MatchUI matchUI =
        FindFirstObjectByType<MatchUI>();

        if (matchUI != null)
        {
            matchUI.ShowResult();
        }
    }


    // =========================================================
    // 選択画面に戻る
    // =========================================================

    public void ReturnToSelect()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(
                countdownCoroutine
            );

            countdownCoroutine = null;
        }


        // スコアと試合時間をリセット
        MatchScoreManager scoreManager =
            FindFirstObjectByType<MatchScoreManager>();

        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }


        if (playerManager != null)
        {
            playerManager.ReturnToSelect();
        }


        if (exhibitionLobbyManager != null)
        {
            exhibitionLobbyManager.ReturnToLobby();
        }


        if (countdownText != null)
        {
            countdownText.text = "";
        }


        ChangeState(
            GameState.Select
        );
    }
}