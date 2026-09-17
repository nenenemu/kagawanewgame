using UnityEngine;

public class MatchScoreManager : MonoBehaviour
{
    [Header("試合時間")]
    public float matchTime = 60f;

    [Header("現在の残り時間")]
    public float remainingTime;

    [Header("プレイヤー")]
    public int[] koCounts = new int[4];

    // 各プレイヤーを最後に攻撃したプレイヤー
    // 例：
    // lastAttacker[1] = 0
    // → P2を最後に攻撃したのはP1
    private int[] lastAttacker = new int[4];

    private PlayerManager playerManager;
    private GameManager gameManager;

    private bool matchStarted = false;
    private bool matchFinished = false;


    private void Awake()
    {
        playerManager =
            FindFirstObjectByType<PlayerManager>();

        gameManager =
            FindFirstObjectByType<GameManager>();

        ResetScore();
    }


    // =========================================
    // 試合開始
    // =========================================

    public void StartMatch()
    {
        ResetScore();

        remainingTime = matchTime;

        matchStarted = true;
        matchFinished = false;

        Debug.Log(
            "試合開始！ 制限時間 " +
            matchTime +
            " 秒"
        );
    }


    // =========================================
    // 毎フレーム
    // =========================================

    private void Update()
    {
        if (!matchStarted)
            return;

        if (matchFinished)
            return;


        remainingTime -= Time.deltaTime;


        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            FinishMatch();
        }
    }


    // =========================================
    // 試合終了
    // =========================================

    private void FinishMatch()
    {
        if (matchFinished)
            return;

        matchFinished = true;
        matchStarted = false;

        Debug.Log("試合終了！");


        // GameManagerに試合終了を伝える
        if (gameManager != null)
        {
            gameManager.EndGame();
        }
    }


    // =========================================
    // 攻撃を記録
    // =========================================
    //
    // attacker = 攻撃した卵
    // target   = 攻撃された卵
    //

    public void RegisterHit(
        GameObject attacker,
        GameObject target
    )
    {
        if (!matchStarted)
            return;

        if (playerManager == null)
            return;


        int attackerIndex =
            FindPlayerIndex(attacker);

        int targetIndex =
            FindPlayerIndex(target);


        if (attackerIndex < 0)
            return;

        if (targetIndex < 0)
            return;

        if (attackerIndex == targetIndex)
            return;


        // 最後に攻撃した人を記録
        lastAttacker[targetIndex] =
            attackerIndex;


        Debug.Log(
            "P" +
            (attackerIndex + 1) +
            " → P" +
            (targetIndex + 1) +
            " を攻撃"
        );
    }


    // =========================================
    // KOを記録
    // =========================================

    public void RegisterKO(int targetIndex)
    {
        if (!matchStarted)
            return;

        if (targetIndex < 0 ||
            targetIndex >= lastAttacker.Length)
        {
            return;
        }


        int attackerIndex =
            lastAttacker[targetIndex];


        // 誰にも攻撃されていなかった
        if (attackerIndex < 0)
        {
            Debug.Log(
                "P" +
                (targetIndex + 1) +
                " が自力で落下"
            );

            return;
        }


        // 自分自身はKOできない
        if (attackerIndex == targetIndex)
            return;


        koCounts[attackerIndex]++;


        Debug.Log(
            "P" +
            (attackerIndex + 1) +
            " KO！" +
            " → " +
            koCounts[attackerIndex] +
            " KO"
        );


        // 一度KOしたら記録を消す
        lastAttacker[targetIndex] = -1;
    }


    // =========================================
    // プレイヤー番号を調べる
    // =========================================

    private int FindPlayerIndex(
        GameObject egg
    )
    {
        if (egg == null)
            return -1;

        if (playerManager == null)
            return -1;


        for (
            int i = 0;
            i < playerManager.players.Length;
            i++
        )
        {
            if (
                playerManager.players[i].spawnedEgg
                == egg
            )
            {
                return i;
            }
        }


        return -1;
    }


    // =========================================
    // スコア初期化
    // =========================================

    public void ResetScore()
    {
        for (int i = 0; i < 4; i++)
        {
            koCounts[i] = 0;
            lastAttacker[i] = -1;
        }

        remainingTime = matchTime;

        matchStarted = false;
        matchFinished = false;
    }


    // =========================================
    // 外から取得
    // =========================================

    public int GetKO(int playerIndex)
    {
        if (
            playerIndex < 0 ||
            playerIndex >= koCounts.Length
        )
        {
            return 0;
        }

        return koCounts[playerIndex];
    }


    public float GetRemainingTime()
    {
        return remainingTime;
    }


    public bool IsMatchStarted()
    {
        return matchStarted;
    }
}