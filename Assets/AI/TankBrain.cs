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
            Debug.Log("into fire");
            GameEvent.WhichEnemyHitPlayer?.Invoke(this.gameObject);
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

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case TankState.Moving:
                // 讓 Agent 鎖定玩家為目標
                tankMovement.SetMoveTarget(player.position, true);

                float distToPlayer = Vector3.Distance(transform.position, player.position);

                // 只要「路徑計算完成」且「物理距離」進入射程，就切換狀態
                if (!agent.pathPending && distToPlayer <= tankData.attackRange)
                {
                    currentState = TankState.Braking;
                }
                break;

            case TankState.Braking:
                // 1. 強制設定目標並讓 Agent 停下
                tankMovement.SetMoveTarget(transform.position, false); 
                
                // 2. 核心修正：將坦克強制錨定在當前位置，確保它不會再滑行
                agent.Warp(transform.position); 
                
                // 3. 確保速度歸零
                agent.velocity = Vector3.zero;
                
                currentState = TankState.Firing;
                break;

            case TankState.Firing:
                Debug.Log("in Firing");
                FireLogic();
                break;
                //FireLogic();
                //if (Time.time > stateChangeTime) {
                //    stateChangeTime = Time.time + 3.0f;
                //}
                //break;
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