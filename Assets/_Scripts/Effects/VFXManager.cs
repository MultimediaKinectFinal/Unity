using UnityEngine;
using System;
using System.Collections; // 必填：為了使用 Coroutine

public class VFXManager : MonoBehaviour
{
    [Header("特效 Prefabs (請在 Inspector 拖曳放入)")]
    public GameObject muzzleFlashPrefab;
    public GameObject bounceSparkPrefab;
    public GameObject blockSmokePrefab;
    public GameObject penetrateSparkPrefab;
    public GameObject explosionPrefab;

    [Header("殘骸設定")]
    public Material scorchedMaterial; // 你剛剛做的焦黑材質

    private void OnEnable()
    {
        GameEvent.OnPlayerFire += SpawnMuzzleFlash;
        GameEvent.OnShellBounce += SpawnBounceSpark;
        GameEvent.OnShellBlock += SpawnBlockSmoke;
        GameEvent.OnArmorPenetrated += SpawnPenetrateSpark;
        GameEvent.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        GameEvent.OnPlayerFire -= SpawnMuzzleFlash;
        GameEvent.OnShellBounce -= SpawnBounceSpark;
        GameEvent.OnShellBlock -= SpawnBlockSmoke;
        GameEvent.OnArmorPenetrated -= SpawnPenetrateSpark;
        GameEvent.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void SpawnMuzzleFlash()
    {
        Debug.Log("生成砲口火光");
    }

    private void SpawnBounceSpark(Vector3 pos)
    {
        Instantiate(bounceSparkPrefab, pos, Quaternion.identity);
    }

    private void SpawnBlockSmoke(Vector3 pos)
    {
        Instantiate(blockSmokePrefab, pos, Quaternion.identity);
    }

    private void SpawnPenetrateSpark(Vector3 pos, string part, int damage)
    {
        Instantiate(penetrateSparkPrefab, pos, Quaternion.identity);
    }

    private void HandleEnemyDestroyed(GameObject tank, Vector3 pos, int score)
    {
        // 1. 在該座標生成大爆炸
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, pos, Quaternion.identity);

            // 讓大爆炸固定放大 5 倍
            explosion.transform.localScale = Vector3.one * 5f;

            Destroy(explosion, 3f);
        }

        // 如果有成功接收到戰車本體
        if (tank != null)
        {
            // 2. 替換為焦黑材質
            MeshRenderer[] renderers = tank.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in renderers)
            {
                r.material = scorchedMaterial;
            }

            // 3. 執行 Coroutine 讓殘骸停留後沉入地底
            StartCoroutine(SinkAndDestroy(tank));
        }
    }

    // 負責處理殘骸下沉與銷毀的 Coroutine
    private IEnumerator SinkAndDestroy(GameObject tank)
    {
        if (tank != null)
        {
            //關閉所有碰撞體
            Collider[] tankColliders = tank.GetComponentsInChildren<Collider>();
            foreach (var col in tankColliders)
            {
                col.enabled = false;
            }

            Rigidbody tankRb = tank.GetComponent<Rigidbody>();
            if (tankRb != null)
            {
                tankRb.isKinematic = true;
            }
        }

        yield return new WaitForSeconds(5f);

        // 【針對大坦克調高下沉速度】
        float sinkSpeed = 1.8f;
        float sinkDuration = 4f;
        float elapsedTime = 0f;

        while (elapsedTime < sinkDuration)
        {
            if (tank == null) yield break;

            // 持續往下移動
            tank.transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime, Space.World);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // 最後執行 Destroy，徹底從場景中移除
        if (tank != null)
        {
            Destroy(tank);
        }
    }
}