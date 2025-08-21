using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using Ubiq;
using Ubiq.Spawning;
using Ubiq.Networking;
using CesiumForUnity;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private Camera m_CameraToFace;
        [SerializeField] private List<GameObject> m_ObjectPrefabs = new List<GameObject>();
        [SerializeField] private GameObject m_SpawnVisualizationPrefab;
        [SerializeField] private int m_SpawnOptionIndex = -1;
        [SerializeField] private bool m_OnlySpawnInView = true;
        [SerializeField] private float m_ViewportPeriphery = 0.15f;
        [SerializeField] private bool m_ApplyRandomAngleAtSpawn = true;
        [SerializeField] private float m_SpawnAngleRange = 45f;
        [SerializeField] private Transform m_SpawnParent;
        [SerializeField] private NetworkSpawnManager m_NetworkSpawnManager;

        private bool readyToSpawn = false;
        public event Action<GameObject> objectSpawned;

        public Camera cameraToFace
        {
            get { EnsureFacingCamera(); return m_CameraToFace; }
            set => m_CameraToFace = value;
        }

        public List<GameObject> objectPrefabs
        {
            get => m_ObjectPrefabs;
            set => m_ObjectPrefabs = value;
        }

        public GameObject spawnVisualizationPrefab
        {
            get => m_SpawnVisualizationPrefab;
            set => m_SpawnVisualizationPrefab = value;
        }

        public int spawnOptionIndex
        {
            get => m_SpawnOptionIndex;
            set => m_SpawnOptionIndex = value;
        }

        public bool isSpawnOptionRandomized => m_SpawnOptionIndex < 0 || m_SpawnOptionIndex >= m_ObjectPrefabs.Count;

        public bool onlySpawnInView
        {
            get => m_OnlySpawnInView;
            set => m_OnlySpawnInView = value;
        }

        public float viewportPeriphery
        {
            get => m_ViewportPeriphery;
            set => m_ViewportPeriphery = value;
        }

        public bool applyRandomAngleAtSpawn
        {
            get => m_ApplyRandomAngleAtSpawn;
            set => m_ApplyRandomAngleAtSpawn = value;
        }

        public float spawnAngleRange
        {
            get => m_SpawnAngleRange;
            set => m_SpawnAngleRange = value;
        }

        public Transform spawnParent
        {
            get => m_SpawnParent;
            set => m_SpawnParent = value;
        }

        public void SetReadyToSpawn(bool ready)
        {
            readyToSpawn = ready;
        }

        public bool IsReadyToSpawn()
        {
            return readyToSpawn;
        }





        private void Awake()
        {
            EnsureFacingCamera();
            // 自动获取 NetworkSpawnManager
            if (m_NetworkSpawnManager == null)
            {
                m_NetworkSpawnManager = NetworkSpawnManager.Find(this);
                Debug.Log($"[Spawner] Auto-find NetworkSpawnManager: {m_NetworkSpawnManager}");
            }
        }

        private void EnsureFacingCamera()
        {
            if (m_CameraToFace == null)
            {
                m_CameraToFace = Camera.main;
            }
        }

        public void RandomizeSpawnOption()
        {
            m_SpawnOptionIndex = -1;
        }

        
        private string GetFullHierarchyPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private Transform GetFinalParent()
        {
            if (m_SpawnParent == null)
            {
                return this.transform;
            }

            // Look for the CesiumGeoreference component in any child of the anchor prefab
            var georef = m_SpawnParent.GetComponentInChildren<CesiumGeoreference>();
            if (georef != null)
            {
                return georef.transform;
            }

            // No CesiumGeoreference found, just parent to the anchor itself
            return m_SpawnParent;
        }
        
        
        public GameObject TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
        {
            // 调试信息
            Debug.Log($"[Spawner] TrySpawnObject ▶ networkSpawnManager={(m_NetworkSpawnManager == null ? "null" : m_NetworkSpawnManager.GetType().Name)} | prefabsCount={(m_ObjectPrefabs == null ? 0 : m_ObjectPrefabs.Count)} | spawnParent={(m_SpawnParent == null ? "null" : m_SpawnParent.name)}");
                        
            var finalParent = GetFinalParent();
            Vector3 localPos = finalParent.InverseTransformPoint(spawnPoint);

            Debug.Log($"[Spawn] parent = {finalParent.name}, worldPos = {finalParent.position}, localPos = {localPos}");
            Debug.Log($"[Spawn] parent worldRot = {finalParent.rotation.eulerAngles}");
            Debug.Log($"[Spawn] spawn worldPos = {spawnPoint}");
            Debug.Log($"[Spawn] localPos computed = {localPos}");


            // 视野检查
            if (m_OnlySpawnInView)
            {
                var vp = cameraToFace.WorldToViewportPoint(spawnPoint);
                var min = m_ViewportPeriphery;
                var max = 1f - m_ViewportPeriphery;
                if (vp.z < 0f || vp.x < min || vp.x > max || vp.y < min || vp.y > max)
                {
                    return null;
                }
            }



            var index = isSpawnOptionRandomized
                ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count)
                : m_SpawnOptionIndex;


            // 网络生成
            var newObject = m_NetworkSpawnManager.SpawnWithPeerScope(m_ObjectPrefabs[index]);
            // var netObj = newObject.GetComponent<MyNetworkedObject>();
         
            
            if (newObject == null)
            {
                Debug.LogWarning("[Spawner] 网络生成失败");
                return null;
            }

            
            // newObject.transform.position = spawnPoint;

            newObject.transform.SetParent(finalParent, worldPositionStays: false);
            newObject.transform.localPosition = localPos;

            //計算 localRotation
            var forward = m_CameraToFace.transform.position - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projected);
            Quaternion worldRotation = Quaternion.LookRotation(projected, spawnNormal);
            Quaternion localRot = Quaternion.Inverse(finalParent.rotation) * worldRotation;
            newObject.transform.localRotation = localRot;
            // newObject.transform.rotation = Quaternion.LookRotation(projected, spawnNormal);

            // // 現在才設置父物件（保持世界座標）
            // var finalParent = GetFinalParent();
            // newObject.transform.SetParent(finalParent, worldPositionStays: true);
            
    
            // // 取得設置後的本地座標（用於網路同步）
            // Vector3 localPos = newObject.transform.localPosition;
            // Quaternion localRot = newObject.transform.localRotation;


            // 取得或添加同步組件
            var worldSync = newObject.GetComponent<MyNetworkedObject>();
            if (worldSync == null)
            {
                var oldNetObj = newObject.GetComponent<MyNetworkedObject>();
                if (oldNetObj != null)
                {
                    Destroy(oldNetObj);
                }

                worldSync = newObject.AddComponent<MyNetworkedObject>();
            }
            
            if (worldSync != null)
            {
                worldSync.SetOwner(true);
            }
            
            // newObject.transform.localRotation = Quaternion.identity;
            // newObject.transform.Rotate(Vector3.up, 180f, Space.Self);

            // if (netObj != null)
            // {
            //     netObj.SetOwner(true);
            //     netObj.BroadcastInitialState(localPos, localRot, finalParent.rotation);
            // }

            // 可视化
            if (m_SpawnVisualizationPrefab != null)
            {
                var vis = Instantiate(m_SpawnVisualizationPrefab);
                vis.transform.position = spawnPoint;
                vis.transform.rotation = newObject.transform.rotation;
            }

            objectSpawned?.Invoke(newObject);
            

            var fullPath = GetFullHierarchyPath(newObject.transform);
            Debug.Log($"[Hierarchy] Spawned object '{newObject.name}' at: {fullPath}");
            return newObject;
        }
    }
}