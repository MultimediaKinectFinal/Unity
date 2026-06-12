using UnityEngine;
using System;
public static class GameEvent
{
    public static Action OnPlayerFire;
    public static Action<Vector3> OnShellBounce;
    public static Action<Vector3> OnShellBlock;
    public static Action<Vector3, string, int> OnArmorPenetrated;
    public static Action<GameObject, Vector3, int> OnEnemyDestroyed;
    public static Action OnGameOver;
}