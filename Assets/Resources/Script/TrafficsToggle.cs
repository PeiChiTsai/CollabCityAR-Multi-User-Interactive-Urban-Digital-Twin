using UnityEngine;
using UnityEngine.UI;

public class TrafficsToggle : MonoBehaviour
{
    

    [SerializeField]
    private Slider toggleSlider;
    private Transform _modelRoot;
    private bool _TrafficsVisible = true;

    public void ToggleTrafficsAndSlider()
    {
        toggleSlider.value = Mathf.Approximately(toggleSlider.value, 1f) ? 0f : 1f;


        if (_modelRoot == null)
        {
            // 假设 AR Cloud Anchor 创建出的根物体叫 "ARCloudAnchor"
            var go = GameObject.Find("ARCloudAnchor");
            if (go == null)
            {
                Debug.LogError("找不到名为 ARCloudAnchor 的物体，请确认运行时它已经被创建。");
                return;
            }
            _modelRoot = go.transform;
        }

        

        // 1) 在 _modelRoot 及其所有子节点中查找名为 "Traffics" 的 Transform  
        Transform Traffics = null;
        foreach (var t in _modelRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Traffics")
            {
                Traffics = t;
                break;
            }
        }

        // 2) 如果上面没找到，再全场景查一次（作为兜底）  
        if (Traffics == null)
        {
            foreach (var t in GameObject.FindObjectsOfType<Transform>())
            {
                if (t.name == "Traffics")
                {
                    Traffics = t;
                    break;
                }
            }
        }

        if (Traffics == null)
        {
            Debug.LogError("TrafficsToggle: 无法在场景中找到名为 'Traffics' 的物体，请确认名字完全匹配。");
            return;
        }

        // 3) 切换它的激活状态  
        _TrafficsVisible = !_TrafficsVisible;
        Traffics.gameObject.SetActive(_TrafficsVisible);
    }
}
