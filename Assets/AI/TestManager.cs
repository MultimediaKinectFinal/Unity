using UnityEngine;

public class TestManager : MonoBehaviour
{
    public Transform playerTransform; 
    
    // 改用 Update 或一個簡單的定時器來確保新生成的坦克也能被抓到
    void Update()
    {
        // 簡單優化：每隔一段時間掃描一次，或者在生成坦克後呼叫這個方法
        AssignPlayerToTanks();
    }

    public void AssignPlayerToTanks()
    {
        // 尋找場景中所有活躍或不活躍的 TankBrain
        TankBrain[] tanks = FindObjectsByType<TankBrain>(FindObjectsInactive.Include);
        foreach (TankBrain tank in tanks)
        {
            // 自動賦予目標
            Transform target = tank.transform.parent?.Find("AITarget"); 
            tank.player = (target != null) ? target : playerTransform;
        }
    }
}