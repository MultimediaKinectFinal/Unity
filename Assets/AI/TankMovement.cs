using UnityEngine;
using UnityEngine.AI; // 這是控制導航最核心的套件

public class TankMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform playerTarget; // 我們會把玩家拖進來

    void Start()
    {
        // 自動抓取坦克身上的 NavMeshAgent 元件
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // 加入檢查：如果 agent 沒有被啟動 (enabled)，就直接跳出，不執行後續邏輯
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        if (playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);
        }
    }
}