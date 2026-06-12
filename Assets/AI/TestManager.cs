using UnityEngine;

public class TestManager : MonoBehaviour
{
    public Transform playerTransform; // 玩家的根目錄
    private Transform finalTarget;    // 計算好的精確目標

    void Awake()
    {
        // 1. 確保只計算一次目標，避免 Update 重複計算
        if (playerTransform != null)
        {
            Transform targetPoint = playerTransform.Find("AITarget");
            finalTarget = (targetPoint != null) ? targetPoint : playerTransform;
            Debug.Log($"【Manager】目標已鎖定為: {finalTarget.name}");
        }
    }

    void Update()
    {
        // 每幀檢查，如果坦克沒有目標，就幫它補上
        AssignPlayerToTanks();
    }

    public void AssignPlayerToTanks()
    {
        if (finalTarget == null) return;

        TankBrain[] tanks = FindObjectsByType<TankBrain>(FindObjectsInactive.Include);
        foreach (TankBrain tank in tanks)
        {
            // 只有在坦克還沒設定 Player 的時候才賦予，不要每幀覆寫
            if (tank.player == null)
            {
                tank.player = finalTarget;
            }
        }
    }
}