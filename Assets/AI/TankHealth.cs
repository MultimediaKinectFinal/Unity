using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
    public TankData tankData;
    public float currentHP;
    public int scoreValue = 100;
    private bool isDead = false;

    public bool isCannonDamaged = false;

    public Image healthBar;         // 負責顯示血量的圖片 (fillAmount)
    public Canvas healthBarCanvas;  // 負責 Billboard 效果的 Canvas

    void Start() {
        Debug.Log("TankHealth 腳本已經載入並啟動了！");
        currentHP = tankData.maxHP;
        Debug.Log(gameObject.name + " 的初始血量為: " + currentHP);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHP / tankData.maxHP;
        }
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null && Camera.main != null)
        {
            healthBarCanvas.transform.LookAt(Camera.main.transform);
        }
    }

    void OnEnable()
    {
        // 訂閱事件：這就是所謂的"監聽"
        GameEvent.OnArmorPenetrated += HandleArmorPenetrated;
        Debug.Log("TankHealth 已經成功訂閱 OnArmorPenetrated 事件！");
    }

    void OnDisable()
    {
        // 記得在物件銷毀或隱藏時取消訂閱，避免報錯
        GameEvent.OnArmorPenetrated -= HandleArmorPenetrated;
    }

    private void HandleArmorPenetrated(Vector3 hitPos, string part, int damage)
    {
        // 關鍵：比對座標。因為場上有很多坦克，每個坦克都要檢查這發子彈是不是打到自己
        if (Vector3.Distance(transform.position, hitPos) < 1.0f) 
        {
            Debug.Log($"坦克收到擊穿事件！部位: {part}, 當前血量: {currentHP}");
            currentHP -= damage;
            UpdateHealthUI();
            if (part == "Cannon") 
            {
                isCannonDamaged = true;
                Debug.Log("砲管受損！無法發射！");
                // 你可以在這裡加入一些視覺特效，例如砲管冒煙
            }
            
            if (part == "Track") 
            { 
                GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 0; // 斷履帶 
            }
            
            if (currentHP <= 0 && !isDead)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        isDead = true;
        // 觸發擊毀事件，通知組員 1 號加分
        GameEvent.OnEnemyDestroyed?.Invoke(transform.position, scoreValue);
        
        // 依據要求，不 Destroy，只禁用組件
        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }
}