using UnityEngine;
using UnityEngine.AI;

public class TankBrain : MonoBehaviour
{
    public enum TankState { Moving, Braking, Firing }

    public Transform player;
    public TankData tankData; // 拖入建立的 TankData (例如 ShermanData)

    private NavMeshAgent agent;
    private TankState currentState = TankState.Moving;
    private TankState lastState;

    private float fireTimer;
    private float stateChangeTime;
    private TankMovement tankMovement; // 引用底層移動腳本

    void Awake()
    {
        Debug.Log("【坦克】成功偵測到 TankBrain 腳本正在運作！");
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        tankMovement = GetComponent<TankMovement>();

        // 強制重置路徑，活化 Agent
        agent.enabled = false;
        agent.enabled = true;

        if (tankData != null)
        {
            agent.speed = tankData.moveSpeed;
        }
        else
        {
            Debug.LogError("【關鍵修正】TankData 仍然是空的！請再次確認 Inspector 是否顯示正確的 Data");
        }
    }

    void OnEnable()
    {
        GameEvent.OnGameOver += StopAllActions;
    }

    void OnDisable()
    {
        GameEvent.OnGameOver -= StopAllActions;
    }

    void Update()
    {
        if (player == null || tankData == null) return;

        // 只有在狀態發生改變時，才打印一次 Log
        if (currentState != lastState)
        {
            Debug.Log(gameObject.name + " 狀態切換: " + lastState + " -> " + currentState);
            lastState = currentState;
        }

        // --- 🔍 保留第一版：NavMesh 異常路徑偵測 ---
        if (agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.LogWarning($"{gameObject.name} 路徑不完整 (Partial Path)！");
        }
        else if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Debug.LogError($"{gameObject.name} 路徑無效 (Invalid Path) - Agent 找不到目標！");
        }

        // 執行狀態機
        switch (currentState)
        {
            case TankState.Moving:
                // 讓 Agent 鎖定玩家為目標
                tankMovement.SetMoveTarget(player.position, true);

                float distToPlayer = Vector3.Distance(transform.position, player.position);

                // 🛠️ 採用第二版優化：只要物理距離進入射程，強制切換至煞車狀態
                if (distToPlayer <= tankData.attackRange)
                {
                    currentState = TankState.Braking;
                }
                break;

            case TankState.Braking:
                // 1. 強制設定目標並讓 Agent 停下
                tankMovement.SetMoveTarget(transform.position, false);
                Debug.Log("【Braking】狀態持續中，進行位置錨定");

                // 2. 將坦克強制錨定在當前位置，確保它不會因為慣性繼續滑行
                agent.Warp(transform.position);

                // 3. 確保速度完全歸零
                agent.velocity = Vector3.zero;

                currentState = TankState.Firing;
                break;

            case TankState.Firing:
                FireLogic();
                if (Time.time > stateChangeTime)
                {
                    stateChangeTime = Time.time + 3.0f;
                }
                break;
        }
    }

    // --- 🛠️ 核心融合：開火邏輯 ---
    private void FireLogic()
    {
        if (Time.time >= fireTimer)
        {
            // 採用第二版事件驅動開火：傳入自己，觸發後續的砲塔旋轉與反擊工作流
            GameEvent.WhichEnemyHitPlayer?.Invoke(this.gameObject);
            fireTimer = Time.time + tankData.fireRate;
        }
    }

    private void StopAllActions()
    {
        if (agent != null)
        {
            // 停下導航
            agent.isStopped = true;
        }
        // 禁用大腦腳本，停止 Update 內的所有行為與開火
        this.enabled = false;
    }
}