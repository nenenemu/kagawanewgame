using System.Collections;
using TMPro;
using UnityEngine;

public class MatchUI : MonoBehaviour
{
    [Header("試合中UI")]
    public GameObject matchUI;
    public TMP_Text timeText;

    [Header("リザルトUI")]
    public GameObject resultUI;

    public TMP_Text p1ScoreText;
    public TMP_Text p2ScoreText;
    public TMP_Text p3ScoreText;
    public TMP_Text p4ScoreText;

    [Header("リザルト後")]
    public TMP_Text returnCountdownText;
    public float resultDisplayTime = 5f;

    private MatchScoreManager scoreManager;
    private PlayerManager playerManager;
    private GameManager gameManager;

    private Coroutine resultCoroutine;

    private void Awake()
    {
        scoreManager =
            FindFirstObjectByType<MatchScoreManager>();

        playerManager =
            FindFirstObjectByType<PlayerManager>();

        gameManager =
            FindFirstObjectByType<GameManager>();

        if (matchUI != null)
            matchUI.SetActive(false);

        if (resultUI != null)
            resultUI.SetActive(false);
    }

    private void Update()
    {
        if (scoreManager == null)
            return;

        // 試合中
        if (scoreManager.IsMatchStarted())
        {
            UpdateTimeUI();
        }
    }

    // =========================================================
    // 試合開始
    // =========================================================

    public void StartMatchUI()
    {
        if (resultCoroutine != null)
        {
            StopCoroutine(resultCoroutine);
            resultCoroutine = null;
        }

        if (matchUI != null)
            matchUI.SetActive(true);

        if (resultUI != null)
            resultUI.SetActive(false);

        if (returnCountdownText != null)
            returnCountdownText.text = "";

        UpdateTimeUI();
    }

    // =========================================================
    // 残り時間表示
    // =========================================================

    private void UpdateTimeUI()
    {
        if (timeText == null)
            return;

        float remaining =
            scoreManager.GetRemainingTime();

        int seconds =
            Mathf.CeilToInt(remaining);

        if (seconds < 0)
            seconds = 0;

        timeText.text =
            "TIME " + seconds.ToString();
    }

    // =========================================================
    // リザルト開始
    // =========================================================

    public void ShowResult()
    {
        if (resultCoroutine != null)
            return;

        if (matchUI != null)
            matchUI.SetActive(false);

        if (resultUI != null)
            resultUI.SetActive(true);

        UpdateResultScores();

        resultCoroutine =
            StartCoroutine(ResultCountdown());
    }

    // =========================================================
    // KO数表示
    // =========================================================

    private void UpdateResultScores()
    {
        if (scoreManager == null)
            return;

        if (playerManager == null)
            return;

        for (int i = 0; i < 4; i++)
        {
            bool joined =
                playerManager.players[i].joined;

            int score =
                scoreManager.GetKO(i);

            TMP_Text targetText = null;

            switch (i)
            {
                case 0:
                    targetText = p1ScoreText;
                    break;

                case 1:
                    targetText = p2ScoreText;
                    break;

                case 2:
                    targetText = p3ScoreText;
                    break;

                case 3:
                    targetText = p4ScoreText;
                    break;
            }

            if (targetText == null)
                continue;

            if (joined)
            {
                targetText.gameObject.SetActive(true);

                targetText.text =
                    "P" +
                    (i + 1) +
                    "   " +
                    score +
                    " KO";
            }
            else
            {
                targetText.gameObject.SetActive(false);
            }
        }
    }

    // =========================================================
    // 5秒カウントダウン
    // =========================================================

    private IEnumerator ResultCountdown()
    {
        float remaining =
            resultDisplayTime;

        while (remaining > 0f)
        {
            if (returnCountdownText != null)
            {
                int seconds =
                    Mathf.CeilToInt(remaining);

                returnCountdownText.text =
                    seconds +
                    "秒後に戻ります";
            }

            remaining -= Time.deltaTime;

            yield return null;
        }

        if (returnCountdownText != null)
            returnCountdownText.text = "";

        ReturnToLobby();
    }

    // =========================================================
    // ロビーへ戻る
    // =========================================================

    private void ReturnToLobby()
    {
        if (resultUI != null)
            resultUI.SetActive(false);

        if (matchUI != null)
            matchUI.SetActive(false);

        if (gameManager != null)
        {
            gameManager.ReturnToSelect();
        }

        resultCoroutine = null;
    }
}