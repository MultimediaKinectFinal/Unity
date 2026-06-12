using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成設定")]
    [Tooltip("敵軍Prefab")]
    public GameObject enemyTankPrefab;

    [Tooltip("重生點物件(Transform Array)")]
    public Transform[] spawnPoints;

    [Header("節奏與難度")]
    [Tooltip("初始生成間隔(秒)")]
    public float initialSpawnInterval = 5f;

    [Tooltip("最小生成間隔")]
    public float minSpawnInterval = 1f;

    [Tooltip("難度提升到最高所需時間(秒)")]
    public float difficultyRampUpTime = 60f;

    [Tooltip("場上同時最多坦克數")]
    public int maxEnemiesOnScreen = 5;

    private float timer = 0f;
    private int currentEnemyCount = 0;
    private bool isGameOver = false;

    private void OnEnable()
    {
        GameEvent.OnEnemyDestroyed += HandleEnemyDestroyed;
        GameEvent.OnGameOver += StopSpawning;
    }

    private void OnDisable()
    {
        GameEvent.OnEnemyDestroyed -= HandleEnemyDestroyed;
        GameEvent.OnGameOver -= StopSpawning;
    }

    private void Update()
    {
        if (isGameOver || spawnPoints.Length == 0 || enemyTankPrefab is null || GameManager.Instance.CurrentState != GameState.Playing) return;

        if (currentEnemyCount < maxEnemiesOnScreen)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                SpawnEnemy();
                timer = GetCurrentSpawnInterval();
            }
        }
    }

    // 生成數度
    private float GetCurrentSpawnInterval()
    {
        // 計算遊戲時間進度 (0 到 1) 
        float timeProgress = Mathf.Clamp01(GameManager.Instance.GetPlayingTime() / difficultyRampUpTime);
        
        // 使用 Lerp 根據時間進度，在初始間隔和最小間隔之間進行線性插值
        return Mathf.Lerp(initialSpawnInterval, minSpawnInterval, timeProgress);
    }

    private void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedPoint = spawnPoints[randomIndex];

        Instantiate(enemyTankPrefab, selectedPoint.position, enemyTankPrefab.transform.rotation);

        currentEnemyCount++;

        Debug.Log($"敵軍數：{currentEnemyCount}/{maxEnemiesOnScreen} | 當前生成間隔: {GetCurrentSpawnInterval():F2}s");
    }

    private void HandleEnemyDestroyed(GameObject tank,Vector3 deathPos, int score)
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
    }

    private void StopSpawning()
    {
        isGameOver = true;
    }
}