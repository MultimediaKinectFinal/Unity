using UnityEngine;
using UnityEngine.AI;

public class TankBrain : MonoBehaviour
{
    public enum TankState { Moving, Braking, Firing }

    public Transform player;
    public TankData tankData; // 拖入剛剛建立的 Data
    private NavMeshAgent agent;
    private TankState currentState = TankState.Moving;
    private float fireTimer;
    private EnemyFire enemyFire;
    private float stateChangeTime;

    private TankMovement tankMovement; // 引用底層移動腳本
    
    void Start() {
        enemyFire = GetComponent<EnemyFire>();
        agent = GetComponent<NavMeshAgent>();
        tankMovement = GetComponent<TankMovement>();

        // 強制重置路徑
        agent.enabled = false;
        agent.enabled = true;
        
        if (tankData != null) {
            agent.speed = tankData.moveSpeed;
        } else {
            Debug.LogError("【關鍵修正】TankData 仍然是空的！請再次確認 Inspector 是否顯示 ShermanData");
        }
    }
    private void FireLogic() 
    {
        if (Time.time >= fireTimer) 
        {
            Debug.Log("【Firing2】狀態持續中");

            enemyFire.Shoot();
            fireTimer = Time.time + tankData.fireRate;
        }
    }

    private TankState lastState;


    void Awake()
    {
        Debug.Log("【坦克】成功偵測到 TankBrain 腳本正在運作！");
    }

    void Update()
    {
        if (player == null || tankData == null) return;

        // 只有在狀態發生改變時，才打印一次 Log
        if (currentState != lastState) {
            Debug.Log(gameObject.name + " 狀態切換: " + lastState + " -> " + currentState);
            lastState = currentState;
        }

        // 刪除那兩行無用的強制重置代碼

        if (agent.pathStatus == NavMeshPathStatus.PathPartial) {
            Debug.LogWarning("路徑不完整 (Partial Path)");
        } else if (agent.pathStatus == NavMeshPathStatus.PathInvalid) {
            Debug.LogWarning("路徑無效 (Invalid Path) - Agent 找不到目標！");
        }

        float distance = Vector3.Distance(transform.position, player.position);
        

        //Debug.Log("剩餘距離: " + agent.remainingDistance);

        //Debug.Log("Agent Speed: " + agent.speed + " | CurrentState: " + currentState);

        switch (currentState)
        {
            case TankState.Moving:
                // 讓 Agent 鎖定玩家為目標
                Debug.Log("【Moving】狀態持續中");
                tankMovement.SetMoveTarget(player.position, true);

                float distToPlayer = Vector3.Distance(transform.position, player.position);

                Debug.Log("當前距離: " + distToPlayer + " | 設定射程: " + tankData.attackRange);

                // 只要「路徑計算完成」且「物理距離」進入射程，就切換狀態
                // if (!agent.pathPending && distToPlayer <= tankData.attackRange)
                // {
                //     currentState = TankState.Braking;
                //     Debug.Log("【Moving, state change】狀態持續中");

                // }

                // 檢查條件：如果距離足夠，強制觸發切換
                if (distToPlayer <= tankData.attackRange)
                {
                    Debug.Log($"<color=green>【偵測到目標】距離 {distToPlayer:F2} 小於射程 {tankData.attackRange}，切換至 Braking</color>");
                    currentState = TankState.Braking; 
                }
                else
                {
                    // 額外除錯：如果一直沒切換，告訴你為什麼
                    if (Time.frameCount % 100 == 0) // 每 100 幀印一次，避免洗版
                        Debug.Log($"【移動中】目前距離: {distToPlayer:F2} (射程: {tankData.attackRange})");
                }

                break;

            case TankState.Braking:
                // 1. 強制設定目標並讓 Agent 停下
                tankMovement.SetMoveTarget(transform.position, false); 
                Debug.Log("【Braking】狀態持續中");

                
                // 2. 核心修正：將坦克強制錨定在當前位置，確保它不會再滑行
                agent.Warp(transform.position); 
                
                // 3. 確保速度歸零
                agent.velocity = Vector3.zero;
                
                currentState = TankState.Firing;
                break;

            case TankState.Firing:
                FireLogic();
                Debug.Log("【Firing】狀態持續中");
                if (Time.time > stateChangeTime) {
                    stateChangeTime = Time.time + 3.0f;
                }
                break;
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

    private void StopAllActions()
    {
        // 這裡要停下導航
        agent.isStopped = true;
        // 如果有開火邏輯，也一併停止
        this.enabled = false; 
    }
}