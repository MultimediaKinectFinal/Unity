using UnityEngine;
using UnityEngine.AI;

public class TankBrain : MonoBehaviour
{
    public enum TankState { Moving, Braking, Aiming, Firing }

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
            enemyFire.Shoot(); // 觸發開火
            fireTimer = Time.time + tankData.fireRate; // 更新冷卻時間
            currentState = TankState.Moving; // 射完變回 Moving 重新判斷
        }
    }

    private TankState lastState;


    void Awake()
    {
        Debug.Log("【坦克】成功偵測到 TankBrain 腳本正在運作！");
    }

    void Update()
    {
        // 偵錯用：確保兩者都有值
        if (player == null) {
            Debug.LogError("【報錯】Player 不見了！請檢查場景中的 Player 物件是否被誤刪或隱藏。");
            return;
        }
        if (tankData == null) {
            Debug.LogError("【報錯】TankData 遺失了！請檢查為何 ShermanData 被移除。");
            return;
        }

        if (currentState != lastState) {
        Debug.Log(gameObject.name + " 狀態切換: " + lastState + " -> " + currentState);
        lastState = currentState;
        }

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        

        Debug.Log("剩餘距離: " + agent.remainingDistance);

        switch (currentState)
        {
            case TankState.Moving:
                tankMovement.SetMoveTarget(player.position, true);
                if (distance <= tankData.attackRange && Time.time > stateChangeTime) 
                {
                    currentState = TankState.Braking;
                    stateChangeTime = Time.time + 0.5f; // 下次允許切換的時間
                }
                break;

            case TankState.Braking:
                tankMovement.SetMoveTarget(transform.position, false);

                if (Time.time > stateChangeTime) 
                {
                    currentState = TankState.Aiming;
                    stateChangeTime = Time.time + 0.5f;
                }
                break;

            case TankState.Aiming:
                // 給它一點點時間把速度歸零，再進入瞄準
                if (Time.time > stateChangeTime) 
                {
                    currentState = TankState.Firing;
                    stateChangeTime = Time.time + 0.5f;
                }
                break;

            case TankState.Firing:
                FireLogic();

                if (Time.time > stateChangeTime) 
                {
                    currentState = TankState.Aiming;
                    stateChangeTime = Time.time + 0.5f;
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