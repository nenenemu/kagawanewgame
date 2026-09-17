using UnityEngine;

public class EggRespawn : MonoBehaviour
{
    [Header("復活設定")]
    [Tooltip("SpawnPointからY方向に何だけ上へ復活するか")]
    public float respawnHeight = 2f;

    [Header("落下判定タグ")]
    public string fallTag = "BB";

    private PlayerManager playerManager;
    private MatchScoreManager scoreManager;

    private int playerIndex = -1;


    // =========================================
    // PlayerManagerからP番号を受け取る
    // =========================================

    public void Initialize(
        PlayerManager manager,
        int index
    )
    {
        playerManager = manager;
        playerIndex = index;

        scoreManager =
            FindFirstObjectByType<MatchScoreManager>();
    }


    // =========================================
    // 落下
    // =========================================

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (!other.CompareTag(fallTag))
            return;

        if (playerManager == null)
            return;


        if (
            playerIndex < 0 ||
            playerIndex >=
            playerManager.players.Length
        )
        {
            return;
        }


        PlayerManager.PlayerData player =
            playerManager.players[playerIndex];


        // =====================================
        // 選択卵チェック
        // =====================================

        if (player.selectedEgg == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : 選択卵がありません。"
            );

            return;
        }


        // =====================================
        // SpawnPointチェック
        // =====================================

        if (player.spawnPoint == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : SpawnPointがありません。"
            );

            return;
        }


        // =====================================
        // ★KO判定
        // =====================================

        if (scoreManager != null)
        {
            scoreManager.RegisterKO(
                playerIndex
            );
        }


        // =====================================
        // 復活位置
        // =====================================

        Vector3 respawnPosition =
            player.spawnPoint.position;

        respawnPosition.y +=
            respawnHeight;


        Quaternion respawnRotation =
            player.spawnPoint.rotation;


        // =====================================
        // 新しい卵を生成
        // =====================================

        GameObject oldEgg =
            gameObject;


        GameObject newEgg =
            Instantiate(
                player.selectedEgg.eggPrefab,
                respawnPosition,
                respawnRotation
            );


        // PlayerManagerの登録を更新
        player.spawnedEgg =
            newEgg;


        // =====================================
        // EggPrefab初期化
        // =====================================

        EggPrefab eggScript =
            newEgg.GetComponent<EggPrefab>();


        if (eggScript != null)
        {
            eggScript.Initialize(
                player.selectedEgg
            );
        }


        // =====================================
        // EggRespawn初期化
        // =====================================

        EggRespawn respawnScript =
            newEgg.GetComponent<EggRespawn>();


        if (respawnScript != null)
        {
            respawnScript.Initialize(
                playerManager,
                playerIndex
            );
        }


        // =====================================
        // 自分のカメラ
        // =====================================

        if (player.playerCamera != null)
        {
            TargetCamera cameraController =
                player.playerCamera
                .GetComponent<TargetCamera>();


            if (cameraController != null)
            {
                cameraController.player =
                    newEgg.transform;
            }
        }


        // =====================================
        // ★全カメラ更新
        // =====================================

        playerManager.RefreshCameras();


        // =====================================
        // 古い卵削除
        // =====================================

        Destroy(oldEgg);


        Debug.Log(
            "P" +
            (playerIndex + 1) +
            " : 落下！" +
            " → SpawnPoint + Y" +
            respawnHeight +
            " に復活"
        );
    }
}