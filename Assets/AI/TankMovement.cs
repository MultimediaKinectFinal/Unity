using UnityEngine;
using UnityEngine.AI; // 這是控制導航最核心的套件

public class TankMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    void Awake() { // 改用 Awake 確保先抓到 agent
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetMoveTarget(Vector3 target, bool shouldMove)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        agent.isStopped = !shouldMove; // 核心：由 Brain 決定停止還是移動
        if (shouldMove)
        {
            agent.SetDestination(target);
        }
    }
}