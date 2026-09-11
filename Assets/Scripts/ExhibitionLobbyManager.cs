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


    void Start()
    {
        currentState = LobbyState.WaitingForFirstPlayer;

        remainingTime = lobbyTime;

        SetupInitialUI();

        UpdateTimerUI();
    }


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
    // マウス入力
    // =========================================================

    /// <summary>
    /// P1～P4に割り当てられたマウスが動いたとき。
    /// </summary>
    public void OnPlayerMouseInput(int playerIndex)
    {
        if (gameStarted)
            return;

        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return;


        LobbyPlayer player = players[playerIndex];


        // -----------------------------------------------------
        // まだ参加していない
        // -----------------------------------------------------

        if (!player.joined)
        {
            JoinPlayer(playerIndex);
            return;
        }


        // -----------------------------------------------------
        // 選択中
        // -----------------------------------------------------

        if (player.state == LobbyPlayerState.Selecting)
        {
            // 今はマウス移動では何もしない
            return;
        }


        // -----------------------------------------------------
        // READY中
        // -----------------------------------------------------

        if (player.state == LobbyPlayerState.Ready)
        {
            // マウス移動では何もしない
            return;
        }
    }


    /// <summary>
    /// P1～P4に割り当てられたマウスがクリックされた。
    /// </summary>
    public void OnPlayerMouseClick(int playerIndex)
    {
        if (gameStarted)
            return;

        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return;


        LobbyPlayer player = players[playerIndex];


        // まだ参加していないなら
        // クリックでも参加扱い
        if (!player.joined)
        {
            JoinPlayer(playerIndex);
            return;
        }


        // -----------------------------------------------------
        // 選択中 → READY
        // -----------------------------------------------------

        if (player.state == LobbyPlayerState.Selecting)
        {
            SetReady(playerIndex);
            return;
        }


        // -----------------------------------------------------
        // READY → 選択中
        // -----------------------------------------------------

        if (player.state == LobbyPlayerState.Ready)
        {
            CancelReady(playerIndex);
            return;
        }
    }


    // =========================================================
    // 参加
    // =========================================================

    void JoinPlayer(int playerIndex)
    {
        LobbyPlayer player = players[playerIndex];

        player.joined = true;
        player.ready = false;

        player.state = LobbyPlayerState.Selecting;


        // PlayerManagerに参加を通知
        if (playerManager != null)
        {
            playerManager.SetPlayerJoined(
                playerIndex,
                true
            );

            // 仮の卵が設定されている場合だけセット
            if (defaultEgg != null)
            {
                playerManager.SetPlayerEgg(
                    playerIndex,
                    defaultEgg
                );
            }
        }


        // UI変更 
        if (player.ui != null)
        {
            player.ui.SetState(
                PlayerLobbyUI.UIState.Selecting
            );
        }


        // 最初の参加者なら60秒開始 
        if (!timerStarted)
        {
            timerStarted = true;
            currentState = LobbyState.Joining;

            remainingTime = lobbyTime;

            Debug.Log("最初のプレイヤー参加 → 60秒開始");
        }

        Debug.Log(
            "P" + (playerIndex + 1) +
            " 参加"
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
            " READY"
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
            " READY解除"
        );
    }


    // =========================================================
    // タイマー表示
    // =========================================================

    void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int seconds = Mathf.CeilToInt(remainingTime);

        timerText.text = seconds.ToString();
    }


    // =========================================================
    // ゲーム開始
    // =========================================================

    void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        Debug.Log("===== 60秒終了 =====");
        Debug.Log("ゲーム開始");


        // -----------------------------------------------------
        // READYしているプレイヤーだけ参加扱い
        // -----------------------------------------------------

        for (int i = 0; i < players.Length; i++)
        {
            LobbyPlayer player = players[i];

            bool active =
                player.joined &&
                player.ready;


            if (playerManager != null)
            {
                playerManager.SetPlayerJoined(
                    i,
                    active
                );

                if (active && defaultEgg != null)
                {
                    playerManager.SetPlayerEgg(
                        i,
                        defaultEgg
                    );
                }
            }
        }


        // -----------------------------------------------------
        // GameManagerへ
        // -----------------------------------------------------

        if (gameManager != null)
        {
            gameManager.StartGame();
        }
    }


    // =========================================================
    // 外部から状態取得
    // =========================================================

    public bool IsGameStarted()
    {
        return gameStarted;
    }

    public bool IsPlayerJoined(int playerIndex)
    {
        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return false;

        return players[playerIndex].joined;
    }

    public bool IsPlayerReady(int playerIndex)
    {
        if (playerIndex < 0 ||
            playerIndex >= players.Length)
            return false;

        return players[playerIndex].ready;
    }
}