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
            if (Quaternion.Angle(enemyShaft.localRotation, targetShaftRot) < 2.0f)
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