using UnityEngine;

public class ArmorPlate : MonoBehaviour
{
    [Header("裝甲設定")]
    public string armorName = "未命名裝甲";

    [Tooltip("基礎物理厚度 (單位：毫米 mm)")]
    public float nominalThickness = 100f;

    [Header("特殊材料 (選填)")]
    [Tooltip("材料係數：均質鋼=1.0, 履帶/結構鋼=0.5, 複合裝甲=1.5")]
    public float materialMultiplier = 1.0f;
}
