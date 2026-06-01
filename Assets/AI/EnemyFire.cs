using UnityEngine;

public class EnemyFire : MonoBehaviour
{
    // public GameObject shellPrefab; // 把剛剛做的 Prefab 拖進來
    public Transform firePoint;    // 坦克砲管前端的空物件

    // 加上這一段測試用的 Update
    void Update()
    {
        // 按下 F 鍵測試開火
        if (Input.GetKeyDown(KeyCode.F))
        {
            Shoot();
            Debug.Log("測試：手動開火！");
        }
    }

    public void Shoot()
    {
        // 1. 在砲管位置生成砲彈
        // GameObject shell = Instantiate(shellPrefab, firePoint.position, firePoint.rotation);
        
        // 2. 給予砲彈向前推的力量
        // Rigidbody rb = shell.GetComponent<Rigidbody>();
        // rb.AddForce(firePoint.forward * 20f, ForceMode.Impulse);
        
        // 3. 3秒後銷毀子彈，避免浪費資源
        // Destroy(shell, 3f);
    }
}