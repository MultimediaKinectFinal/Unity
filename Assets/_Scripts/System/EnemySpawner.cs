using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成設定")]
    [Tooltip("敵軍Prefab")]
    public GameObject enemyTankPrefab;

    [Tooltip("重生點物件(Transform Array)")]
    public Transform[] spawnPoints;

    [Header("生成設定")]
    [Tooltip("固定生成間隔(秒)")]
    public float spawnInterval = 1f;

    [Tooltip("場上同時最多坦克數")]
    public int maxEnemiesOnScreen = 5;

    private float timer = 0f;
    private int currentEnemyCount = 0;
    private bool isGameOver = false;

    private static Vector3 lastSpawnPos;

    private void Start()
    {
        // 加上隨機初始時間，讓多個生成器實例錯開第一次生成的時間
        timer = Random.Range(0f, spawnInterval);
    }

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
                // 每次生成後稍微加入隨機誤差，避免長時間後多個生成器步調同步
                timer = spawnInterval + Random.Range(-0.1f, 0.1f);
            }
        }
    }

    private List<int> shuffledIndices = new List<int>();

    private int GetNextSpawnIndex()
    {
        if (shuffledIndices.Count == 0)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
                shuffledIndices.Add(i);

            // Fisher-Yates 洗牌演算法
            for (int i = 0; i < shuffledIndices.Count; i++)
            {
                int temp = shuffledIndices[i];
                int randomIndex = Random.Range(i, shuffledIndices.Count);
                shuffledIndices[i] = shuffledIndices[randomIndex];
                shuffledIndices[randomIndex] = temp;
            }
        }

        int nextIndex = shuffledIndices[0];
        shuffledIndices.RemoveAt(0);
        return nextIndex;
    }

    private void SpawnEnemy()
    {
        int randomIndex = GetNextSpawnIndex();
        Transform selectedPoint = spawnPoints[randomIndex];

        // 如果有多個生成點，避免與其他生成器實例連續生成在同一個位置
        if (spawnPoints.Length > 1 && Vector3.Distance(selectedPoint.position, lastSpawnPos) < 1f)
        {
            if (shuffledIndices.Count > 0)
            {
                // 如果撞位且列表還有其他點，把這個點塞回最後，重新抽一個
                shuffledIndices.Add(randomIndex);
                randomIndex = GetNextSpawnIndex();
                selectedPoint = spawnPoints[randomIndex];
            }
            else
            {
                // 如果剛好是這輪最後一個，就直接向後順延一格
                randomIndex = (randomIndex + 1) % spawnPoints.Length;
                selectedPoint = spawnPoints[randomIndex];
            }
        }

        lastSpawnPos = selectedPoint.position;

        Instantiate(enemyTankPrefab, selectedPoint.position, enemyTankPrefab.transform.rotation);

        currentEnemyCount++;

        Debug.Log($"敵軍數：{currentEnemyCount}/{maxEnemiesOnScreen} | 當前生成間隔: {spawnInterval:F2}s");
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