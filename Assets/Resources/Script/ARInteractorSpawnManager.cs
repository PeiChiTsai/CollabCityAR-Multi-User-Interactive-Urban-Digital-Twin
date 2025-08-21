using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    /// <summary>
    /// Spawns an object at a raycast hit position when a touch occurs,
    /// but only after a Cloud Anchor has been resolved.
    /// </summary>
    public class ARInteractorSpawnManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private ARViewManager arViewManager;

        [SerializeField]
        private ARRaycastManager raycastManager;    // [ADDED] for direct touch raycasts

        [SerializeField]
        private Spawner objectSpawner;

        private bool canSpawn = false;
        private bool allowOneSpawn = false;
        private List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        void OnEnable()
        {
            if (arViewManager != null)
            {
                // arViewManager.OnAnchorResolved.AddListener(OnResolved);
                objectSpawner.objectSpawned += OnObjectActuallySpawned;
            }
        }

        void OnDisable()
        {
            if (arViewManager != null)
            {
                // arViewManager.OnAnchorResolved.RemoveListener(OnResolved);
                objectSpawner.objectSpawned -= OnObjectActuallySpawned;
            }
        }

        private void OnObjectActuallySpawned(GameObject go)
        {
            Debug.Log($"[SpawnMgr] 收到 objectSpawned 事件 → 一次性 spawn 完成，重置标志");
            allowOneSpawn = false;
        }

        public void OnResolved()
        {
            canSpawn = true;
            Debug.Log("Cloud Anchor resolved → spawner enabled");
        }

        public void SelectShape(int prefabIndex)          // ← CHANGED
        {
            if (objectSpawner != null && 
                prefabIndex >= 0 && 
                prefabIndex < objectSpawner.objectPrefabs.Count)
            {
                objectSpawner.spawnOptionIndex = prefabIndex;
                allowOneSpawn = true;
                Debug.Log($"Shape selected: {prefabIndex}, next tap will spawn one.");
            }
        }

        private string GetHierarchyPath(Transform t)
        {
            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
        

        public void EnableNextSpawn()
        {
            allowOneSpawn = true;
        }



        void Update()
        {
            // 1. 必须先确认：已经 resolve，且用户已点选过一次才允许 spawn
            if (!canSpawn || !allowOneSpawn)
            {
                return;
            }

            // 2. 真正有触摸，并且是「新一指按下」事件
            if (Input.touchCount == 0)
            {
                return;
            }
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began)
            {
                return;
            }

            // 3. 忽略点在 UI 的情况
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            // —— 关键：一旦走到这里，就马上关掉 flag，后续再触屏都不会再进来了 ——  
            allowOneSpawn = false;

            // 4. 清空旧的射线缓存
            s_Hits.Clear();

            // 5. 真正做射线检测
            if (!raycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                Debug.Log("[SpawnMgr] Raycast miss – 尝试过一次就不再尝试");
                return;
            }

            // 6. 平面命中，执行 spawn
            var hitPose = s_Hits[0].pose;
            var spawned = objectSpawner.TrySpawnObject(
                hitPose.position,
                hitPose.rotation * Vector3.up);

            if (spawned != null)
            {
                Debug.Log("[SpawnMgr] Spawn 成功: " + spawned.name);
            }
            else
            {
                Debug.LogWarning("[SpawnMgr] Spawn 失败 (null)，需要用户重新点选才会再试一次");
            }
        }


    }
}