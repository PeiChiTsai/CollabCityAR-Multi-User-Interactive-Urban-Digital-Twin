using UnityEngine;
using UnityEngine.UI;

public class BirdsToggle : MonoBehaviour
{
    

    [SerializeField]
    private Slider toggleSlider;
    private Transform _modelRoot;
    private bool _birdsVisible = true;

    public void ToggleBirdsAndSlider()
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



        // 1) 在 _modelRoot 及其所有子节点中查找名为 "Birds" 的 Transform  
        Transform birds = null;
        foreach (var t in _modelRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Birds")
            {
                birds = t;
                break;
            }
        }

        // 2) 如果上面没找到，再全场景查一次（作为兜底）  
        if (birds == null)
        {
            foreach (var t in GameObject.FindObjectsOfType<Transform>())
            {
                if (t.name == "Birds")
                {
                    birds = t;
                    break;
                }
            }
        }

        if (birds == null)
        {
            Debug.LogError("BirdsToggle: 无法在场景中找到名为 'Birds' 的物体，请确认名字完全匹配。");
            return;
        }

        // 3) 切换它的激活状态  
        _birdsVisible = !_birdsVisible;
        birds.gameObject.SetActive(_birdsVisible);
    }
}
