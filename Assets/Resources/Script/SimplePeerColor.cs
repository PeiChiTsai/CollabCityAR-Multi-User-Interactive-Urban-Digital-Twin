using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;

public class SimplePeerColor : MonoBehaviour
{
    [Header("顏色設定")]
    [SerializeField] private GameObject colorTargetObject;
    [SerializeField] private string colorPropertyName = "_BaseColor";
    
    // 使用設備唯一識別碼作為 ID
    private static string deviceId;
    private static Color myDeviceColor;
    private static bool colorAssigned = false;
    
    // 所有已知設備的顏色
    private static Dictionary<string, Color> deviceColors = new Dictionary<string, Color>();
    
    private MaterialPropertyBlock propertyBlock;
    private Renderer targetRenderer;
    
    // 預設顏色池
    private static readonly Color[] colorPool = new Color[]
    {
        new Color(1f, 0.2f, 0.2f, 1f),      // 紅
        new Color(0.2f, 1f, 0.2f, 1f),      // 綠
        new Color(0.2f, 0.4f, 1f, 1f),      // 藍
        new Color(1f, 0.8f, 0.2f, 1f),      // 黃
        new Color(1f, 0.2f, 1f, 1f),        // 洋紅
        new Color(0.2f, 1f, 1f, 1f),        // 青
        new Color(1f, 0.5f, 0.2f, 1f),      // 橙
        new Color(0.6f, 0.2f, 1f, 1f),      // 紫
    };
    
    void Awake()
    {
        // 生成設備 ID（只在第一次生成）
        if (string.IsNullOrEmpty(deviceId))
        {
            // 使用多個因素組合生成較穩定的 ID
            deviceId = SystemInfo.deviceUniqueIdentifier;
            
            // 如果無法取得，使用備用方案
            if (string.IsNullOrEmpty(deviceId) || deviceId == "n/a")
            {
                // 使用裝置名稱 + 隨機數
                deviceId = SystemInfo.deviceName + "_" + Random.Range(1000, 9999);
            }
            
            Debug.Log($"[SimplePeerColor] Device ID: {deviceId}");
        }
        
        // 分配顏色（只在第一次分配）
        if (!colorAssigned)
        {
            AssignDeviceColor();
        }
    }
    
    void Start()
    {
        // 自動尋找目標物件
        if (colorTargetObject == null)
        {
            var debugCube = transform.Find("Interaction Affordance/Debug Cube");
            if (debugCube != null)
            {
                colorTargetObject = debugCube.gameObject;
            }
        }
        
        // 取得 Renderer
        if (colorTargetObject != null)
        {
            targetRenderer = colorTargetObject.GetComponent<Renderer>();
        }
        
        // 初始化 MaterialPropertyBlock
        propertyBlock = new MaterialPropertyBlock();
        
        // 套用顏色
        ApplyColor();
    }
    
    private static void AssignDeviceColor()
    {
        // 使用 device ID 的 hash 來選擇顏色
        int hash = deviceId.GetHashCode();
        int colorIndex = Mathf.Abs(hash) % colorPool.Length;
        
        myDeviceColor = colorPool[colorIndex];
        
        // 加入一些變化，讓相同索引的顏色也有些微不同
        float variation = (hash % 100) / 1000f;
        myDeviceColor.r = Mathf.Clamp01(myDeviceColor.r + variation);
        myDeviceColor.g = Mathf.Clamp01(myDeviceColor.g - variation);
        
        deviceColors[deviceId] = myDeviceColor;
        colorAssigned = true;
        
        Debug.Log($"[SimplePeerColor] Assigned color {myDeviceColor} to device {deviceId}");
    }
    
    private void ApplyColor()
    {
        if (targetRenderer != null && propertyBlock != null)
        {
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(colorPropertyName, myDeviceColor);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
    
    // 公開方法
    public static Color GetMyColor()
    {
        if (!colorAssigned)
        {
            AssignDeviceColor();
        }
        return myDeviceColor;
    }
    
    public static string GetMyDeviceId()
    {
        return deviceId;
    }
    
    public void SetTargetObject(GameObject target)
    {
        colorTargetObject = target;
        if (target != null)
        {
            targetRenderer = target.GetComponent<Renderer>();
            ApplyColor();
        }
    }
    
    // 在編輯器中顯示顏色
    void OnDrawGizmosSelected()
    {
        if (colorAssigned)
        {
            Gizmos.color = myDeviceColor;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }
    }
}