using UnityEngine;
using UnityEngine.UI;

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
    // 戦闘中背景画像
    // =========================================================

    [Header("戦闘中背景画像")]
    [Tooltip("P1～P4の背景Imageを設定")]
    public RectTransform[] backgroundImages = new RectTransform[4];


    // =========================================================
    // 背景用Canvas
    // =========================================================

    [Header("背景用Canvas")]
    [Tooltip("B1～B4が入っているCanvasを設定")]
    public Canvas backgroundCanvas;


    // =========================================================
    // カメラサイズ調整
    // =========================================================

    [Header("カメラサイズ調整")]
    [Tooltip("背景の分割領域に対してカメラを内側へ縮める割合")]
    [Range(0f, 0.2f)]
    public float cameraMargin = 0.04f;


    // =========================================================
    // 初期化
    // =========================================================

    void Awake()
    {
        if (players == null || players.Length != 4)
        {
            players = new PlayerData[4];

            for (int i = 0; i < 4; i++)
            {
                players[i] = new PlayerData();
            }
        }

        DisableAllCameras();
        DisableAllBackgrounds();
    }


    // =========================================================
    // ゲーム開始
    // =========================================================

    public void StartGame()
    {
        Debug.Log(
            "===== PLAYER MANAGER : GAME START ====="
        );

        DeleteAllSpawnedEggs();


        // -----------------------------------------------------
        // 参加プレイヤーの卵を生成
        // -----------------------------------------------------

        for (int i = 0; i < players.Length; i++)
        {
            CreatePlayer(i);
        }


        // -----------------------------------------------------
        // カメラ・背景設定
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


        // -----------------------------------------------------
        // 不参加
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // EggDataチェック
        // -----------------------------------------------------

        if (player.selectedEgg == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : 卵が選択されていません。"
            );

            return;
        }


        // -----------------------------------------------------
        // Prefabチェック
        // -----------------------------------------------------

        if (player.selectedEgg.eggPrefab == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : EggDataにPrefabが設定されていません。"
            );

            return;
        }


        // -----------------------------------------------------
        // SpawnPointチェック
        // -----------------------------------------------------

        if (player.spawnPoint == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : SpawnPointが設定されていません。"
            );

            return;
        }


        // -----------------------------------------------------
        // 卵生成
        // -----------------------------------------------------

        GameObject egg =
            Instantiate(
                player.selectedEgg.eggPrefab,
                player.spawnPoint.position,
                player.spawnPoint.rotation
            );


        player.spawnedEgg = egg;


        // -----------------------------------------------------
        // EggRespawn
        // -----------------------------------------------------

        EggRespawn respawn =
            egg.GetComponent<EggRespawn>();

        if (respawn != null)
        {
            respawn.Initialize(
                this,
                playerIndex
            );
        }


        // -----------------------------------------------------
        // EggPrefab
        // -----------------------------------------------------

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


        Debug.Log(
            "P" +
            (playerIndex + 1) +
            " : " +
            player.selectedEgg.eggName +
            " を生成"
        );
    }


    // =========================================================
    // カメラ・背景設定
    // =========================================================

    void UpdateCameras()
    {
        int playerCount =
            GetPlayerCount();


        Debug.Log(
            "===== CAMERA LAYOUT ====="
        );

        Debug.Log(
            "参加人数 : " +
            playerCount
        );


        // -----------------------------------------------------
        // 全部OFF
        // -----------------------------------------------------

        DisableAllCameras();
        DisableAllBackgrounds();


        // -----------------------------------------------------
        // 参加順に配置
        // -----------------------------------------------------

        int slot = 0;

        for (int playerIndex = 0;
             playerIndex < players.Length;
             playerIndex++)
        {
            PlayerData player =
                players[playerIndex];


            if (player == null)
                continue;

            if (!player.joined)
                continue;

            if (player.spawnedEgg == null)
                continue;


            // -------------------------------------------------
            // Camera
            // -------------------------------------------------

            if (player.playerCamera == null)
            {
                Debug.LogWarning(
                    "P" +
                    (playerIndex + 1) +
                    " : Cameraが設定されていません。"
                );

                slot++;
                continue;
            }


            player.playerCamera.gameObject.SetActive(true);


            // -------------------------------------------------
            // カメラ
            // -------------------------------------------------

            SetCameraViewport(
                player.playerCamera,
                slot,
                playerCount
            );


            // -------------------------------------------------
            // 背景
            // -------------------------------------------------

            SetBackgroundViewport(
                playerIndex,
                slot,
                playerCount
            );


            // -------------------------------------------------
            // TargetCamera
            // -------------------------------------------------

            TargetCamera cameraController =
                player.playerCamera
                    .GetComponent<TargetCamera>();

            if (cameraController == null)
            {
                Debug.LogError(
                    "P" +
                    (playerIndex + 1) +
                    " : CameraにTargetCameraがありません！"
                );

                slot++;
                continue;
            }


            cameraController.player =
                player.spawnedEgg.transform;


            cameraController.target =
                FindTargetForPlayer(playerIndex);


            Debug.Log(
                "P" +
                (playerIndex + 1) +
                " → 画面slot " +
                slot +
                " Camera設定完了"
            );


            slot++;
        }
    }


    // =========================================================
    // カメラ設定
    // =========================================================

    void SetCameraViewport(
        Camera camera,
        int slot,
        int playerCount
    )
    {
        Rect baseRect =
            GetViewportRect(
                slot,
                playerCount
            );


        float xMargin =
            baseRect.width * cameraMargin;

        float yMargin =
            baseRect.height * cameraMargin;


        camera.rect =
            new Rect(
                baseRect.x + xMargin,
                baseRect.y + yMargin,
                baseRect.width - (xMargin * 2f),
                baseRect.height - (yMargin * 2f)
            );
    }


    // =========================================================
    // 背景設定
    //
    // Canvasの実サイズを基準に
    // Width / Heightを直接設定する
    // =========================================================

    void SetBackgroundViewport(
    int playerIndex,
    int slot,
    int playerCount
)
    {
        if (backgroundImages == null)
            return;

        if (playerIndex < 0 ||
            playerIndex >= backgroundImages.Length)
            return;

        RectTransform bg =
            backgroundImages[playerIndex];

        if (bg == null)
            return;

        if (backgroundCanvas == null)
        {
            Debug.LogError(
                "PlayerManagerの「Background Canvas」にCanvasを設定してください。"
            );
            return;
        }

        bg.gameObject.SetActive(true);

        // ---------------------------------------------------------
        // Canvasの最大サイズを取得
        // ---------------------------------------------------------

        Canvas.ForceUpdateCanvases();

        RectTransform canvasRect =
            backgroundCanvas.GetComponent<RectTransform>();

        if (canvasRect == null)
            return;

        float canvasWidth =
            canvasRect.rect.width;

        float canvasHeight =
            canvasRect.rect.height;

        if (canvasWidth <= 0f)
            canvasWidth = Screen.width;

        if (canvasHeight <= 0f)
            canvasHeight = Screen.height;


        // ---------------------------------------------------------
        // 今の背景Imageそのものの元サイズ
        // ---------------------------------------------------------

        float originalWidth =
            bg.rect.width;

        float originalHeight =
            bg.rect.height;


        if (originalWidth <= 0f ||
            originalHeight <= 0f)
        {
            Debug.LogWarning(
                "P" +
                (playerIndex + 1) +
                " 背景Imageの元サイズが0です。"
            );

            return;
        }


        // ---------------------------------------------------------
        // 分割領域
        // ---------------------------------------------------------

        Rect rect =
            GetViewportRect(
                slot,
                playerCount
            );


        // ---------------------------------------------------------
        // この背景が必要なサイズ
        // ---------------------------------------------------------

        float targetWidth =
            canvasWidth * rect.width;

        float targetHeight =
            canvasHeight * rect.height;


        // ---------------------------------------------------------
        // 横・縦それぞれの拡大率を計算
        // ---------------------------------------------------------

        float scaleX =
            targetWidth / originalWidth;

        float scaleY =
            targetHeight / originalHeight;


        // ---------------------------------------------------------
        // Imageを分割領域いっぱいまで拡大
        //
        // 縦横比を無視して、それぞれ合わせる
        // ---------------------------------------------------------

        bg.localScale =
            new Vector3(
                scaleX,
                scaleY,
                1f
            );


        // ---------------------------------------------------------
        // 中央基準
        // ---------------------------------------------------------

        bg.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        bg.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        bg.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        // ---------------------------------------------------------
        // 分割領域の中心
        // ---------------------------------------------------------

        float centerX =
            rect.x +
            rect.width * 0.5f;

        float centerY =
            rect.y +
            rect.height * 0.5f;


        // ---------------------------------------------------------
        // Canvas中央からの位置
        // ---------------------------------------------------------

        bg.anchoredPosition =
            new Vector2(
                (centerX - 0.5f) * canvasWidth,
                (centerY - 0.5f) * canvasHeight
            );


        Debug.Log(
            "P" +
            (playerIndex + 1) +
            " 背景Scale : " +
            bg.localScale +
            " / 元サイズ : " +
            originalWidth +
            " x " +
            originalHeight +
            " / 目標 : " +
            targetWidth +
            " x " +
            targetHeight
        );
    }


    // =========================================================
    // 分割位置
    // =========================================================

    Rect GetViewportRect(
        int slot,
        int playerCount
    )
    {
        // =====================================================
        // 1人
        // =====================================================

        if (playerCount == 1)
        {
            return new Rect(
                0f,
                0f,
                1f,
                1f
            );
        }


        // =====================================================
        // 2人
        // =====================================================

        if (playerCount == 2)
        {
            if (slot == 0)
            {
                return new Rect(
                    0f,
                    0f,
                    0.5f,
                    1f
                );
            }

            return new Rect(
                0.5f,
                0f,
                0.5f,
                1f
            );
        }


        // =====================================================
        // 3人
        //
        // ┌────────┬────────┐
        // │   P1   │   P2   │
        // ├────────┴────────┤
        // │       P3        │
        // └─────────────────┘
        // =====================================================

        if (playerCount == 3)
        {
            if (slot == 0)
            {
                return new Rect(
                    0f,
                    0.5f,
                    0.5f,
                    0.5f
                );
            }

            if (slot == 1)
            {
                return new Rect(
                    0.5f,
                    0.5f,
                    0.5f,
                    0.5f
                );
            }

            return new Rect(
                0f,
                0f,
                1f,
                0.5f
            );
        }


        // =====================================================
        // 4人
        //
        // ┌────────┬────────┐
        // │   P1   │   P2   │
        // ├────────┼────────┤
        // │   P3   │   P4   │
        // └────────┴────────┘
        // =====================================================

        if (slot == 0)
        {
            return new Rect(
                0f,
                0.5f,
                0.5f,
                0.5f
            );
        }

        if (slot == 1)
        {
            return new Rect(
                0.5f,
                0.5f,
                0.5f,
                0.5f
            );
        }

        if (slot == 2)
        {
            return new Rect(
                0f,
                0f,
                0.5f,
                0.5f
            );
        }

        return new Rect(
            0.5f,
            0f,
            0.5f,
            0.5f
        );
    }


    // =========================================================
    // 背景全部OFF
    // =========================================================

    void DisableAllBackgrounds()
    {
        if (backgroundImages == null)
            return;

        for (int i = 0;
             i < backgroundImages.Length;
             i++)
        {
            if (backgroundImages[i] != null)
            {
                backgroundImages[i]
                    .gameObject
                    .SetActive(false);
            }
        }
    }


    // =========================================================
    // カメラのターゲットを探す
    // =========================================================

    Transform FindTargetForPlayer(
        int playerIndex
    )
    {
        for (int i = 0;
             i < players.Length;
             i++)
        {
            if (i == playerIndex)
                continue;

            if (!players[i].joined)
                continue;

            if (players[i].spawnedEgg == null)
                continue;

            return players[i]
                .spawnedEgg
                .transform;
        }

        return null;
    }


    // =========================================================
    // 全カメラOFF
    // =========================================================

    void DisableAllCameras()
    {
        for (int i = 0;
             i < players.Length;
             i++)
        {
            DisableCamera(
                players[i]
            );
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
            player.playerCamera
                .gameObject
                .SetActive(false);
        }
    }


    // =========================================================
    // 参加人数
    // =========================================================

    public int GetPlayerCount()
    {
        int count = 0;

        for (int i = 0;
             i < players.Length;
             i++)
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
        for (int i = 0;
             i < players.Length;
             i++)
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
    // 選択画面へ戻る
    // =========================================================

    public void ReturnToSelect()
    {
        Debug.Log(
            "===== RETURN TO SELECT ====="
        );

        DeleteAllSpawnedEggs();

        DisableAllCameras();
        DisableAllBackgrounds();

        for (int i = 0;
             i < players.Length;
             i++)
        {
            players[i].joined = false;
            players[i].selectedEgg = null;
        }
    }


    // =========================================================
    // カメラ再更新
    // =========================================================

    public void RefreshCameras()
    {
        UpdateCameras();
    }
}