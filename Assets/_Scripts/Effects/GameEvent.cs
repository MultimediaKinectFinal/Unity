using UnityEngine;
using System; // 必須要有 System 才能用 Action

// 企劃書規定：這是一個獨立的靜態類別，不掛載於任何物件
public static class GameEvent
{
    public static Action OnPlayerFire;
    public static Action<Vector3> OnShellBounce;
    public static Action<Vector3> OnShellBlock;
    public static Action<Vector3, string, int> OnArmorPenetrated;
    public static Action<GameObject, Vector3, int> OnEnemyDestroyed;
    public static Action OnGameOver;
}
