using UnityEngine;

[CreateAssetMenu(fileName = "NewTankData", menuName = "Tank/TankData")]
public class TankData : ScriptableObject
{
    public float moveSpeed = 3.5f;
    public float fireRate = 2.0f;     // 開火間隔
    public float attackRange = 10f;   // 偵測距離
}