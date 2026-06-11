using System;
using UnityEngine;

public static class GameEvent
{
    public static Action OnPlayerFire; 
    public static Action OnGameOver;

    // 跳彈<跳彈位置>
    public static Action<Vector3> OnShellBounce; 
    // 未擊穿<未擊穿位置>
    public static Action<Vector3> OnShellBlock;
    // 擊穿<擊穿座標，擊穿部位，傷害>
    public static Action<GameObject, string, int> OnArmorPenetrated;

    // 敵軍死亡<死亡座標, 擊殺分數>
    public static Action<Vector3, int> OnEnemyDestroyed; 

    // 擊中玩家的敵軍<敵軍位置>
    public static Action<GameObject> WhichEnemyHitPlayer; 

}