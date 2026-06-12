using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections; // 引入協程所需的命名空間

public class TankHealth : MonoBehaviour
{
    public TankData tankData;
    public float currentHP;
    public int scoreValue = 100;
    private bool isDead = false;

    public bool isCannonDamaged = false;

    public Image healthBar;          // 負責顯示血量的圖片 (fillAmount)
    public Canvas healthBarCanvas;   // 負責 Billboard 效果的 Canvas

    void Start()
    {
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

    private void HandleArmorPenetrated(GameObject hitTarget, string part, int damage)
    {
        // 關鍵：比對座標。因為場上有很多坦克，每個坦克都要檢查這發子彈是不是打到自己
        if (hitTarget == gameObject)
        {
            Debug.Log($"坦克收到擊穿事件！部位: {part}, 當前血量: {currentHP}");
            currentHP -= damage;
            UpdateHealthUI();

            if (part == "Cannon")
            {
                isCannonDamaged = true;
                Debug.Log("砲管受損！無法發射！");
                // 視覺特效可以加在這裡，例如砲管冒煙
                StartCoroutine(RepairPart(part, 20f)); // 啟動修復計時 (第二版功能：20秒)
            }

            if (part == "Track")
            {
                // 直接取得組件，若速度設為0，坦克就會停下
                var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.speed = 0;
                Debug.Log("履帶受損！坦克速度歸零！");
                StartCoroutine(RepairPart(part, 30f)); // 啟動修復計時 (第二版功能：30秒)
            }

            if (currentHP <= 0 && !isDead)
            {
                Die();
            }
        }
    }

    // ========================================================
    // 🛠️ 第二版新增功能：部件修復協程
    // ========================================================
    private IEnumerator RepairPart(string part, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (part == "Cannon")
        {
            isCannonDamaged = false;
            Debug.Log("砲管已修復！");
        }

        if (part == "Track")
        {
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null && tankData != null)
            {
                agent.speed = tankData.moveSpeed; // 恢復正常速度
                Debug.Log("履帶已修復！");
            }
        }
    }

    // ========================================================
    // 🛠️ 核心融合點：死亡邏輯與事件發送 (相容第一版與第二版的 GameEvent)
    // ========================================================
    private void Die()
    {
        isDead = true;

        // 【同時觸發兩種版本的事件】
        // 版本一：帶有三個參數 (GameObject, Vector3, int)
        GameEvent.OnEnemyDestroyed?.Invoke(gameObject, transform.position, scoreValue);

        // 版本二：帶有兩個參數 (Vector3, int)
        // GameEvent.OnEnemyDestroyed?.Invoke(transform.position, scoreValue);

        // 依據要求，不 Destroy，只禁用組件
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}