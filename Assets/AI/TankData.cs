using UnityEngine;

[CreateAssetMenu(fileName = "NewTankData", menuName = "Tank/TankData")]
public class TankData : ScriptableObject
{
    public float moveSpeed = 0.5f;
    public float fireRate = 10.0f;     // 開火間隔
    public float attackRange = 2.0f;   // 偵測距離
    public float maxHP;
}