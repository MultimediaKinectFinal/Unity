using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ProceduralScope : MonoBehaviour
{
    [Header("瞄準鏡設定")]
    public int resolution = 1024;        // 圖片解析度
    public float scopeRadius = 0.4f;     // 圓圈半徑 (0~1)
    public float lineThickness = 0.0005f; // 十字線粗細

    void Start()
    {
        GenerateScopeTexture();
    }

    private void GenerateScopeTexture()
    {
        Texture2D scopeTex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        
        // 關鍵 1：取得螢幕長寬比，用來抵銷 UI 拉滿全螢幕時產生的變形
        float aspect = (float)Screen.width / Screen.height;
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // 計算與中心的距離，並強制將 X 軸乘上螢幕比例校正
                float normX = ((x - center.x) / resolution) * aspect;
                float normY = (y - center.y) / resolution;
                
                // 畢氏定理算出精準距離
                float distFromCenter = Mathf.Sqrt(normX * normX + normY * normY);

                Color pixelColor = Color.clear; // 預設透明

                // 關鍵 2：直接用大於小於判斷 (一刀切)，不留任何模糊過渡帶
                if (distFromCenter > scopeRadius)
                {
                    pixelColor = Color.black; // 外圈純黑遮罩
                }
                else
                {
                    // 在內圈透明區畫十字線 (同樣套用比例校正避免線條粗細不一)
                    if (Mathf.Abs(normX) < lineThickness || Mathf.Abs(normY) < lineThickness)
                    {
                        pixelColor = Color.black; // 十字線
                    }
                }

                scopeTex.SetPixel(x, y, pixelColor);
            }
        }

        scopeTex.Apply();
        GetComponent<RawImage>().texture = scopeTex;
    }
}