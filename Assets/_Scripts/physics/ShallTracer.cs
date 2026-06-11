//using UnityEngine;

//public class ShellVisualTracer : MonoBehaviour
//{
//    private Vector3 startPoint;
//    private Vector3 endPoint;
//    private float speed;
//    private float progress = 0f;
//    private float totalDistance;

//    public void Initialize(Vector3 start, Vector3 end, float flySpeed)
//    {
//        startPoint = start;
//        endPoint = end;
//        speed = flySpeed;
//        totalDistance = Vector3.Distance(start, end);

//        transform.position = start;
//        transform.LookAt(end);
//    }

//    void Update()
//    {
//        if (totalDistance <= 0) return;

//        // 計算當前進度
//        progress += (speed * Time.deltaTime) / totalDistance;

//        // 使用線性插值（Lerp）讓特效沿著軌跡前進
//        transform.position = Vector3.Lerp(startPoint, endPoint, progress);

//        // 當抵達物理中彈點時，摧毀視覺特效
//        if (progress >= 1.0f)
//        {
//            // 可以在這裡額外生成中彈的煙霧或爆炸粒子
//            Destroy(gameObject);
//        }
//    }
//}
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DistortionBeamEffect : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float lifeTime = 0.4f;       // 射線在原地留存的總時間 (秒)
    private float countdown;
    private float initialMaxWidth;       // 記錄最一開始設定的最大粗細

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // 🛠️ 核心：由 Manager 生成時呼叫，瞬間把起點與終點連起來！
    public void InitializeBeam(Vector3 startPos, Vector3 endPos)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPos); // 瞬間定死起點 (砲口)
        lineRenderer.SetPosition(1, endPos);   // 瞬間定死終點 (落點)

        // 讀取你在 Line Renderer 視窗裡調好的初始乘數寬度
        initialMaxWidth = lineRenderer.widthMultiplier;
        countdown = lifeTime;
    }

    private void Update()
    {
        if (countdown > 0)
        {
            countdown -= Time.deltaTime;

            // 計算生命週期的百分比 (從 1 慢慢降到 0)
            float normalizedTime = countdown / lifeTime;

            // 🛠️ 讓整條線在原地「整體慢慢變細」直至不見
            lineRenderer.widthMultiplier = initialMaxWidth * normalizedTime;
        }
        else
        {
            // 時間到，功成身退，自我摧毀
            Destroy(gameObject);
        }
    }
}