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
        // 只要玩家還在，坦克就會持續更新路徑追過去
        if (playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);
        }
    }
}