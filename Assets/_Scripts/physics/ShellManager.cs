//using System;
//using UnityEngine;

//public class ShellManager : MonoBehaviour
//{
//    public float shellPenetration = 200f;
//    public float shellDamage = 650f;

//    private void OnEnable()
//    {
//        GameEvent.OnPlayerFire += SpawnAndCalculateShell;
//    }

//    private void OnDisable()
//    {
//        GameEvent.OnPlayerFire -= SpawnAndCalculateShell;
//    }

//    private void SpawnAndCalculateShell(Vector3 spawnPosition, Vector3 fireDirection)
//    {
//        Debug.Log("【ShellManager】接收到開火廣播！開始進行彈道計算...");

//        // 穿深隨機 -10% ~ 20%
//        float penRoll = UnityEngine.Random.Range(0.90f, 1.20f);
//        float finalPenetration = shellPenetration * penRoll;

//        // 傷害隨機 -10% ~ +10%
//        float dmgRoll = UnityEngine.Random.Range(0.90f, 1.10f);
//        float finalDamage = shellDamage * dmgRoll;

//        Vector3 normalizedFireDir = fireDirection.normalized;
//        RaycastHit hit;

//        if (Physics.Raycast(spawnPosition, normalizedFireDir, out hit, 1000f,Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
//        {
//            Collider hitCollider = hit.collider;
//            GameObject rootObject = hitCollider.transform.root.gameObject;
//            ArmorPlate armor = hitCollider.GetComponent<ArmorPlate>();
//            if (armor != null)
//            {
//                float cosTheta = Vector3.Dot(hit.normal, -normalizedFireDir);
//                float hitAngle = Mathf.Acos(Mathf.Clamp01(cosTheta)) * Mathf.Rad2Deg;

//                if (cosTheta <= 0)
//                {
//                    Debug.Log("從背面射入，忽略");
//                    return;
//                }

//                if (hitAngle >= 70f)
//                {
//                    //Debug.Log($"<color=yellow>【跳彈】角度 {hitAngle:F1}°，砲彈滑開！</color>");
//                    GameEvent.OnShellBounce?.Invoke(hit.point);
//                    return;
//                }

//                float finalEffectiveThickness = (armor.nominalThickness / cosTheta) * armor.materialMultiplier;

//                if (finalPenetration >= finalEffectiveThickness)
//                {
//                    //Debug.Log($"<color=red>【擊穿！】{armor.armorName} | " + $"等效厚度: {finalEffectiveThickness:F1}mm | " + $"實際穿深: {finalPenetration:F1}mm | " + $"造成 {finalDamage:F0} 點傷害！ + </color>"+$"hitpoint: {hit.point}" + $"rootObjectName:{rootObject.name}");
//                    // 廣播：擊中座標、部位名稱、傷害值
//                    GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Armor", Mathf.RoundToInt(finalDamage));
//                }
//                else
//                {
//                    //Debug.Log($"<color=white>【未擊穿】{armor.armorName} | " +
//                    //          $"等效厚度: {finalEffectiveThickness:F1}mm | " +
//                    //          $"實際穿深: {finalPenetration:F1}mm</color>");
//                    GameEvent.OnShellBlock?.Invoke(hit.point);
//                }
//                return;
//            }

//            TrackComponent track = hitCollider.GetComponent<TrackComponent>();
//            if (track != null)
//            {
//                //Debug.Log($"<color=orange>【履帶失能】{track.trackName} 被擊毀！坦克失去移動能力。</color>");
//                GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Track", Mathf.RoundToInt(finalDamage));
//                return;
//            }

//            BarrelComponent barrel = hitCollider.GetComponent<BarrelComponent>();
//            if (barrel != null)
//            {
//                //Debug.Log($"<color=orange>【砲管損毀】{barrel.barrelName} 被擊毀！坦克失去開火能力。</color>");
//                GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Cannon", Mathf.RoundToInt(finalDamage));
//                return;
//            }
//        }
//    }
//}
using System;
using UnityEngine;

public class ShellManager : MonoBehaviour
{
    [Header("視覺特效引用")]
    public Transform realMuzzleTransform;  // 真正的坦克砲口位置 (特效起點)
    public GameObject shellTracerPrefab;   // 砲彈飛跡特效 Prefab (掛有 ShellVisualTracer 腳本)

    [Header("砲彈屬性設定")]
    public float shellPenetration = 200f; // 砲彈基礎穿深 (mm)
    public float shellDamage = 650f;      // 砲彈基礎傷害 (HP)
    public float tracerSpeed = 250f;       // 視覺砲彈飛行速度 (m/s)

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
        // 1. 隨機數值彈滾 (保留你的核心邏輯)
        float penRoll = UnityEngine.Random.Range(0.90f, 1.20f);
        float finalPenetration = shellPenetration * penRoll;

        float dmgRoll = UnityEngine.Random.Range(0.90f, 1.10f);
        float finalDamage = shellDamage * dmgRoll;

        Vector3 normalizedCamDir = cameraDirection.normalized;
        RaycastHit cameraHit;

        // 宣告特效最終要飛過去的「終點點位」
        Vector3 finalImpactPoint;

        // ========================================================
        // 核心判定：從「視角鏡頭」出發做 Raycast 判定 (無視差、最精準)
        // ========================================================
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

                // 70度跳彈判定
                if (hitAngle >= 70f)
                {
                    Debug.Log($"<color=yellow>【跳彈 Ricochet！】部位: {armor.armorName} | 命中角度過斜: {hitAngle:F1}° | 砲彈未能咬住裝甲，直接滑開！</color> | 目標: {rootObject.name}");

                    GameEvent.OnShellBounce?.Invoke(cameraHit.point);
                    SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
                    return;
                }

                // 計算等效厚度 (加上防呆，避免斜角分母為 0 導致數據暴走)
                float clampedCos = Mathf.Clamp(cosTheta, 0.05f, 1.0f);
                float finalEffectiveThickness = (armor.nominalThickness / clampedCos) * armor.materialMultiplier;

                // 穿深大於等效厚度 -> 擊穿
                if (finalPenetration >= finalEffectiveThickness)
                {
                    Debug.Log($"<color=red>【擊穿 Penetration！】成功貫穿 {armor.armorName}！\n" +
                              $"[數據] 實際穿深: {finalPenetration:F1}mm >= 等效厚度: {finalEffectiveThickness:F1}mm (基礎: {armor.nominalThickness}mm/角度: {hitAngle:F1}°)\n" +
                              $"[結果] 造成 {finalDamage:F0} 點核心血量傷害！ | 命中點: {cameraHit.point} | 目標: {rootObject.name}</color>");

                    GameEvent.OnArmorPenetrated?.Invoke(rootObject, "Armor", Mathf.RoundToInt(finalDamage));
                }
                // 穿深小於等效厚度 -> 未擊穿
                else
                {
                    Debug.Log($"<color=white>【未擊穿 Blocked】砲彈未能穿透 {armor.armorName}！\n" +
                              $"[數據] 實際穿深: {finalPenetration:F1}mm < 等效厚度: {finalEffectiveThickness:F1}mm (基礎: {armor.nominalThickness}mm/角度: {hitAngle:F1}°)\n" +
                              $"[結果] 砲彈被裝甲無傷擋下！</color> | 目標: {rootObject.name}");

                    GameEvent.OnShellBlock?.Invoke(cameraHit.point);
                }

                // 計算完畢，命令砲口噴出特效飛向中彈點
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
            // -------------------- 狀態 5：沒打中任何東西 (射向天空) --------------------
            finalImpactPoint = cameraPosition + normalizedCamDir * 1000f;
            Debug.Log("<color=cyan>【脫靶】砲彈未能命中任何目標，飛向無盡的天空。</color>");

            // 沒打中也要讓砲彈從砲口往天際線飛過去，視覺才不會穿幫
            SpawnVisualTracer(realMuzzleTransform.position, finalImpactPoint);
        }
    }

    // ==========================================
    // 專門負責在「真實砲口」生成視覺飛行軌跡特效的函式
    // ==========================================
    private void SpawnVisualTracer(Vector3 start, Vector3 end)
    {
        if (realMuzzleTransform == null)
        {
            Debug.LogError("【ShellManager】未設定真實砲口 realMuzzleTransform！無法生成砲彈飛行特效。");
            return;
        }

        if (shellTracerPrefab == null)
        {
            Debug.LogWarning("【ShellManager】未指定 shellTracerPrefab！將跳過飛行特效生成。");
            return;
        }

        // 在真實砲口座標生成特效
        GameObject tracerObj = Instantiate(shellTracerPrefab, start, Quaternion.identity);

        // 確保特效物件上有掛載我們先前寫好的動態飛行腳本 (ShellVisualTracer)
        ShellVisualTracer tracerScript = tracerObj.GetComponent<ShellVisualTracer>();
        if (tracerScript == null)
        {
            tracerScript = tracerObj.AddComponent<ShellVisualTracer>();
        }

        // 啟動飛行（把速度與終點餵給它，它就會自己動態滑行過去並在終點自我摧毀）
        tracerScript.Initialize(start, end, tracerSpeed);
    }
}