using UnityEngine;
using UnityEngine.InputSystem; // 必須新增這行來呼叫新系統

public class TestTrigger : MonoBehaviour
{
    [Header("請把場景上的戰車拖曳到這裡")]
    public GameObject targetTank;

    void Update()
    {
        // 改用新版 Input System 的鍵盤偵測寫法
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (targetTank != null)
            {
                Debug.Log("模擬觸發：發送戰車擊毀廣播！");

                // 模擬 3 號隊友呼叫 1 號建置的全域事件
                GameEvent.OnEnemyDestroyed?.Invoke(targetTank, targetTank.transform.position, 100);
            }
            else
            {
                Debug.LogWarning("請先在 Inspector 放入 Target Tank！");
            }
        }
    }
}