using System;
using UnityEngine;

public class EnemyFireManager : MonoBehaviour
{
    [Header("砲彈特效設定")]
    public float enemyTracerSpeed = 180f;   // 敵方砲彈飛行速度 (m/s)
    [HideInInspector] 
    public GameObject enemyShellPrefab;
    private void Awake()
    {
        enemyShellPrefab = Resources.Load<GameObject>("EnemyTraceEffect");
    }

    private void OnEnable()
    {
        GameEvent.EnemyCounterAttack += HandleEnemyCounterAttack;
    }

    private void OnDisable()
    {
        GameEvent.EnemyCounterAttack -= HandleEnemyCounterAttack;
    }


    private void HandleEnemyCounterAttack(GameObject enemyObject)
    {
        if (enemyObject == null)
        {
            Debug.LogWarning("【敵軍反擊中止】觸發反擊的敵軍物件已被摧毀。");
            return;
        }

        if (enemyObject != this.gameObject)
        {
            return;
        }

        Transform enemyMuzzle = FindChildTarget(this.transform, "Muzzle");

        if (Camera.main == null)
        {
            Debug.LogError("場景中找不到 Tag 為 'MainCamera' 的主相機！無法以相機為基準進行判定。");
            return;
        }
        Transform mainCameraTransform = Camera.main.transform;
        Vector3 cameraPos = mainCameraTransform.position;

        Vector2 randomCirclePoint = UnityEngine.Random.insideUnitCircle * 8f;

        Vector3 predictedImpactPoint = new Vector3(cameraPos.x + randomCirclePoint.x,cameraPos.y,cameraPos.z + randomCirclePoint.y);

        float distanceFromCamera = Vector3.Distance(cameraPos, predictedImpactPoint);

        Vector3 finalTracerEndPoint;

        // ========================================================
        // 區分「命中 (砸在相機附近)」與「未命中 (擦過鏡頭往後飛)」的視覺路徑
        // ========================================================
        if (distanceFromCamera <= 2f)
        {
            // ------------------ 【情況 A：命中核心區】 ------------------
            // 特效終點直接鎖定在相機旁邊的這個落點！
            finalTracerEndPoint = predictedImpactPoint;

            Debug.Log($"<color=red>【敵軍精準打擊！】來源: {gameObject.name} | 砲彈砸在玩家鏡頭旁: {distanceFromCamera:F2} 公尺處！</color>");

            TriggerGameOver(distanceFromCamera);
        }
        else
        {
            // ------------------ 【情況 B：未命中外圍區】 ------------------
            // 砲彈打歪了，讓它擦過鏡頭，繼續往玩家後方無限遠處飛去
            Vector3 muzzlePos = enemyMuzzle.position;

            // 計算從敵方砲口指向這個打歪點的方向向量
            Vector3 flyDirection = (predictedImpactPoint - muzzlePos).normalized;

            // 讓終點沿著射擊方向延伸 500 公尺，強行送進玩家視野後方的虛空中
            finalTracerEndPoint = muzzlePos + flyDirection * 500f;

            Debug.Log($"<color=white>【敵軍打歪了】來源: {gameObject.name} | 砲彈從玩家視野旁 {distanceFromCamera:F2}m 處驚險擦過，飛向後方！</color>");
        }

        // 7. 生成視覺特效與彈道 (帶入由相機基準決定的終點)
        SpawnEnemyVisualTracer(enemyMuzzle.position, finalTracerEndPoint);
    }

    // ==========================================
    // 預留：供後續擴充的爆炸特效 Function
    // ==========================================
    public void PlayExplosionEffect(Vector3 impactPosition)
    {
        Debug.Log($"<color=orange>【敵方砲彈引信觸發】在平面落點座標 {impactPosition} 生成爆炸特效與彈坑！</color>");

        // 未來擴充：
        // Instantiate(explosionPrefab, impactPosition, Quaternion.identity);
    }

    private void SpawnEnemyVisualTracer(Vector3 start, Vector3 end)
    {
        if (enemyShellPrefab == null)
        {
            // 沒特效 Prefab 時，直接畫一條【紫色】Debug 線
            Debug.DrawLine(start, end, Color.magenta, 3.0f);
            PlayExplosionEffect(end);
            return;
        }

        GameObject tracerObj = Instantiate(enemyShellPrefab, start, Quaternion.identity);
        ShellVisualTracer tracerScript = tracerObj.GetComponent<ShellVisualTracer>();
        if (tracerScript == null)
        {
            tracerScript = tracerObj.AddComponent<ShellVisualTracer>();
        }

        tracerScript.Initialize(start, end, enemyTracerSpeed);
    }

    private void TriggerGameOver(float distance)
    {
        Debug.LogError($"<color=red>【GAME OVER 玩家陣亡】\n" +
                       $"[原因] 敵方砲彈砸進核心同心圓內！\n" +
                       $"[數據] 彈著距離: {distance:F2}m (<= 2m)");

        // GameManager.Instance.GameOver(); // 這裡放你原本的遊戲結束邏輯
    }

    private Transform FindChildTarget(Transform current, string targetName)
    {
        if (current.name == targetName) return current;

        for (int i = 0; i < current.childCount; i++)
        {
            Transform found = FindChildTarget(current.GetChild(i), targetName);
            if (found != null) return found;
        }
        return null;
    }
}