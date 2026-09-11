using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // =========================================================
    // プレイヤー1人分のデータ
    // =========================================================

    [System.Serializable]
    public class PlayerData
    {
        [Header("参加")]
        public bool joined = false;

        [Header("選択した卵")]
        public EggData selectedEgg;

        [Header("生成位置")]
        public Transform spawnPoint;

        [Header("専用カメラ")]
        public Camera playerCamera;

        [Header("生成された卵")]
        [HideInInspector]
        public GameObject spawnedEgg;
    }


    // =========================================================
    // プレイヤー4人
    // =========================================================

    [Header("プレイヤー")]
    public PlayerData[] players = new PlayerData[4];


    // =========================================================
    // 初期化
    // =========================================================

    void Awake()
    {
        // 配列が4人分でなければ作り直す
        if (players == null || players.Length != 4)
        {
            players = new PlayerData[4];

            for (int i = 0; i < 4; i++)
            {
                players[i] = new PlayerData();
            }
        }

        // 最初は全カメラOFF
        DisableAllCameras();
    }


    // =========================================================
    // ゲーム開始
    // GameManagerから呼ぶ
    // =========================================================

    public void StartGame()
    {
        Debug.Log(
            "===== PLAYER MANAGER : GAME START ====="
        );


        // -----------------------------------------------------
        // 念のため古い卵を削除
        // -----------------------------------------------------

        DeleteAllSpawnedEggs();


        // -----------------------------------------------------
        // 4人分の卵を生成
        // -----------------------------------------------------

        for (int i = 0; i < players.Length; i++)
        {
            CreatePlayer(i);
        }


        // -----------------------------------------------------
        // カメラ設定
        // -----------------------------------------------------

        UpdateCameras();


        Debug.Log(
            "参加人数 : " +
            GetPlayerCount()
        );
    }


    // =========================================================
    // プレイヤー生成
    // =========================================================

    void CreatePlayer(int playerIndex)
    {
        PlayerData player =
            players[playerIndex];


        // =====================================================
        // 参加していない
        // =====================================================

        if (!player.joined)
        {
            Debug.Log(
                "P" +
                (playerIndex + 1) +
                " : 不参加"
            );

            if (player.playerCamera != null)
            {
                player.playerCamera.gameObject.SetActive(false);
            }

            return;
        }


        // =====================================================
        // EggDataチェック
        // =====================================================

        if (player.selectedEgg == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : 卵が選択されていません。"
            );

            return;
        }


        // =====================================================
        // Prefabチェック
        // =====================================================

        if (player.selectedEgg.eggPrefab == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : EggDataにPrefabが設定されていません。"
            );

            return;
        }


        // =====================================================
        // SpawnPointチェック
        // =====================================================

        if (player.spawnPoint == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : SpawnPointが設定されていません。"
            );

            return;
        }


        // =====================================================
        // 卵生成
        // =====================================================

        GameObject egg =
            Instantiate(
                player.selectedEgg.eggPrefab,
                player.spawnPoint.position,
                player.spawnPoint.rotation
            );


        player.spawnedEgg = egg;



        // =====================================================
        // EggPrefab初期化
        // =====================================================

        EggPrefab eggScript =
            egg.GetComponent<EggPrefab>();


        if (eggScript != null)
        {
            eggScript.Initialize(
                player.selectedEgg
            );
        }
        else
        {
            Debug.LogWarning(
                egg.name +
                " に EggPrefab がありません。"
            );
        }


        // =====================================================
        // MultiMouseTransform初期化
        // =====================================================

        


        // =====================================================
        // ログ
        // =====================================================

        Debug.Log(
            "P" +
            (playerIndex + 1) +
            " : " +
            player.selectedEgg.eggName +
            " を生成"
        );
    }


    // =========================================================
    // カメラ設定
    // =========================================================

    void UpdateCameras()
    {
        for (int i = 0; i < players.Length; i++)
        {
            PlayerData player =
                players[i];


            // =====================================================
            // カメラなし
            // =====================================================

            if (player.playerCamera == null)
            {
                Debug.LogWarning(
                    "P" +
                    (i + 1) +
                    " : Cameraが設定されていません。"
                );

                continue;
            }


            // =====================================================
            // 参加していない / 卵がない
            // =====================================================

            if (!player.joined ||
                player.spawnedEgg == null)
            {
                player.playerCamera.gameObject.SetActive(false);

                continue;
            }


            // =====================================================
            // カメラON
            // =====================================================

            player.playerCamera.gameObject.SetActive(true);


            // =====================================================
            // TargetCamera取得
            // =====================================================

            TargetCamera cameraController =
                player.playerCamera
                    .GetComponent<TargetCamera>();


            if (cameraController == null)
            {
                Debug.LogError(
                    "P" +
                    (i + 1) +
                    " : CameraにTargetCameraがありません！"
                );

                continue;
            }


            // =====================================================
            // 自分
            // =====================================================

            cameraController.player =
                player.spawnedEgg.transform;


            // =====================================================
            // ターゲット
            // =====================================================

            cameraController.target =
                FindTargetForPlayer(i);


            Debug.Log(
                "P" +
                (i + 1) +
                " Camera設定完了"
            );
        }
    }


    // =========================================================
    // カメラの相手を探す
    // =========================================================

    Transform FindTargetForPlayer(
        int playerIndex
    )
    {
        // 自分以外の参加プレイヤーを探す

        for (int i = 0; i < players.Length; i++)
        {
            // 自分は除外
            if (i == playerIndex)
                continue;


            // 不参加は除外
            if (!players[i].joined)
                continue;


            // 卵がない場合は除外
            if (players[i].spawnedEgg == null)
                continue;


            // 最初に見つかった相手を返す
            return players[i]
                .spawnedEgg
                .transform;
        }


        // 相手がいない
        return null;
    }


    // =========================================================
    // 全カメラOFF
    // =========================================================

    void DisableAllCameras()
    {
        for (int i = 0; i < players.Length; i++)
        {
            DisableCamera(players[i]);
        }
    }


    // =========================================================
    // カメラ1台OFF
    // =========================================================

    void DisableCamera(
        PlayerData player
    )
    {
        if (player == null)
            return;


        if (player.playerCamera != null)
        {
            player.playerCamera.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // 参加人数取得
    // =========================================================

    public int GetPlayerCount()
    {
        int count = 0;


        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].joined &&
                players[i].spawnedEgg != null)
            {
                count++;
            }
        }


        return count;
    }


    // =========================================================
    // UIから呼ぶ
    // プレイヤー参加設定
    // =========================================================

    public void SetPlayerJoined(
        int playerIndex,
        bool joined
    )
    {
        if (playerIndex < 0 ||
            playerIndex >= players.Length)
        {
            return;
        }


        players[playerIndex].joined =
            joined;


        Debug.Log(
            "P" +
            (playerIndex + 1) +
            " 参加 : " +
            joined
        );
    }


    // =========================================================
    // UIから呼ぶ
    // 卵選択
    // =========================================================

    public void SetPlayerEgg(
        int playerIndex,
        EggData egg
    )
    {
        if (playerIndex < 0 ||
            playerIndex >= players.Length)
        {
            return;
        }


        players[playerIndex].selectedEgg =
            egg;


        if (egg != null)
        {
            Debug.Log(
                "P" +
                (playerIndex + 1) +
                " 卵選択 : " +
                egg.eggName
            );
        }
    }


    // =========================================================
    // 生成された卵を全部削除
    // =========================================================

    public void DeleteAllSpawnedEggs()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].spawnedEgg != null)
            {
                Destroy(
                    players[i].spawnedEgg
                );

                players[i].spawnedEgg = null;
            }
        }
    }


    // =========================================================
    // 選択画面に戻る
    // =========================================================

    public void ReturnToSelect()
    {
        Debug.Log(
            "===== RETURN TO SELECT ====="
        );


        // 卵を全部削除
        DeleteAllSpawnedEggs();


        // カメラを全部OFF
        DisableAllCameras();


        // 参加状態と選択をリセット
        for (int i = 0; i < players.Length; i++)
        {
            players[i].joined = false;
            players[i].selectedEgg = null;
        }
    }
}