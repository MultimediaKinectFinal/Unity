using System;
using UnityEngine;

public class ShellManager : MonoBehaviour
{
    [Header("視覺特效引用")]
    public Transform realMuzzleTransform;
    public GameObject shellTracerPrefab;

    [Header("砲彈屬性設定")]
    public float shellPenetration = 200f;
    public float shellDamage = 650f;
    public float tracerSpeed = 250f;

    private void OnEnable()
    {
        GameEvent.OnPlayerFire += SpawnAndCalculateShell;
    }

    private void OnDisable()
    {
        GameEvent.OnPlayerFire -= SpawnAndCalculateShell;
    }

    private void SpawnAndCalculateShell(Vector3 cameraPosition, Vector3 cameraDirection)
    {
        float penRoll = UnityEngine.Random.Range(0.90f, 1.20f);
        float finalPenetration = shellPenetration * penRoll;

        float dmgRoll = UnityEngine.Random.Range(0.90f, 1.10f);
        float finalDamage = shellDamage * dmgRoll;

        Vector3 normalizedCamDir = cameraDirection.normalized;
        RaycastHit cameraHit;

        Vector3 finalImpactPoint;

        if (Physics.Raycast(cameraPosition, normalizedCamDir, out cameraHit, 1000f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            finalImpactPoint = cameraHit.point; // 特效終點定在鏡頭打到的表面
            Collider hitCollider = cameraHit.collider;
            GameObject rootObject = hitCollider.transform.root.gameObject;

            // -------------------- 狀態 1：打中裝甲板 --------------------
            ArmorPlate armor = hitCollider.GetComponent<ArmorPlate>();
            if (armor != null)
            {
                float cosTheta = Vector3.Dot(cameraHit.normal, -normalizedCamDir);
                float hitAngle = Mathf.Acos(Mathf.Clamp01(cosTheta)) * Mathf.Rad2Deg;

                if (cosTheta <= 0)
                {
                    Debug.LogWarning($"【彈道報告】砲彈從 {armor.armorName} 的背面射入，邏輯忽略。");
                    SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
                    return;
                }

                if (hitAngle >= 70f)
                {
                    Debug.Log($"<color=yellow>【跳彈 Ricochet！】部位: {armor.armorName} | 命中角度過斜: {hitAngle:F1}° | 砲彈未能咬住裝甲，直接滑開！</color> | 目標: {rootObject.name}");

                    GameEvent.OnShellBounce?.Invoke(cameraHit.point);
                    SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
                    return;
                }

                float clampedCos = Mathf.Clamp(cosTheta, 0.05f, 1.0f);
                float finalEffectiveThickness = (armor.nominalThickness / clampedCos) * armor.materialMultiplier;

                if (finalPenetration >= finalEffectiveThickness)
                {
                    Debug.Log($"<color=red>【擊穿 Penetration！】成功貫穿 {armor.armorName}！\n" +
                              $"[數據] 實際穿深: {finalPenetration:F1}mm >= 等效厚度: {finalEffectiveThickness:F1}mm (基礎: {armor.nominalThickness}mm/角度: {hitAngle:F1}°)\n" +
                              $"[結果] 造成 {finalDamage:F0} 點核心血量傷害！ | 命中點: {cameraHit.point} | 目標: {rootObject.name}</color>");

                    GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Armor", Mathf.RoundToInt(finalDamage));
                }
                else
                {
                    Debug.Log($"<color=white>【未擊穿 Blocked】砲彈未能穿透 {armor.armorName}！\n" +
                              $"[數據] 實際穿深: {finalPenetration:F1}mm < 等效厚度: {finalEffectiveThickness:F1}mm (基礎: {armor.nominalThickness}mm/角度: {hitAngle:F1}°)\n" +
                              $"[結果] 砲彈被裝甲無傷擋下！</color> | 目標: {rootObject.name}");

                    GameEvent.OnShellBlock?.Invoke(cameraHit.point);
                }
                GameEvent.EnemyCounterAttack(rootObject);
                SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
                return;
            }

            // -------------------- 狀態 2：打中履帶 --------------------
            TrackComponent track = hitCollider.GetComponent<TrackComponent>();
            if (track != null)
            {
                Debug.Log($"<color=orange>【部位破壞 - 履帶失能！】精準命中 {track.trackName}！\n" +
                          $"[結果] 觸發模組毀損，目標坦克直接失去移動能力！</color> | 目標: {rootObject.name}");

                GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Track", Mathf.RoundToInt(finalDamage));
                SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
                return;
            }

            // -------------------- 狀態 3：打中砲管 --------------------
            BarrelComponent barrel = hitCollider.GetComponent<BarrelComponent>();
            if (barrel != null)
            {
                Debug.Log($"<color=orange>【部位破壞 - 砲管損毀！】精準命中 {barrel.barrelName}！\n" +
                          $"[結果] 觸發模組毀損，目標坦克直接失去開火能力！</color> | 目標: {rootObject.name}");

                GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Cannon", Mathf.RoundToInt(finalDamage));
                SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
                return;
            }

            // -------------------- 狀態 4：打中一般地圖障礙物 --------------------
            Debug.Log($"【命中環境】砲彈打中了無防護物件: {hitCollider.name}，判定為無效人體傷害。");
            SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
        }
        else
        {
            finalImpactPoint = cameraPosition + normalizedCamDir * 1000f;
            Debug.Log("<color=cyan>【脫靶】砲彈未能命中任何目標，飛向無盡的天空。</color>");

            SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
        }
    }

    private void SpawnVisualTracer(Vector3 start, Vector3 end)
    {
        if (realMuzzleTransform == null)
        {
            Debug.LogError("【ShellManager】未設定真實砲口 realMuzzleTransform！無法生成砲彈飛行特效。");
            return;
        }

        if (shellTracerPrefab == null)
        {
            // 🛠️ 防呆：如果沒手動拉 Prefab，在場景畫一條【青藍色 Cyan】Debug 線頂替
            Debug.DrawLine(start, end, Color.cyan, 3.0f);
            return;
        }

        // 1. 🛠️ 核心修改：在世界座標中心 (Vector3.zero) 生成這個射線 Prefab
        GameObject tracerObj = Instantiate(shellTracerPrefab, Vector3.zero, Quaternion.identity);

        // 2. 🛠️ 核心修改：獲取全新寫好的「瞬間連線控制腳本」(DistortionBeamEffect)
        DistortionBeamEffect beamScript = tracerObj.GetComponent<DistortionBeamEffect>();
        if (beamScript == null)
        {
            beamScript = tracerObj.AddComponent<DistortionBeamEffect>();
        }

        // 3. 🛠️ 核心修改：瞬間將玩家的「真實砲口 (start)」與「中彈判定點 (end)」連線！
        beamScript.InitializeBeam(start, end);

        // 💡 提示：因為現在射線是 0 秒瞬間連過去的，你可以直接在這裡播放中彈音效或就地觸發受擊特效了！
    }
}









//using System;
//using UnityEngine;

//public class ShellManager : MonoBehaviour
//{
//    [Header("視覺特效引用")]
//    public Transform realMuzzleTransform;
//    public GameObject shellTracerPrefab;

//    [Header("砲彈屬性設定")]
//    public float shellPenetration = 200f;
//    public float shellDamage = 650f;
//    public float tracerSpeed = 250f;

//    private void OnEnable()
//    {
//        GameEvent.OnPlayerFire += SpawnAndCalculateShell;
//    }

//    private void OnDisable()
//    {
//        GameEvent.OnPlayerFire -= SpawnAndCalculateShell;
//    }

//    private void SpawnAndCalculateShell(Vector3 cameraPosition, Vector3 cameraDirection)
//    {
//        float penRoll = UnityEngine.Random.Range(0.90f, 1.20f);
//        float finalPenetration = shellPenetration * penRoll;

//        float dmgRoll = UnityEngine.Random.Range(0.90f, 1.10f);
//        float finalDamage = shellDamage * dmgRoll;

//        Vector3 normalizedCamDir = cameraDirection.normalized;
//        RaycastHit cameraHit;

//        Vector3 finalImpactPoint;

//        if (Physics.Raycast(cameraPosition, normalizedCamDir, out cameraHit, 1000f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
//        {
//            finalImpactPoint = cameraHit.point; // 特效第一階段終點：裝甲表面
//            Collider hitCollider = cameraHit.collider;
//            GameObject rootObject = hitCollider.transform.root.gameObject;

//            // -------------------- 狀態 1：打中裝甲板 --------------------
//            ArmorPlate armor = hitCollider.GetComponent<ArmorPlate>();
//            if (armor != null)
//            {
//                float cosTheta = Vector3.Dot(cameraHit.normal, -normalizedCamDir);
//                float hitAngle = Mathf.Acos(Mathf.Clamp01(cosTheta)) * Mathf.Rad2Deg;

//                if (cosTheta <= 0)
//                {
//                    Debug.LogWarning($"【彈道報告】砲彈從 {armor.armorName} 的背面射入，邏輯忽略。");
//                    SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
//                    return;
//                }

//                // 🛠️ 核心修改：進入跳彈分支
//                if (hitAngle >= 70f)
//                {
//                    Debug.Log($"<color=yellow>【跳彈 Ricochet！】部位: {armor.armorName} | 命中角度過斜: {hitAngle:F1}° | 砲彈未能咬住裝甲，直接滑開！</color> | 目標: {rootObject.name}");

//                    GameEvent.OnShellBounce?.Invoke(cameraHit.point);

//                    // 🛠️ 【關鍵數學公式】：計算反射向量。傳入（彈道入射方向, 撞擊面的法線向量）
//                    Vector3 reflectDirection = Vector3.Reflect(normalizedCamDir, cameraHit.normal).normalized;

//                    // 讓跳彈後的軌跡朝著反射方向高速飛往 300 公尺外的天空
//                    Vector3 ricochetEndPoint = cameraHit.point + reflectDirection * 300f;

//                    // 🛠️ 呼叫全新重構的發射器：同時傳入（砲口, 裝甲撞擊點, 跳彈終點, 是否跳彈=true）
//                    SpawnVisualTracerWithRicochet(realMuzzleTransform.position, finalImpactPoint, ricochetEndPoint, true);
//                    return;
//                }

//                float clampedCos = Mathf.Clamp(cosTheta, 0.05f, 1.0f);
//                float finalEffectiveThickness = (armor.nominalThickness / clampedCos) * armor.materialMultiplier;

//                if (finalPenetration >= finalEffectiveThickness)
//                {
//                    Debug.Log($"<color=red>【擊穿 Penetration！】成功貫穿 {armor.armorName}！\n" +
//                              $"[數據] 實際穿深: {finalPenetration:F1}mm >= 等效厚度: {finalEffectiveThickness:F1}mm\n" +
//                              $"[結果] 造成 {finalDamage:F0} 點核心血量傷害！</color>");

//                    GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Armor", Mathf.RoundToInt(finalDamage));
//                }
//                else
//                {
//                    Debug.Log($"<color=white>【未擊穿 Blocked】砲彈未能穿透 {armor.armorName}！</color>");
//                    GameEvent.OnShellBlock?.Invoke(cameraHit.point);
//                }

//                GameEvent.EnemyCounterAttack(rootObject);
//                // 正常擊穿或被擋下：只拉一條到裝甲表面的正常射線
//                SpawnVisualTracerWithRicochet(realMuzzleTransform.position, finalImpactPoint, Vector3.zero, false);
//                return;
//            }

//            // -------------------- 狀態 2：打中履帶 --------------------
//            TrackComponent track = hitCollider.GetComponent<TrackComponent>();
//            if (track != null)
//            {
//                Debug.Log($"<color=orange>【部位破壞 - 履帶失能！】精準命中 {track.trackName}！</color>");
//                GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Track", Mathf.RoundToInt(finalDamage));
//                SpawnVisualTracerWithRicochet(realMuzzleTransform.position, finalImpactPoint, Vector3.zero, false);
//                return;
//            }

//            // -------------------- 狀態 3：打中砲管 --------------------
//            BarrelComponent barrel = hitCollider.GetComponent<BarrelComponent>();
//            if (barrel != null)
//            {
//                Debug.Log($"<color=orange>【部位破壞 - 砲管損毀！】精準命中 {barrel.barrelName}！</color>");
//                GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Cannon", Mathf.RoundToInt(finalDamage));
//                SpawnVisualTracerWithRicochet(realMuzzleTransform.position, finalImpactPoint, Vector3.zero, false);
//                return;
//            }

//            // -------------------- 狀態 4：打中一般地圖障礙物 --------------------
//            Debug.Log($"【命中環境】砲彈打中了無防護物件: {hitCollider.name}");
//            SpawnVisualTracerWithRicochet(realMuzzleTransform.position, finalImpactPoint, Vector3.zero, false);
//        }
//        else
//        {
//            finalImpactPoint = cameraPosition + normalizedCamDir * 1000f;
//            Debug.Log("<color=cyan>【脫靶】砲彈飛向無盡的天空。</color>");
//            SpawnVisualTracerWithRicochet(realMuzzleTransform.position, finalImpactPoint, Vector3.zero, false);
//        }
//    }

//    // 舊的單發功能保留防呆，底層直接導向新系統
//    private void SpawnVisualTracer(Vector3 start, Vector3 end)
//    {
//        SpawnVisualTracerWithRicochet(start, end, Vector3.zero, false);
//    }

//    // ========================================================
//    // 🛠️ 全新重構：支援生成「雙段跳彈軌跡」的視覺發射器
//    // ========================================================
//    private void SpawnVisualTracerWithRicochet(Vector3 start, Vector3 hitPoint, Vector3 ricochetEnd, bool isRicochet)
//    {
//        if (realMuzzleTransform == null)
//        {
//            Debug.LogError("【ShellManager】未設定真實砲口 realMuzzleTransform！");
//            return;
//        }

//        if (shellTracerPrefab == null)
//        {
//            Debug.DrawLine(start, hitPoint, Color.cyan, 3.0f);
//            if (isRicochet) Debug.DrawLine(hitPoint, ricochetEnd, Color.yellow, 3.0f); // 沒 Prefab 時畫黃色線代表跳彈
//            return;
//        }

//        // --- 1. 生成第一條射線：砲口 -> 裝甲板面 ---
//        GameObject tracerObj1 = Instantiate(shellTracerPrefab, Vector3.zero, Quaternion.identity);
//        DistortionBeamEffect beamScript1 = tracerObj1.GetComponent<DistortionBeamEffect>();
//        if (beamScript1 == null) beamScript1 = tracerObj1.AddComponent<DistortionBeamEffect>();

//        beamScript1.InitializeBeam(start, hitPoint);

//        // --- 2. 如果判定為跳彈，立刻在同一個影格生成第二條折射射線 ---
//        if (isRicochet)
//        {
//            GameObject tracerObj2 = Instantiate(shellTracerPrefab, Vector3.zero, Quaternion.identity);
//            DistortionBeamEffect beamScript2 = tracerObj2.GetComponent<DistortionBeamEffect>();
//            if (beamScript2 == null) beamScript2 = tracerObj2.AddComponent<DistortionBeamEffect>();

//            // 🛠️ 這條線的起點是「裝甲板撞擊點 (hitPoint)」，終點是「彈開後的虛空位置 (ricochetEnd)」
//            beamScript2.InitializeBeam(hitPoint, ricochetEnd);
//        }
//    }
//}