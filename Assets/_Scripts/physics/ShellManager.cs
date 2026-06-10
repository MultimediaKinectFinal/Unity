using System;
using UnityEngine;

public class ShellManager : MonoBehaviour
{
    public float shellPenetration = 200f;
    public float shellDamage = 650f;

    private void OnEnable()
    {
        GameEvent.OnPlayerFire += SpawnAndCalculateShell;
    }

    private void OnDisable()
    {
        GameEvent.OnPlayerFire -= SpawnAndCalculateShell;
    }

    private void SpawnAndCalculateShell(Vector3 spawnPosition, Vector3 fireDirection)
    {
        Debug.Log("【ShellManager】接收到開火廣播！開始進行彈道計算...");

        // 穿深隨機 -10% ~ 20%
        float penRoll = Random.Range(0.90f, 1.20f);
        float finalPenetration = shellPenetration * penRoll;

        // 傷害隨機 -10% ~ +10%
        float dmgRoll = Random.Range(0.90f, 1.10f);
        float finalDamage = shellDamage * dmgRoll;

        Vector3 normalizedFireDir = fireDirection.normalized;
        RaycastHit hit;

        if (Physics.Raycast(spawnPosition, normalizedFireDir, out hit, 1000f,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            Collider hitCollider = hit.collider;

            ArmorPlate armor = hitCollider.GetComponent<ArmorPlate>();
            if (armor != null)
            {
                float cosTheta = Vector3.Dot(hit.normal, -normalizedFireDir);
                float hitAngle = Mathf.Acos(Mathf.Clamp01(cosTheta)) * Mathf.Rad2Deg;

                if (cosTheta <= 0)
                {
                    Debug.Log("從背面射入，忽略");
                    return;
                }

                if (hitAngle >= 70f)
                {
                    //Debug.Log($"<color=yellow>【跳彈】角度 {hitAngle:F1}°，砲彈滑開！</color>");
                    GameEvent.OnShellBounce?.Invoke(hit.point);
                    return;
                }

                float finalEffectiveThickness = (armor.nominalThickness / cosTheta) * armor.materialMultiplier;

                if (finalPenetration >= finalEffectiveThickness)
                {
                    //Debug.Log($"<color=red>【擊穿！】{armor.armorName} | " + $"等效厚度: {finalEffectiveThickness:F1}mm | " + $"實際穿深: {finalPenetration:F1}mm | " + $"造成 {finalDamage:F0} 點傷害！ + </color>"+$"hitpoint: {hit.point}");
                    // 廣播：擊中座標、部位名稱、傷害值
                    GameEvent.OnArmorPenetrated?.Invoke(hit.point, "Armor", Mathf.RoundToInt(finalDamage));
                }
                else
                {
                    //Debug.Log($"<color=white>【未擊穿】{armor.armorName} | " +
                    //          $"等效厚度: {finalEffectiveThickness:F1}mm | " +
                    //          $"實際穿深: {finalPenetration:F1}mm</color>");
                    GameEvent.OnShellBlock?.Invoke(hit.point);

}
                return;
            }

            TrackComponent track = hitCollider.GetComponent<TrackComponent>();
            if (track != null)
            {
                //Debug.Log($"<color=orange>【履帶失能】{track.trackName} 被擊毀！坦克失去移動能力。</color>");
                GameEvent.OnArmorPenetrated?.Invoke(hit.point, "Track", Mathf.RoundToInt(finalDamage));
                return;
            }

            BarrelComponent barrel = hitCollider.GetComponent<BarrelComponent>();
            if (barrel != null)
            {
                //Debug.Log($"<color=orange>【砲管損毀】{barrel.barrelName} 被擊毀！坦克失去開火能力。</color>");
                GameEvent.OnArmorPenetrated?.Invoke(hit.point, "Cannon", Mathf.RoundToInt(finalDamage));
                return;
            }
        }
    }
}