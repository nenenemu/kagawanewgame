using System.Collections;
using UnityEngine;
using TMPro;

public class ExhibitionLobbyManager : MonoBehaviour
{
    // =========================================================
    // ロビー状態
    // =========================================================

    public enum LobbyState
    {
        Waiting,
        Countdown
    }


    // =========================================================
    // プレイヤー
    // =========================================================

    [System.Serializable]
    public class LobbyPlayer
    {
        public PlayerLobbyUI ui;

        [HideInInspector]
        public LobbyPlayerState state = LobbyPlayerState.Waiting;

        [HideInInspector]
        public bool joined = false;

        [HideInInspector]
        public bool ready = false;
    }


    public enum LobbyPlayerState
    {
        Waiting,
        Selecting,
        Ready
    }


    // =========================================================
    // プレイヤー設定
    // =========================================================

    [Header("プレイヤー")]
    public LobbyPlayer[] players = new LobbyPlayer[4];


    // =========================================================
    // READYカウントダウン
    // =========================================================

    [Header("READYカウントダウン")]
    [Tooltip("全員READYになってからゲーム開始までの秒数")]
    public float readyCountdownTime = 10f;


    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    public TMP_Text timerText;


    // =========================================================
    // ゲーム管理
    // =========================================================

    [Header("ゲーム管理")]
    public GameManager gameManager;


    // =========================================================
    // プレイヤー管理
    // =========================================================

    [Header("プレイヤー管理")]
    public PlayerManager playerManager;


    // =========================================================
    // 仮の卵
    // =========================================================

    [Header("仮の卵")]
    [Tooltip("キャラ選択がまだ未実装の間に使う卵")]
    public EggData defaultEgg;


    // =========================================================
    // 内部状態
    // =========================================================

    private LobbyState currentState;

    private bool gameStarted = false;

    private bool countdownRunning = false;

    private Coroutine countdownCoroutine;


    // =========================================================
    // 初期化
    // =========================================================

    void Start()
    {
        currentState = LobbyState.Waiting;

        gameStarted = false;

        countdownRunning = false;

        SetupInitialUI();

        ClearTimerUI();
    }


    // =========================================================
    // 初期UI
    // =========================================================

    void SetupInitialUI()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].state =
                LobbyPlayerState.Waiting;

            players[i].joined = false;

            players[i].ready = false;


            if (players[i].ui != null)
            {
                players[i].ui.SetState(
                    PlayerLobbyUI.UIState.Waiting
                );
            }
        }
    }


    // =========================================================
    // 右クリック
    // 未参加 → 参加
    // =========================================================

    public void OnPlayerMouseRightClick(int playerIndex)
    {
        if (gameStarted)
            return;

        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return;


        LobbyPlayer player =
            players[playerIndex];


        // -----------------------------------------------------
        // まだ参加していない場合だけ参加
        // -----------------------------------------------------

        if (!player.joined)
        {
            JoinPlayer(playerIndex);

            return;
        }


        // -----------------------------------------------------
        // すでに参加している場合
        // -----------------------------------------------------
        // 右クリックでは何もしない

        Debug.Log(
            "P" + (playerIndex + 1) +
            " : すでに参加済み"
        );
    }


    // =========================================================
    // 左クリック
    // SELECT ↔ READY
    // =========================================================

    public void OnPlayerMouseLeftClick(int playerIndex)
    {
        if (gameStarted)
            return;

        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return;


        LobbyPlayer player =
            players[playerIndex];


        // -----------------------------------------------------
        // 未参加なら何もしない
        // -----------------------------------------------------

        if (!player.joined)
        {
            Debug.Log(
                "P" + (playerIndex + 1) +
                " : 未参加なのでLEFT CLICK無効"
            );

            return;
        }


        // -----------------------------------------------------
        // SELECTING → READY
        // -----------------------------------------------------

        if (player.state ==
            LobbyPlayerState.Selecting)
        {
            SetReady(playerIndex);

            return;
        }


        // -----------------------------------------------------
        // READY → SELECTING
        // -----------------------------------------------------

        if (player.state ==
            LobbyPlayerState.Ready)
        {
            CancelReady(playerIndex);

            return;
        }
    }


    // =========================================================
    // プレイヤー参加
    // =========================================================

    void JoinPlayer(int playerIndex)
    {
        LobbyPlayer player =
            players[playerIndex];


        player.joined = true;

        player.ready = false;

        player.state =
            LobbyPlayerState.Selecting;


        // -----------------------------------------------------
        // PlayerManager
        // -----------------------------------------------------

        if (playerManager != null)
        {
            playerManager.SetPlayerJoined(
                playerIndex,
                true
            );


            if (defaultEgg != null)
            {
                playerManager.SetPlayerEgg(
                    playerIndex,
                    defaultEgg
                );
            }
        }


        // -----------------------------------------------------
        // UI
        // -----------------------------------------------------

        if (player.ui != null)
        {
            player.ui.SetState(
                PlayerLobbyUI.UIState.Selecting
            );
        }


        Debug.Log(
            "P" + (playerIndex + 1) +
            " : 参加"
        );


        // -----------------------------------------------------
        // カウントダウン中ならキャンセル
        // -----------------------------------------------------

        CancelCountdownIfRunning();


        // -----------------------------------------------------
        // READY条件チェック
        // -----------------------------------------------------

        CheckStartCondition();
    }


    // =========================================================
    // READY
    // =========================================================

    void SetReady(int playerIndex)
    {
        LobbyPlayer player =
            players[playerIndex];


        player.ready = true;

        player.state =
            LobbyPlayerState.Ready;


        if (player.ui != null)
        {
            player.ui.SetState(
                PlayerLobbyUI.UIState.Ready
            );
        }


        Debug.Log(
            "P" + (playerIndex + 1) +
            " : READY"
        );


        // -----------------------------------------------------
        // READY条件チェック
        // -----------------------------------------------------

        CheckStartCondition();
    }


    // =========================================================
    // READY解除
    // =========================================================

    void CancelReady(int playerIndex)
    {
        LobbyPlayer player =
            players[playerIndex];


        player.ready = false;

        player.state =
            LobbyPlayerState.Selecting;


        if (player.ui != null)
        {
            player.ui.SetState(
                PlayerLobbyUI.UIState.Selecting
            );
        }


        Debug.Log(
            "P" + (playerIndex + 1) +
            " : READY解除"
        );


        // -----------------------------------------------------
        // カウントダウン中なら即キャンセル
        // -----------------------------------------------------

        CancelCountdownIfRunning();


        // -----------------------------------------------------
        // 条件チェック
        // -----------------------------------------------------

        CheckStartCondition();
    }


    // =========================================================
    // ゲーム開始条件チェック
    // =========================================================

    void CheckStartCondition()
    {
        if (gameStarted)
            return;


        int joinedCount = 0;

        bool allReady = true;


        // -----------------------------------------------------
        // 参加人数とREADY状態を確認
        // -----------------------------------------------------

        for (int i = 0; i < players.Length; i++)
        {
            LobbyPlayer player =
                players[i];


            if (!player.joined)
                continue;


            joinedCount++;


            if (!player.ready)
            {
                allReady = false;
            }
        }


        Debug.Log(
            "参加人数 : " +
            joinedCount +
            " / 全員READY : " +
            allReady
        );


        // -----------------------------------------------------
        // 2人未満
        // -----------------------------------------------------

        if (joinedCount < 2)
        {
            return;
        }


        // -----------------------------------------------------
        // 全員READYではない
        // -----------------------------------------------------

        if (!allReady)
        {
            return;
        }


        // -----------------------------------------------------
        // すでにカウントダウン中
        // -----------------------------------------------------

        if (countdownRunning)
        {
            return;
        }


        // -----------------------------------------------------
        // 条件成立
        // -----------------------------------------------------

        StartReadyCountdown();
    }


    // =========================================================
    // 10秒カウントダウン開始
    // =========================================================

    void StartReadyCountdown()
    {
        if (gameStarted)
            return;

        if (countdownRunning)
            return;


        countdownRunning = true;

        currentState =
            LobbyState.Countdown;


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "===== READY COUNTDOWN START ====="
        );

        Debug.Log(
            "10秒後にゲーム開始"
        );


        countdownCoroutine =
            StartCoroutine(
                ReadyCountdownCoroutine()
            );
    }


    // =========================================================
    // READYカウントダウン
    // =========================================================

    IEnumerator ReadyCountdownCoroutine()
    {
        float remaining =
            readyCountdownTime;


        while (remaining > 0f)
        {
            // -------------------------------------------------
            // 毎フレーム、開始条件が維持されているか確認
            // -------------------------------------------------

            if (!IsStartConditionValid())
            {
                CancelReadyCountdown();

                yield break;
            }


            // -------------------------------------------------
            // UI
            // -------------------------------------------------

            if (timerText != null)
            {
                int seconds =
                    Mathf.CeilToInt(
                        remaining
                    );

                timerText.text =
                    seconds.ToString();
            }


            yield return null;


            remaining -=
                Time.deltaTime;
        }


        // -----------------------------------------------------
        // 0秒
        // -----------------------------------------------------

        if (!IsStartConditionValid())
        {
            CancelReadyCountdown();

            yield break;
        }


        if (timerText != null)
        {
            timerText.text = "0";
        }


        // -----------------------------------------------------
        // ゲーム開始
        // -----------------------------------------------------

        Debug.Log(
            "===== READY COUNTDOWN END ====="
        );


        countdownRunning = false;

        currentState =
            LobbyState.Waiting;


        gameStarted = true;


        // -----------------------------------------------------
        // PlayerManagerへ最終反映
        // -----------------------------------------------------

        ApplyPlayersToPlayerManager();


        // -----------------------------------------------------
        // GameManagerへ
        // -----------------------------------------------------

        if (gameManager != null)
        {
            gameManager.StartPlayingFromLobby();
        }
        else
        {
            Debug.LogError(
                "ExhibitionLobbyManager : " +
                "GameManagerが設定されていません。"
            );
        }
    }


    // =========================================================
    // 開始条件がまだ有効か
    // =========================================================

    bool IsStartConditionValid()
    {
        int joinedCount = 0;


        for (int i = 0; i < players.Length; i++)
        {
            LobbyPlayer player =
                players[i];


            if (!player.joined)
                continue;


            joinedCount++;


            if (!player.ready)
            {
                return false;
            }
        }


        // 2人以上必要
        if (joinedCount < 2)
        {
            return false;
        }


        return true;
    }


    // =========================================================
    // カウントダウンキャンセル
    // =========================================================

    void CancelReadyCountdown()
    {
        if (!countdownRunning)
            return;


        Debug.Log(
            "===== READY COUNTDOWN CANCEL ====="
        );


        if (countdownCoroutine != null)
        {
            StopCoroutine(
                countdownCoroutine
            );

            countdownCoroutine = null;
        }


        countdownRunning = false;


        currentState =
            LobbyState.Waiting;


        ClearTimerUI();
    }


    // =========================================================
    // カウントダウン中ならキャンセル
    // =========================================================

    void CancelCountdownIfRunning()
    {
        if (!countdownRunning)
            return;


        CancelReadyCountdown();
    }


    // =========================================================
    // PlayerManagerへ反映
    // =========================================================

    void ApplyPlayersToPlayerManager()
    {
        if (playerManager == null)
            return;


        for (int i = 0; i < players.Length; i++)
        {
            LobbyPlayer player =
                players[i];


            bool active =
                player.joined &&
                player.ready;


            playerManager.SetPlayerJoined(
                i,
                active
            );


            if (active &&
                defaultEgg != null)
            {
                playerManager.SetPlayerEgg(
                    i,
                    defaultEgg
                );
            }
        }
    }


    // =========================================================
    // タイマーUI消去
    // =========================================================

    void ClearTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "";
        }
    }


    // =========================================================
    // ゲーム開始済みか
    // =========================================================

    public bool IsGameStarted()
    {
        return gameStarted;
    }


    // =========================================================
    // ロビー中か
    // =========================================================

    public bool IsLobbyActive()
    {
        return !gameStarted;
    }


    // =========================================================
    // 参加確認
    // =========================================================

    public bool IsPlayerJoined(int playerIndex)
    {
        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return false;


        return players[playerIndex].joined;
    }


    // =========================================================
    // READY確認
    // =========================================================

    public bool IsPlayerReady(int playerIndex)
    {
        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return false;


        return players[playerIndex].ready;
    }


    // =========================================================
    // 選択画面へ戻る
    // =========================================================

    public void ReturnToLobby()
    {
        Debug.Log(
            "===== RETURN TO LOBBY ====="
        );


        // -----------------------------------------------------
        // カウントダウン停止
        // -----------------------------------------------------

        if (countdownCoroutine != null)
        {
            StopCoroutine(
                countdownCoroutine
            );

            countdownCoroutine = null;
        }


        countdownRunning = false;

        gameStarted = false;

        currentState =
            LobbyState.Waiting;


        // -----------------------------------------------------
        // プレイヤーリセット
        // -----------------------------------------------------

        for (int i = 0; i < players.Length; i++)
        {
            players[i].joined = false;

            players[i].ready = false;

            players[i].state =
                LobbyPlayerState.Waiting;


            if (players[i].ui != null)
            {
                players[i].ui.SetState(
                    PlayerLobbyUI.UIState.Waiting
                );
            }
        }


        // -----------------------------------------------------
        // PlayerManager
        // -----------------------------------------------------

        if (playerManager != null)
        {
            playerManager.ReturnToSelect();
        }


        // -----------------------------------------------------
        // UI
        // -----------------------------------------------------

        ClearTimerUI();
    }
}