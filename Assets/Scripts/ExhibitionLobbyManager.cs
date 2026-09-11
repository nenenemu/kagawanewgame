using UnityEngine;
using TMPro;

public class ExhibitionLobbyManager : MonoBehaviour
{
    public enum LobbyState
    {
        WaitingForFirstPlayer,
        Joining
    }

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

    [Header("プレイヤー")]
    public LobbyPlayer[] players = new LobbyPlayer[4];

    [Header("タイマー")]
    public float lobbyTime = 60f;

    [Header("UI")]
    public TMP_Text timerText;

    [Header("ゲーム管理")]
    public GameManager gameManager;

    [Header("プレイヤー管理")]
    public PlayerManager playerManager;

    [Header("仮の卵")]
    [Tooltip("キャラ選択がまだ未実装の間に使う卵")]
    public EggData defaultEgg;

    private LobbyState currentState;
    private float remainingTime;
    private bool timerStarted = false;
    private bool gameStarted = false;


    // =========================================================
    // 初期化
    // =========================================================

    void Start()
    {
        currentState = LobbyState.WaitingForFirstPlayer;

        remainingTime = lobbyTime;

        SetupInitialUI();
        UpdateTimerUI();
    }


    // =========================================================
    // 更新
    // =========================================================

    void Update()
    {
        if (gameStarted)
            return;

        if (!timerStarted)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            UpdateTimerUI();

            StartGame();

            return;
        }

        UpdateTimerUI();
    }


    // =========================================================
    // 初期UI
    // =========================================================

    void SetupInitialUI()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].state = LobbyPlayerState.Waiting;
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
    // マウス移動
    // =========================================================

    public void OnPlayerMouseInput(int playerIndex)
    {
        if (gameStarted)
            return;

        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return;


        LobbyPlayer player = players[playerIndex];


        // -----------------------------------------------------
        // 未参加なら参加
        // -----------------------------------------------------

        if (!player.joined)
        {
            JoinPlayer(playerIndex);
            return;
        }


        // -----------------------------------------------------
        // Selecting中
        // -----------------------------------------------------

        if (player.state == LobbyPlayerState.Selecting)
        {
            return;
        }


        // -----------------------------------------------------
        // READY中
        // -----------------------------------------------------

        if (player.state == LobbyPlayerState.Ready)
        {
            return;
        }
    }


    // =========================================================
    // 左クリック
    // =========================================================

    public void OnPlayerMouseClick(int playerIndex)
    {
        if (gameStarted)
            return;

        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return;


        LobbyPlayer player = players[playerIndex];


        // -----------------------------------------------------
        // 未参加でクリック
        // -----------------------------------------------------

        if (!player.joined)
        {
            Debug.Log(
                "P" + (playerIndex + 1) +
                " : LEFT CLICK → 参加"
            );

            JoinPlayer(playerIndex);

            return;
        }


        // -----------------------------------------------------
        // Selecting → READY
        // -----------------------------------------------------

        if (player.state == LobbyPlayerState.Selecting)
        {
            SetReady(playerIndex);

            return;
        }


        // -----------------------------------------------------
        // READY → Selecting
        // -----------------------------------------------------

        if (player.state == LobbyPlayerState.Ready)
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
        LobbyPlayer player = players[playerIndex];

        player.joined = true;
        player.ready = false;
        player.state = LobbyPlayerState.Selecting;


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


        // -----------------------------------------------------
        // タイマー開始
        // -----------------------------------------------------

        if (!timerStarted)
        {
            timerStarted = true;

            currentState =
                LobbyState.Joining;

            remainingTime =
                lobbyTime;

            Debug.Log(
                "===== 最初のプレイヤー参加 ====="
            );

            Debug.Log(
                "60秒タイマー開始"
            );
        }


        Debug.Log(
            "P" + (playerIndex + 1) +
            " : 参加"
        );
    }


    // =========================================================
    // READY
    // =========================================================

    void SetReady(int playerIndex)
    {
        LobbyPlayer player = players[playerIndex];

        player.ready = true;
        player.state = LobbyPlayerState.Ready;


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
    }


    // =========================================================
    // READY解除
    // =========================================================

    void CancelReady(int playerIndex)
    {
        LobbyPlayer player = players[playerIndex];

        player.ready = false;
        player.state = LobbyPlayerState.Selecting;


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
    }


    // =========================================================
    // タイマーUI
    // =========================================================

    void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int seconds =
            Mathf.CeilToInt(
                remainingTime
            );

        timerText.text =
            seconds.ToString();
    }


    // =========================================================
    // 60秒終了 → ゲーム開始
    // =========================================================

    void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "===== 60秒終了 ====="
        );


        int readyCount = 0;


        // -----------------------------------------------------
        // READYプレイヤーを確認
        // -----------------------------------------------------

        for (int i = 0; i < players.Length; i++)
        {
            LobbyPlayer player =
                players[i];

            Debug.Log(
                "P" + (i + 1) +
                " joined=" +
                player.joined +
                " ready=" +
                player.ready +
                " state=" +
                player.state
            );


            bool active =
                player.joined &&
                player.ready;


            if (active)
            {
                readyCount++;
            }


            // -------------------------------------------------
            // PlayerManagerへ反映
            // -------------------------------------------------

            if (playerManager != null)
            {
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


        Debug.Log(
            "READY人数 : " +
            readyCount
        );


        // -----------------------------------------------------
        // READY 0人
        // -----------------------------------------------------

        if (readyCount == 0)
        {
            Debug.LogWarning(
                "READYプレイヤーが0人です。"
            );
        }


        // -----------------------------------------------------
        // GameManager
        // -----------------------------------------------------

        if (gameManager != null)
        {
            gameManager.StartGame();
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
    // ゲーム開始済みか
    // =========================================================

    public bool IsGameStarted()
    {
        return gameStarted;
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
}