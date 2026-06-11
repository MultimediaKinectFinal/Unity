using System;
using UnityEngine;

public static class GameEvent
{
    //public static Action OnPlayerFire; 
    public static Action OnGameOver;
    public static Action OnGameStart;
    
    // 跳彈<跳彈位置>
    public static Action<Vector3> OnShellBounce; 
    // 未擊穿<未擊穿位置>
    public static Action<Vector3> OnShellBlock;
    // 擊穿<擊穿座標，擊穿部位，傷害>
    public static Action<GameObject, string, int> OnArmorPenetrated;

    // 敵軍死亡<死亡座標, 擊殺分數>
    public static Action<Vector3, int> OnEnemyDestroyed;

    public static Action<int> OnUpdateScore;

    // 修改為帶有兩個 Vector3 參數的 Action (發射起點, 發射方向)
    public static Action<Vector3, Vector3> OnPlayerFire;
    public static Action<bool> OnWaitingLoad;
    public static Action<GameObject> EnemyCounterAttack;
    public static Action<Vector3, int> OnEnemyDestroyed; 

    // 擊中玩家的敵軍<敵軍位置>
    public static Action<GameObject> WhichEnemyHitPlayer; 

}