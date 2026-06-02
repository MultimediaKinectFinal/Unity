using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum GameState
{
    Start,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public int TotalScore { get; private set; }
    public int HighScore { get; private set; }
    private float playingStartTime; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void OnEnable()
    {
        GameEvent.OnEnemyDestroyed += AddScore;
        GameEvent.OnGameOver += TriggerGameOver;
    }


    private void Start()
    {
        ChangeState(GameState.Start);
    }

    private void OnDestroy()
    {
        GameEvent.OnEnemyDestroyed -= AddScore;
        GameEvent.OnGameOver -= TriggerGameOver;
    }


    // 遊玩時間
    public float GetPlayingTime()
    {
        if (CurrentState == GameState.Start) return 0f;
        
        return Time.timeSinceLevelLoad - playingStartTime; 
    }
    
    public void StartPlaying()
    {
        ChangeState(GameState.Playing);
    }

    // 狀態管理
    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        
        if (newState == GameState.Start)
        {
            Time.timeScale = 0f;
        }
        else if (newState == GameState.Playing)
        {
            TotalScore = 0; 
            playingStartTime = Time.timeSinceLevelLoad;
            Time.timeScale = 1f;
            
            GameEvent.OnGameStart?.Invoke(); 
        }
        else if (newState == GameState.GameOver)
        {
            Time.timeScale = 0f; // 凍結時間
            
            // 檢查並儲存最高分
            if (TotalScore > HighScore)
            {
                HighScore = TotalScore;
                PlayerPrefs.SetInt("HighScore", HighScore);
                PlayerPrefs.Save();
            }
        }
    }

    // 分數計算
    public void AddScore(Vector3 _, int score)
    {
        if (CurrentState != GameState.Playing) return;
        
        TotalScore += score;
        // 呼叫事件讓 UIManager 更新介面
        GameEvent.OnUpdateScore?.Invoke(TotalScore); 
    }

    // 一發死亡判定與遊戲結束
    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        ChangeState(GameState.GameOver);
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f; // 恢復時間
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // 重新載入當前場景
    }
}