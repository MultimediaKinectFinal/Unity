using UnityEngine;
using UnityEngine.AI; // 這是控制導航最核心的套件

public class TankMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector3 lastTarget; // 記住上一次的目標

    void Awake() { // 改用 Awake 確保先抓到 agent
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetMoveTarget(Vector3 target, bool shouldMove)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        agent.isStopped = !shouldMove; // 核心：由 Brain 決定停止還是移動
        Debug.Log("設定移動狀態: " + shouldMove);
        
        if (shouldMove)
        {
            // 只有當新目標與舊目標差異大於 0.5f 時，才重新計算路徑
            if (Vector3.Distance(lastTarget, target) > 0.5f)
            {
                agent.SetDestination(target);
                lastTarget = target;
            }
        }
    }
}