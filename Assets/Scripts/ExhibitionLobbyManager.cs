using UnityEngine;

public class ExhibitionLobbyManager : MonoBehaviour
{
    //==================================================
    // プレイヤー状態
    //==================================================

    public enum LobbyPlayerState
    {
        Waiting,
        Selecting,
        Ready
    }


    //==================================================
    // プレイヤーデータ
    //==================================================

    [System.Serializable]
    public class LobbyPlayer
    {
        public LobbyPlayerState state =
            LobbyPlayerState.Waiting;

        [HideInInspector]
        public bool joined = false;

        [HideInInspector]
        public int selectedEggIndex = 0;

        public PlayerLobbyUI ui;
    }


    //==================================================
    // Inspector
    //==================================================

    [Header("プレイヤー")]
    public LobbyPlayer[] players =
        new LobbyPlayer[4];

    [Header("ゲーム管理")]
    public GameManager gameManager;

    [Header("プレイヤー管理")]
    public PlayerManager playerManager;

    [Header("卵セレクト")]
    public EggData[] selectableEggs;

    [Header("卵セレクト：空の場合の予備")]
    public EggData defaultEgg;

    [Header("3Dキャラプレビュー")]
    public CharacterPreview[] characterPreviews =
        new CharacterPreview[4];


    //==================================================
    // Awake
    //==================================================

    private void Awake()
    {
        if (players == null ||
            players.Length != 4)
        {
            players = new LobbyPlayer[4];
        }


        for (int i = 0; i < 4; i++)
        {
            if (players[i] == null)
            {
                players[i] =
                    new LobbyPlayer();
            }


            players[i].state =
                LobbyPlayerState.Waiting;

            players[i].joined = false;

            players[i].selectedEggIndex = 0;
        }
    }


    //==================================================
    // ゲーム中か
    //==================================================

    public bool IsGameStarted()
    {
        if (gameManager == null)
            return false;

        return
            gameManager.currentState ==
            GameManager.GameState.Playing;
    }


    //==================================================
    // 左クリック
    //==================================================

    public void OnPlayerMouseLeftClick(int index)
    {
        if (!IsValidPlayerIndex(index))
            return;


        //==================================================
        // PLAYING中は無視
        //==================================================

        if (gameManager != null &&
            gameManager.currentState ==
            GameManager.GameState.Playing)
        {
            return;
        }


        //==================================================
        // COUNTDOWN中
        //==================================================

        if (gameManager != null &&
            gameManager.currentState ==
            GameManager.GameState.Countdown)
        {
            Debug.Log(
                $"P{index + 1} : カウントダウン中にLEFT → キャンセル"
            );


            // カウントダウンを止める
            gameManager.CancelCountdown();


            LobbyPlayer player =
                players[index];


            //==================================================
            // 新規参加者
            //==================================================

            if (!player.joined)
            {
                JoinPlayer(index);
                return;
            }


            //==================================================
            // 既にREADYだった人
            // → SELECTINGへ戻す
            //==================================================

            if (player.state ==
                LobbyPlayerState.Ready)
            {
                CancelReady(index);
                return;
            }


            return;
        }


        //==================================================
        // SELECT画面
        //==================================================

        LobbyPlayer playerData =
            players[index];


        // 未参加
        if (!playerData.joined)
        {
            JoinPlayer(index);
            return;
        }


        // SELECTING
        if (playerData.state ==
            LobbyPlayerState.Selecting)
        {
            SetReady(index);
            return;
        }


        // READY
        if (playerData.state ==
            LobbyPlayerState.Ready)
        {
            CancelReady(index);
            return;
        }
    }


    //==================================================
    // 右クリック
    //==================================================

    public void OnPlayerMouseRightClick(int index)
    {
        if (!IsValidPlayerIndex(index))
            return;


        // Playing中
        if (gameManager != null &&
            gameManager.currentState ==
            GameManager.GameState.Playing)
        {
            return;
        }


        // Countdown中はキャラ変更禁止
        if (gameManager != null &&
            gameManager.currentState ==
            GameManager.GameState.Countdown)
        {
            return;
        }


        LobbyPlayer player =
            players[index];


        // 未参加
        if (!player.joined)
            return;


        // SELECTING中だけ変更可能
        if (player.state !=
            LobbyPlayerState.Selecting)
        {
            return;
        }


        ChangeCharacter(index);
    }


    //==================================================
    // プレイヤー参加
    //==================================================

    private void JoinPlayer(int index)
    {
        LobbyPlayer player =
            players[index];


        player.joined = true;

        player.state =
            LobbyPlayerState.Selecting;

        player.selectedEggIndex = 0;


        ApplySelectedEgg(index);

        UpdateUI(index);

        // 3Dプレビュー更新
        UpdateCharacterPreview(index);


        Debug.Log(
            $"P{index + 1} : JOIN"
        );
    }


    //==================================================
    // キャラ変更
    //==================================================

    private void ChangeCharacter(int index)
    {
        if (!IsValidPlayerIndex(index))
            return;


        LobbyPlayer player =
            players[index];


        if (player.state !=
            LobbyPlayerState.Selecting)
        {
            return;
        }


        if (selectableEggs == null ||
            selectableEggs.Length == 0)
        {
            Debug.LogWarning(
                "選択可能な卵がありません。"
            );

            return;
        }


        player.selectedEggIndex++;


        if (player.selectedEggIndex >=
            selectableEggs.Length)
        {
            player.selectedEggIndex = 0;
        }


        ApplySelectedEgg(index);

        UpdateUI(index);

        // 3Dプレビュー更新
        UpdateCharacterPreview(index);


        EggData egg =
            GetSelectedEgg(index);


        if (egg != null)
        {
            Debug.Log(
                $"P{index + 1} : キャラ変更 → {egg.name}"
            );
        }
    }


    //==================================================
    // 3Dキャラプレビュー更新
    //==================================================

    private void UpdateCharacterPreview(int index)
    {
        if (!IsValidPlayerIndex(index))
            return;


        if (characterPreviews == null)
            return;


        if (index >= characterPreviews.Length)
            return;


        CharacterPreview preview =
            characterPreviews[index];


        if (preview == null)
            return;


        EggData egg =
            GetSelectedEgg(index);


        preview.ShowEgg(egg);
    }


    //==================================================
    // READY
    //==================================================

    private void SetReady(int index)
    {
        if (!IsValidPlayerIndex(index))
            return;


        LobbyPlayer player =
            players[index];


        ApplySelectedEgg(index);


        player.state =
            LobbyPlayerState.Ready;


        UpdateUI(index);


        Debug.Log(
            $"P{index + 1} : READY"
        );


        CheckAllReady();
    }


    //==================================================
    // READYキャンセル
    //==================================================

    private void CancelReady(int index)
    {
        if (!IsValidPlayerIndex(index))
            return;

        LobbyPlayer player =
            players[index];

        player.state =
            LobbyPlayerState.Selecting;

        // 全員READY用画像を消す
        if (player.ui != null)
        {
            player.ui.SetAllReadyImage(false);
        }

        UpdateUI(index);

        UpdateCharacterPreview(index);

        Debug.Log(
            $"P{index + 1} : READY CANCEL"
        );

        if (gameManager != null &&
            gameManager.currentState ==
            GameManager.GameState.Countdown)
        {
            gameManager.CancelCountdown();
        }
    }


    //==================================================
    // 全員READY確認
    //==================================================

    private void CheckAllReady()
    {
        int joinedCount = 0;
        int readyCount = 0;


        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;


            if (!players[i].joined)
                continue;


            joinedCount++;


            if (players[i].state ==
                LobbyPlayerState.Ready)
            {
                readyCount++;
            }
        }


        Debug.Log(
            $"READY確認 : {readyCount}/{joinedCount}"
        );


        //==================================================
        // 2人以上
        // かつ全員READY
        //==================================================

        if (joinedCount >= 2 &&
    readyCount == joinedCount)
        {
            Debug.Log(
                "全参加者READY → 10秒カウントダウン開始"
            );

            // 全員READY用画像を表示
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                    continue;

                if (!players[i].joined)
                    continue;

                if (players[i].ui != null)
                {
                    players[i].ui.SetAllReadyImage(true);
                }
            }

            // 全員の卵を同期
            SyncAllPlayers();

            StartGame();
        }
    }


    //==================================================
    // 全プレイヤー同期
    //==================================================

    private void SyncAllPlayers()
    {
        if (playerManager == null)
        {
            Debug.LogError(
                "PlayerManagerが設定されていません。"
            );

            return;
        }


        for (int i = 0; i < players.Length; i++)
        {
            LobbyPlayer player =
                players[i];


            if (player == null)
                continue;


            if (!player.joined)
            {
                playerManager.SetPlayerJoined(
                    i,
                    false
                );

                playerManager.SetPlayerEgg(
                    i,
                    null
                );

                continue;
            }


            EggData egg =
                GetSelectedEgg(i);


            if (egg == null)
            {
                Debug.LogError(
                    $"P{i + 1} : ロビー側でも卵が取得できません。"
                );

                continue;
            }


            playerManager.SetPlayerJoined(
                i,
                true
            );


            playerManager.SetPlayerEgg(
                i,
                egg
            );


            Debug.Log(
                $"P{i + 1} : ゲーム開始用卵同期 → {egg.name}"
            );
        }
    }


    //==================================================
    // ゲーム開始
    //==================================================

    private void StartGame()
    {
        if (gameManager == null)
        {
            Debug.LogError(
                "GameManagerが設定されていません。"
            );

            return;
        }


        gameManager.StartPlayingFromLobby();
    }


    //==================================================
    // 選択卵をPlayerManagerへ反映
    //==================================================

    private void ApplySelectedEgg(int index)
    {
        if (!IsValidPlayerIndex(index))
            return;


        EggData egg =
            GetSelectedEgg(index);


        if (playerManager == null)
            return;


        if (egg == null)
        {
            Debug.LogError(
                $"P{index + 1} : 選択卵がnullです。"
            );


            playerManager.SetPlayerJoined(
                index,
                false
            );


            playerManager.SetPlayerEgg(
                index,
                null
            );


            return;
        }


        playerManager.SetPlayerJoined(
            index,
            true
        );


        playerManager.SetPlayerEgg(
            index,
            egg
        );
    }


    //==================================================
    // UI更新
    //==================================================

    private void UpdateUI(int index)
    {
        if (!IsValidPlayerIndex(index))
            return;


        LobbyPlayer player =
            players[index];


        if (player.ui == null)
            return;


        player.ui.SetState(
            (PlayerLobbyUI.UIState)
                player.state
        );


        EggData egg =
            GetSelectedEgg(index);


        player.ui.SetCharacterImage(
    player.selectedEggIndex
);


        player.ui.SetStatusImage(
            player.selectedEggIndex
        );
    }


    //==================================================
    // 選択卵取得
    //==================================================

    private EggData GetSelectedEgg(int index)
    {
        if (!IsValidPlayerIndex(index))
            return defaultEgg;


        int eggIndex =
            players[index].selectedEggIndex;


        if (selectableEggs != null &&
            selectableEggs.Length > 0 &&
            eggIndex >= 0 &&
            eggIndex < selectableEggs.Length)
        {
            EggData egg =
                selectableEggs[eggIndex];


            if (egg != null)
                return egg;
        }


        return defaultEgg;
    }


    //==================================================
    // ロビーへ戻る
    //==================================================

    public void ReturnToLobby()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;


            players[i].state =
                LobbyPlayerState.Waiting;


            players[i].joined = false;


            players[i].selectedEggIndex = 0;


            if (playerManager != null)
            {
                playerManager.SetPlayerJoined(
                    i,
                    false
                );


                playerManager.SetPlayerEgg(
                    i,
                    null
                );
            }


            // 3Dプレビューを消す
            ClearCharacterPreview(i);


            if (players[i].ui != null)
            {
                players[i].ui.SetAllReadyImage(false);
            }

            UpdateUI(i);
        }


        Debug.Log(
            "ロビーを初期状態に戻しました。"
        );
    }


    //==================================================
    // 3Dキャラプレビューを消す
    //==================================================

    private void ClearCharacterPreview(int index)
    {
        if (!IsValidPlayerIndex(index))
            return;


        if (characterPreviews == null)
            return;


        if (index >= characterPreviews.Length)
            return;


        CharacterPreview preview =
            characterPreviews[index];


        if (preview == null)
            return;


        preview.ClearPreview();
    }


    //==================================================
    // インデックス確認
    //==================================================

    private bool IsValidPlayerIndex(int index)
    {
        return players != null &&
               index >= 0 &&
               index < players.Length;
    }
}