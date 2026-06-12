using UnityEngine;

public class TestScript : MonoBehaviour
{
    // 記得要確保你的坦克已經在場景中並且掛載了 TankHealth 腳本
    
    void Update() 
    {
        //// 1. 測試：模擬擊穿你的坦克
        //if (Input.GetKeyDown(KeyCode.Space)) 
        //{
        //    Debug.Log("偵測到 Space 鍵按下！準備觸發事件...");
        //    // 這裡抓取場景中的第一個 Tank 物件位置進行測試
        //    GameObject targetTank = GameObject.Find("Tank"); 
        //    if (targetTank != null)
        //    {
        //        GameEvent.OnArmorPenetrated?.Invoke(targetTank, "Track", 20);
        //        Debug.Log("測試：已觸發擊穿事件 (部位: Track, 傷害: 20)");
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.C)) 
        //{
        //    Debug.Log("測試：模擬擊中砲管 (部位: Cannon, 傷害: 10)");
        //    TriggerTestEvent("Cannon", 10);
        //}

        //// 2. 測試：模擬遊戲結束
        //if (Input.GetKeyDown(KeyCode.G)) 
        //{
        //    GameEvent.OnGameOver?.Invoke();
        //    Debug.Log("測試：已觸發遊戲結束事件！");
        //}
    }

    private void TriggerTestEvent(string part, int damage)
    {
        //GameObject targetTank = GameObject.Find("Tank"); 
        //if (targetTank != null)
        //{
        //    Vector3 hitPos = targetTank.transform.position;
        //    // 觸發擊穿事件，TankHealth 會監聽到這個事件
        //    GameEvent.OnArmorPenetrated?.Invoke(targetTank, part, damage);
        //}
        //else
        //{
        //    Debug.LogWarning("找不到名為 Tank 的物件！");
        //}
    }
}