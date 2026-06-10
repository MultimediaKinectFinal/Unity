using UnityEngine;

public class ShellVisualTracer : MonoBehaviour
{
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float speed;
    private float progress = 0f;
    private float totalDistance;

    public void Initialize(Vector3 start, Vector3 end, float flySpeed)
    {
        startPoint = start;
        endPoint = end;
        speed = flySpeed;
        totalDistance = Vector3.Distance(start, end);

        transform.position = start;
        // 讓特效物件朝向終點
        transform.LookAt(end);
    }

    void Update()
    {
        if (totalDistance <= 0) return;

        // 計算當前進度
        progress += (speed * Time.deltaTime) / totalDistance;

        // 使用線性插值（Lerp）讓特效沿著軌跡前進
        transform.position = Vector3.Lerp(startPoint, endPoint, progress);

        // 當抵達物理中彈點時，摧毀視覺特效
        if (progress >= 1.0f)
        {
            // 可以在這裡額外生成中彈的煙霧或爆炸粒子
            Destroy(gameObject);
        }
    }
}
