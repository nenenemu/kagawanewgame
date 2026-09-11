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
        public LobbyPlayerState state =
            LobbyPlayerState.Waiting;

        [HideInInspector]
        public bool joined = false;

        [HideInInspector]
        public bool ready = false;

        // 現在選択しているキャラクター番号
        [HideInInspector]
        public int selectedEggIndex = 0;
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
    public LobbyPlayer[] players =
        new LobbyPlayer[4];


    // =========================================================
    // キャラクター
    // =========================================================

    [Header("キャラクター")]
    [Tooltip("右クリックで順番に切り替えるEggData")]
    public EggData[] selectableEggs;


    [Tooltip("キャラクターが未設定の場合に使用")]
    public EggData defaultEgg;


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
    // 内部状態
    // =========================================================

    private LobbyState currentState;

    private bool gameStarted = false;

    private bool countdownRunning = false;

    private Coroutine countdownCoroutine;


    // =========================================================
    // Start
    // =========================================================

    void Start()
    {
        currentState =
            LobbyState.Waiting;

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

            players[i].selectedEggIndex = 0;


            if (players[i].ui != null)
            {
                players[i].ui.SetState(
                    PlayerLobbyUI.UIState.Waiting
                );
            }
        }
    }


    // =========================================================
    // 左クリック
    //
    // 未参加       → 参加
    // SELECTING   → READY
    // READY       → SELECTING
    // =========================================================

    public void OnPlayerMouseLeftClick(
        int playerIndex
    )
    {
        if (gameStarted)
            return;


        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return;


        LobbyPlayer player =
            players[playerIndex];


        // =====================================================
        // 未参加 → 参加
        // =====================================================

        if (!player.joined)
        {
            Debug.Log(
                "P" +
                (playerIndex + 1) +
                " : LEFT CLICK → 参加"
            );


            JoinPlayer(playerIndex);

            return;
        }


        // =====================================================
        // SELECTING → READY
        // =====================================================

        if (player.state ==
            LobbyPlayerState.Selecting)
        {
            SetReady(playerIndex);

            return;
        }


        // =====================================================
        // READY → SELECTING
        // =====================================================

        if (player.state ==
            LobbyPlayerState.Ready)
        {
            CancelReady(playerIndex);

            return;
        }
    }


    // =========================================================
    // 右クリック
    //
    // 未参加 → 無視
    // 参加済み → キャラクター変更
    // =========================================================

    public void OnPlayerMouseRightClick(
        int playerIndex
    )
    {
        if (gameStarted)
            return;


        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return;


        LobbyPlayer player =
            players[playerIndex];


        // =====================================================
        // 未参加
        // =====================================================

        if (!player.joined)
        {
            Debug.Log(
                "P" +
                (playerIndex + 1) +
                " : 未参加なのでRIGHT CLICK無効"
            );

            return;
        }


        // =====================================================
        // キャラクター変更
        // =====================================================

        ChangeCharacter(playerIndex);
    }


    // =========================================================
    // キャラクター変更
    // =========================================================

    void ChangeCharacter(int playerIndex)
    {
        LobbyPlayer player =
            players[playerIndex];


        // -----------------------------------------------------
        // キャラクターが登録されていない
        // -----------------------------------------------------

        if (selectableEggs == null ||
            selectableEggs.Length == 0)
        {
            Debug.LogWarning(
                "P" +
                (playerIndex + 1) +
                " : selectableEggsが設定されていません。"
            );

            return;
        }


        // -----------------------------------------------------
        // 次のキャラクターへ
        // -----------------------------------------------------

        player.selectedEggIndex++;


        if (player.selectedEggIndex >=
            selectableEggs.Length)
        {
            player.selectedEggIndex = 0;
        }


        EggData selectedEgg =
            selectableEggs[
                player.selectedEggIndex
            ];


        if (selectedEgg == null)
        {
            Debug.LogWarning(
                "P" +
                (playerIndex + 1) +
                " : 選択されたEggDataがnullです。"
            );

            return;
        }


        // -----------------------------------------------------
        // PlayerManagerへ反映
        // -----------------------------------------------------

        if (playerManager != null)
        {
            playerManager.SetPlayerEgg(
                playerIndex,
                selectedEgg
            );
        }


        Debug.Log(
            "P" +
            (playerIndex + 1) +
            " : キャラクター変更 → " +
            selectedEgg.name
        );


        // -----------------------------------------------------
        // キャラクター変更したらREADY解除
        // -----------------------------------------------------

        if (player.state ==
            LobbyPlayerState.Ready)
        {
            player.ready = false;

            player.state =
                LobbyPlayerState.Selecting;


            if (player.ui != null)
            {
                player.ui.SetState(
                    PlayerLobbyUI.UIState.Selecting
                );
            }


            CancelCountdownIfRunning();
        }


        CheckStartCondition();
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

        player.selectedEggIndex = 0;


        // =====================================================
        // PlayerManager
        // =====================================================

        if (playerManager != null)
        {
            playerManager.SetPlayerJoined(
                playerIndex,
                true
            );


            EggData firstEgg =
                GetFirstEgg();


            if (firstEgg != null)
            {
                playerManager.SetPlayerEgg(
                    playerIndex,
                    firstEgg
                );
            }
        }


        // =====================================================
        // UI
        // =====================================================

        if (player.ui != null)
        {
            player.ui.SetState(
                PlayerLobbyUI.UIState.Selecting
            );
        }


        Debug.Log(
            "P" +
            (playerIndex + 1) +
            " : 参加"
        );


        CancelCountdownIfRunning();

        CheckStartCondition();
    }


    // =========================================================
    // 最初のEggData
    // =========================================================

    EggData GetFirstEgg()
    {
        if (selectableEggs != null &&
            selectableEggs.Length > 0)
        {
            if (selectableEggs[0] != null)
                return selectableEggs[0];
        }


        return defaultEgg;
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
            "P" +
            (playerIndex + 1) +
            " : READY"
        );


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
            "P" +
            (playerIndex + 1) +
            " : READY解除"
        );


        CancelCountdownIfRunning();

        CheckStartCondition();
    }


    // =========================================================
    // ゲーム開始条件
    // =========================================================

    void CheckStartCondition()
    {
        if (gameStarted)
            return;


        int joinedCount = 0;

        bool allReady = true;


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


        // 2人未満
        if (joinedCount < 2)
        {
            return;
        }


        // 全員READYではない
        if (!allReady)
        {
            return;
        }


        // すでにカウントダウン中
        if (countdownRunning)
        {
            return;
        }


        StartReadyCountdown();
    }


    // =========================================================
    // READYカウントダウン開始
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
            if (!IsStartConditionValid())
            {
                CancelReadyCountdown();

                yield break;
            }


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


        if (!IsStartConditionValid())
        {
            CancelReadyCountdown();

            yield break;
        }


        if (timerText != null)
        {
            timerText.text = "0";
        }


        Debug.Log(
            "===== READY COUNTDOWN END ====="
        );


        countdownRunning = false;

        currentState =
            LobbyState.Waiting;

        gameStarted = true;


        ApplyPlayersToPlayerManager();


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
    // 開始条件確認
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


            // -------------------------------------------------
            // 選択中キャラクターを維持
            // -------------------------------------------------

            if (active &&
                selectableEggs != null &&
                selectableEggs.Length > 0)
            {
                int index =
                    Mathf.Clamp(
                        player.selectedEggIndex,
                        0,
                        selectableEggs.Length - 1
                    );


                EggData selectedEgg =
                    selectableEggs[index];


                if (selectedEgg != null)
                {
                    playerManager.SetPlayerEgg(
                        i,
                        selectedEgg
                    );
                }
            }
            else if (active &&
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
    // タイマーUI
    // =========================================================

    void ClearTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "";
        }
    }


    // =========================================================
    // ゲーム開始済み
    // =========================================================

    public bool IsGameStarted()
    {
        return gameStarted;
    }


    // =========================================================
    // ロビー中
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


        for (int i = 0; i < players.Length; i++)
        {
            players[i].joined = false;

            players[i].ready = false;

            players[i].state =
                LobbyPlayerState.Waiting;

            players[i].selectedEggIndex = 0;


            if (players[i].ui != null)
            {
                players[i].ui.SetState(
                    PlayerLobbyUI.UIState.Waiting
                );
            }
        }


        if (playerManager != null)
        {
            playerManager.ReturnToSelect();
        }


        ClearTimerUI();
    }
}