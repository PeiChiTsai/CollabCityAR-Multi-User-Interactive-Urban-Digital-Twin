using UnityEngine;
using CesiumForUnity;  // 确保你项目里有这个命名空间

/// <summary>
/// 每次物体移动时，输出它在地球上的经纬度（和高程）
/// 依赖：物体上必须挂有 CesiumGlobeAnchor 组件
/// </summary>
[RequireComponent(typeof(CesiumGlobeAnchor))]
public class LogGeoPositionOnMove : MonoBehaviour
{
    // 上一帧世界位置
    private Vector3 _lastPosition;

    // CesiumGlobeAnchor 组件引用
    private CesiumGlobeAnchor _globeAnchor;

    void Awake()
    {
        // 缓存组件
        _globeAnchor = GetComponent<CesiumGlobeAnchor>();
        if (_globeAnchor == null)
        {
            Debug.LogError($"[{nameof(LogGeoPositionOnMove)}] 缺少 CesiumGlobeAnchor 组件！");
        }

        // 初始化上一次的位置
        _lastPosition = transform.position;
    }

    void Update()
    {
        // 如果位置有变化
        if (transform.position != _lastPosition)
        {
            
            _lastPosition = transform.position;
            // 读取 CesiumGlobeAnchor 存储的经纬度+高程
            // double3 是 (longitude, latitude, height)
            var carto = _globeAnchor.longitudeLatitudeHeight;
            if (carto.y != 0.0)
            {
             // 输出到控制台
            Debug.Log($"[{name}] location → Lon: {carto.x:F6}°, Lat: {carto.y:F6}°, H: {carto.z:F2} m");
            }
        }
    }
}
