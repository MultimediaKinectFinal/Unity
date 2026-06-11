using System;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    [Header("UI 面板")] public GameObject startPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;

    [Header("文字元件")] public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI hudScoreText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI reloadWarningText;

    [Header("警告閃爍設定")] public float flashSpeed = 2f; // 閃爍速度 (數字越大閃越快)
    private bool isFlashing = false;

    private void OnEnable()
    {
        GameEvent.OnGameStart += ShowHUD;
        GameEvent.OnGameOver += ShowGameOver;
        GameEvent.OnUpdateScore += UpdateScore;
        GameEvent.OnWaitingLoad += UpdateReloadWarning;
    }

    private void OnDisable()
    {
        GameEvent.OnGameStart -= ShowHUD;
        GameEvent.OnGameOver -= ShowGameOver;
        GameEvent.OnUpdateScore -= UpdateScore;
        GameEvent.OnWaitingLoad -= UpdateReloadWarning;
    }

    private void Start()
    {
        ShowStartScreen();
    }

    private void Update()
    {
        // 只有在處於裝填狀態時才執行閃爍運算
        if (isFlashing && reloadWarningText is not null)
        {
            // 利用 Sin 波與時間，算出 0.2 ~ 1.0 之間平滑來回浮動的值
            float wave = Mathf.Abs(Mathf.Sin(Time.time * flashSpeed));
            float alpha = Mathf.Lerp(0.2f, 1f, wave);

            // 套用新的透明度 (RGB保持紅色，只改 A)
            reloadWarningText.color = new Color(1f, 0f, 0f, alpha);
        }
    }

    // ==================== 面板切換 ====================
    private void ShowStartScreen()
    {
        startPanel.SetActive(true);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        if (highScoreText != null)
            highScoreText.text = $"Highest score: {GameManager.Instance.HighScore}";
    }

    private void ShowHUD()
    {
        startPanel.SetActive(false);
        hudPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        UpdateScore(0);
    }

    private void ShowGameOver()
    {
        startPanel.SetActive(false);
        hudPanel.SetActive(true);
        gameOverPanel.SetActive(true);

        if (finalScoreText is not null)
            finalScoreText.text = $"Score: {GameManager.Instance.TotalScore}";
    }

    // ==================== 數值更新 ====================
    private void UpdateScore(int score)
    {
        if (hudScoreText is not null) hudScoreText.text = $"Score: {score}";
    }

    private void UpdateReloadWarning(bool isWaiting)
    {
        isFlashing = isWaiting; // 同步閃爍狀態

        if (reloadWarningText is not null)
        {
            if (isWaiting)
            {
                reloadWarningText.text = "LOAD";
                reloadWarningText.color = Color.red; // 初始化為紅色
            }
            else
            {
                reloadWarningText.text = "SHOT";
                reloadWarningText.color = Color.green;
            }
        }
    }
}