//using System;
//using UnityEngine;

//public class EnemyFireManager : MonoBehaviour
//{
//    [Header("砲彈特效設定")]
//    public float enemyTracerSpeed = 180f;   // 敵方砲彈飛行速度 (m/s)
//    [HideInInspector] 
//    public GameObject enemyShellPrefab;
//    private void Awake()
//    {
//        enemyShellPrefab = Resources.Load<GameObject>("EnemyTraceEffect");
//    }

//    private void OnEnable()
//    {
//        GameEvent.EnemyCounterAttack += HandleEnemyCounterAttack;
//    }

//    private void OnDisable()
//    {
//        GameEvent.EnemyCounterAttack -= HandleEnemyCounterAttack;
//    }


//    private void HandleEnemyCounterAttack(GameObject enemyObject)
//    {
//        if (enemyObject == null)
//        {
//            Debug.LogWarning("【敵軍反擊中止】觸發反擊的敵軍物件已被摧毀。");
//            return;
//        }

//        if (enemyObject != this.gameObject)
//        {
//            return;
//        }

//        Transform enemyMuzzle = FindChildTarget(this.transform, "Muzzle");

//        if (Camera.main == null)
//        {
//            Debug.LogError("場景中找不到 Tag 為 'MainCamera' 的主相機！無法以相機為基準進行判定。");
//            return;
//        }
//        Transform mainCameraTransform = Camera.main.transform;
//        Vector3 cameraPos = mainCameraTransform.position;

//        Vector2 randomCirclePoint = UnityEngine.Random.insideUnitCircle * 8f;

//        Vector3 predictedImpactPoint = new Vector3(cameraPos.x + randomCirclePoint.x,cameraPos.y,cameraPos.z + randomCirclePoint.y);

//        float distanceFromCamera = Vector3.Distance(cameraPos, predictedImpactPoint);

//        Vector3 finalTracerEndPoint;

//        // ========================================================
//        // 區分「命中 (砸在相機附近)」與「未命中 (擦過鏡頭往後飛)」的視覺路徑
//        // ========================================================
//        if (distanceFromCamera <= 2f)
//        {
//            // ------------------ 【情況 A：命中核心區】 ------------------
//            // 特效終點直接鎖定在相機旁邊的這個落點！
//            finalTracerEndPoint = predictedImpactPoint;

//            Debug.Log($"<color=red>【敵軍精準打擊！】來源: {gameObject.name} | 砲彈砸在玩家鏡頭旁: {distanceFromCamera:F2} 公尺處！</color>");

//            TriggerGameOver(distanceFromCamera);
//        }
//        else
//        {
//            // ------------------ 【情況 B：未命中外圍區】 ------------------
//            // 砲彈打歪了，讓它擦過鏡頭，繼續往玩家後方無限遠處飛去
//            Vector3 muzzlePos = enemyMuzzle.position;

//            // 計算從敵方砲口指向這個打歪點的方向向量
//            Vector3 flyDirection = (predictedImpactPoint - muzzlePos).normalized;

//            // 讓終點沿著射擊方向延伸 500 公尺，強行送進玩家視野後方的虛空中
//            finalTracerEndPoint = muzzlePos + flyDirection * 500f;

//            Debug.Log($"<color=white>【敵軍打歪了】來源: {gameObject.name} | 砲彈從玩家視野旁 {distanceFromCamera:F2}m 處驚險擦過，飛向後方！</color>");
//        }

//        // 7. 生成視覺特效與彈道 (帶入由相機基準決定的終點)
//        SpawnEnemyVisualTracer(enemyMuzzle.position, finalTracerEndPoint);
//    }

//    // ==========================================
//    // 預留：供後續擴充的爆炸特效 Function
//    // ==========================================
//    public void PlayExplosionEffect(Vector3 impactPosition)
//    {
//        Debug.Log($"<color=orange>【敵方砲彈引信觸發】在平面落點座標 {impactPosition} 生成爆炸特效與彈坑！</color>");

//        // 未來擴充：
//        // Instantiate(explosionPrefab, impactPosition, Quaternion.identity);
//    }

//    private void SpawnEnemyVisualTracer(Vector3 start, Vector3 end)
//    {
//        if (enemyShellPrefab == null)
//        {
//            // 沒特效 Prefab 時，直接畫一條【紫色】Debug 線
//            Debug.DrawLine(start, end, Color.magenta, 3.0f);
//            PlayExplosionEffect(end);
//            return;
//        }

//        GameObject tracerObj = Instantiate(enemyShellPrefab, start, Quaternion.identity);
//        ShellVisualTracer tracerScript = tracerObj.GetComponent<ShellVisualTracer>();
//        if (tracerScript == null)
//        {
//            tracerScript = tracerObj.AddComponent<ShellVisualTracer>();
//        }

//        tracerScript.Initialize(start, end, enemyTracerSpeed);
//    }

//    private void TriggerGameOver(float distance)
//    {
//        Debug.LogError($"<color=red>【GAME OVER 玩家陣亡】\n" +
//                       $"[原因] 敵方砲彈砸進核心同心圓內！\n" +
//                       $"[數據] 彈著距離: {distance:F2}m (<= 2m)");

//        // GameManager.Instance.GameOver(); // 這裡放你原本的遊戲結束邏輯
//    }

//    private Transform FindChildTarget(Transform current, string targetName)
//    {
//        if (current.name == targetName) return current;

//        for (int i = 0; i < current.childCount; i++)
//        {
//            Transform found = FindChildTarget(current.GetChild(i), targetName);
//            if (found != null) return found;
//        }
//        return null;
//    }
//}
using System;
using System.Collections;
using UnityEngine;

public class EnemyFireManager : MonoBehaviour
{
    public float turretRotateSpeed = 15f;   // 砲塔（Shaft）每秒旋轉角度 (度/s)

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

        // 💡 啟動協程：進入目標鎖定、砲塔旋轉、再開火的異步工作流
        StartCoroutine(TrackingAndFireRoutine());
    }

    private IEnumerator TrackingAndFireRoutine()
    {
        // 1. 🔍 尋找關鍵組件 (拿掉 cannon，只抓 Muzzle 與 shaft)
        Transform enemyMuzzle = FindChildTarget(this.transform, "Muzzle");
        Transform enemyShaft = FindChildTarget(this.transform, "shaft");

        if (enemyMuzzle == null || enemyShaft == null)
        {
            Debug.LogError($"【敵軍反擊失敗】{gameObject.name} 身上找不到 Muzzle 或 shaft！請確認大小寫拼字。");
            yield break;
        }

        if (Camera.main == null)
        {
            Debug.LogError("場景中找不到 Tag 為 'MainCamera' 的 MainCamera！");
            yield break;
        }
        Transform mainCameraTransform = Camera.main.transform;
        Vector3 cameraPos = mainCameraTransform.position;

        // 2. 🌍 偵測地面真實高度 (Y)
        float groundY = cameraPos.y;
        RaycastHit groundHit;
        if (Physics.Raycast(cameraPos, Vector3.down, out groundHit, 20f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            groundY = groundHit.point.y;
        }
        else
        {
            groundY = mainCameraTransform.root.position.y;
        }

        // 3. 🎯 核心隨機算：決定這發砲彈的「最終預期落點」與「是否爆炸」
        Vector3 predictedImpactPoint;
        bool shouldExplode = false;
        float hitRoll = UnityEngine.Random.value;

        if (hitRoll <= 0.20f)
        {
            // 20% 命中 4x4 地面
            float randomX = UnityEngine.Random.Range(-2f, 2f);
            float randomZ = UnityEngine.Random.Range(-2f, 2f);
            predictedImpactPoint = new Vector3(cameraPos.x + randomX, groundY, cameraPos.z + randomZ);
            shouldExplode = true;
        }
        else
        {
            // 80% 側邊呼嘯而過
            float sideSign = (UnityEngine.Random.value > 0.5f) ? 1f : -1f;
            float missX = UnityEngine.Random.Range(3f, 6f) * sideSign;
            float missZ = UnityEngine.Random.Range(-4f, 4f);

            Vector3 predictedMissPoint = new Vector3(cameraPos.x + missX, cameraPos.y, cameraPos.z + missZ);

            // 計算延伸射線終點
            Vector3 initialFlyDir = (predictedMissPoint - enemyMuzzle.position).normalized;
            predictedImpactPoint = enemyMuzzle.position + initialFlyDir * 500f;
            shouldExplode = false;
        }

        Debug.Log($"<color=yellow>【瞄準開始】{gameObject.name} 鎖定目標，砲塔（Shaft）開始旋轉...</color>");

        // ========================================================
        // 🛠️ 核心修改：【純粹階段】只旋轉砲塔（Shaft）
        // ========================================================
        bool isTurretAligned = false;
        Quaternion targetShaftRot = Quaternion.identity;

        while (!isTurretAligned)
        {
            // 計算目標點相對於砲塔父物件的局部方向，並抹平Y軸只做水平旋轉
            Vector3 targetLocalForShaft = enemyShaft.parent.InverseTransformPoint(predictedImpactPoint);
            targetLocalForShaft.y = 0;

            if (targetLocalForShaft != Vector3.zero)
            {
                targetShaftRot = Quaternion.LookRotation(targetLocalForShaft, Vector3.up);
                // 執行旋轉
                enemyShaft.localRotation = Quaternion.RotateTowards(enemyShaft.localRotation, targetShaftRot, turretRotateSpeed * Time.deltaTime);
            }

            // 💡 修正：直接比較「當前旋轉角度」與「目標旋轉角度」的差距
            if (Quaternion.Angle(enemyShaft.localRotation, targetShaftRot) < 0.5f)
            {
                isTurretAligned = true;
            }

            yield return null; // 停留到下一幀，維持流暢度
        }

        // 5. 🚀 【完全對準，開火！】
        // 重新讀取轉完後的真實 muzzle 位置作為起點
        Vector3 finalMuzzlePos = enemyMuzzle.position;

        if (hitRoll <= 0.20f)
        {
            float distance = Vector3.Distance(cameraPos, predictedImpactPoint);
            Debug.Log($"<color=red>【敵軍開火 - 20%擊殺】來源: {gameObject.name} 砲彈砸中地面！距離鏡頭: {distance:F2}m</color>");
            TriggerGameOver(distance);
        }
        else
        {
            Debug.Log($"<color=white>【敵軍開火 - 80%擦過】來源: {gameObject.name} 砲彈破空呼嘯而過！</color>");
        }

        // 6. 生成射線視覺特效
        SpawnEnemyVisualTracer(finalMuzzlePos, predictedImpactPoint, shouldExplode);
    }

    public void PlayExplosionEffect(Vector3 impactPosition)
    {
        Debug.Log($"<color=orange>【敵方砲彈引信觸發】在平面落點座標 {impactPosition} 生成爆炸特效與彈坑！</color>");
    }

    private void SpawnEnemyVisualTracer(Vector3 start, Vector3 end, bool shouldExplode)
    {
        if (enemyShellPrefab == null)
        {
            Debug.DrawLine(start, end, Color.magenta, 3.0f);
            if (shouldExplode) PlayExplosionEffect(end);
            return;
        }

        GameObject tracerObj = Instantiate(enemyShellPrefab, Vector3.zero, Quaternion.identity);
        DistortionBeamEffect beamScript = tracerObj.GetComponent<DistortionBeamEffect>();

        if (beamScript == null)
        {
            beamScript = tracerObj.AddComponent<DistortionBeamEffect>();
        }

        beamScript.InitializeBeam(start, end);

        if (shouldExplode)
        {
            PlayExplosionEffect(end);
        }
    }

    private void TriggerGameOver(float distance)
    {
        Debug.LogError($"<color=red>【GAME OVER 玩家陣亡】敵方砲彈砸進 4x4 核心地面致命區！距離鏡頭: {distance:F2}m</color>");
    }

    private Transform FindChildTarget(Transform current, string targetName)
    {
        if (current.name.Equals(targetName, StringComparison.OrdinalIgnoreCase)) return current;

        for (int i = 0; i < current.childCount; i++)
        {
            Transform found = FindChildTarget(current.GetChild(i), targetName);
            if (found != null) return found;
        }
        return null;
    }
}