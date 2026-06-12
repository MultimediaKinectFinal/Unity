using UnityEngine;
using UnityEngine.InputSystem; // 使用新版輸入系統

public class TankDriveTest : MonoBehaviour
{
    [Header("車輛設定")]
    public float moveSpeed = 5f;   // 前後移動速度
    public float turnSpeed = 90f;  // 左右轉向速度

    void Update()
    {
        // 確保鍵盤存在
        if (Keyboard.current == null) return;

        // 偵測 W 和 S 控制前後
        float moveInput = 0f;
        if (Keyboard.current.wKey.isPressed) moveInput = 1f;
        if (Keyboard.current.sKey.isPressed) moveInput = -1f;

        // 偵測 A 和 D 控制左右轉向
        float turnInput = 0f;
        if (Keyboard.current.dKey.isPressed) turnInput = 1f;
        if (Keyboard.current.aKey.isPressed) turnInput = -1f;

        // 執行移動與旋轉
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
    }
}
