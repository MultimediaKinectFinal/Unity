using UnityEngine;

public enum TankState { Moving, Braking, Aiming, Firing }

public class TankBrain : MonoBehaviour
{
    public Transform player;
    public TankData tankData; // 拖入剛剛建立的 Data
    private UnityEngine.AI.NavMeshAgent agent;
    private TankState currentState = TankState.Moving;
    private float fireTimer;
    private EnemyFire enemyFire;

    void Start() {
        enemyFire = GetComponent<EnemyFire>();
    }

    private void FireLogic() 
    {
        if (Time.time >= fireTimer) 
        {
            enemyFire.Shoot(); // 觸發開火
            fireTimer = Time.time + tankData.fireRate; // 更新冷卻時間
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case TankState.Moving:
                agent.SetDestination(player.position);
                if (distance < tankData.attackRange) currentState = TankState.Braking;
                break;

            case TankState.Braking:
                agent.isStopped = true;
                currentState = TankState.Aiming;
                break;

            case TankState.Aiming:
                // 這裡之後可以加入轉動砲塔的程式碼
                currentState = TankState.Firing;
                break;

            case TankState.Firing:
                FireLogic();
                currentState = TankState.Moving; // 射完繼續追
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
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
        // 如果有開火邏輯，也一併停止
        this.enabled = false; 
    }
}