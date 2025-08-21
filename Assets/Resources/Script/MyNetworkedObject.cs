using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.Networking;

public class MyNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    private NetworkContext context;
    private bool isOwner = false;

    [Header("Visual Setting")]
    public GameObject ownerMarker;
    [SerializeField] public GameObject colorTargetObject;
    [SerializeField] private string colorPropertyName = "_BaseColor";
    
    // Peer 顏色管理
    //private static Dictionary<string, Color> peerColors = new Dictionary<string, Color>();
    private static string LocalPeerId;   
    private static Dictionary<string, Color> peerColors = new Dictionary<string, Color>();

    private Color myColor = Color.white;
    private string creatorPeerId;


    // 全局静态：每台装置一次运行代表一个固定 ID
    //private static readonly string LocalPeerId = SystemInfo.deviceUniqueIdentifier;
    
    // 同步設定
    public float sendRate = 0.1f;
    private float nextSendTime;
    public float lerpSpeed = 10f;
    
    // Cloud Anchor 參考
    private static Transform cloudAnchorReference;
    private static Dictionary<Transform, Transform> anchorToNeutralMap = 
        new Dictionary<Transform, Transform>();
    
    
    // 目標位置（相對於 Cloud Anchor）
    private Vector3 targetAnchorRelativePos;
    private Quaternion targetAnchorRelativeRot;
    private Transform myNeutralParent;
    private Vector3 lastSentPosition;
    private Quaternion lastSentRotation;

    private struct AnchorRelativeMessage
    {
        public Vector3 anchorRelativePos;      // 相對於 Cloud Anchor 的位置
        public Quaternion anchorRelativeRot;   // 相對於 Cloud Anchor 的旋轉
    }

    [Serializable]
    private struct DeleteMessage
    {
        public string command;       // 固定填 "delete"
        public string id;  // 哪个对象要删
    }

    [Serializable]
    private struct ColorMessage
    {
        public string command;      // "color"
        public string networkId;    // NetworkId
        public string peerId;       // 創建者的 peer ID
        public float r, g, b, a;    // 顏色
    }
    
    // 設置全域的 Cloud Anchor 參考點
    public static void SetCloudAnchorReference(Transform anchor, Transform neutral)
    {
        cloudAnchorReference = anchor;
        if (neutral != null)
        {
            anchorToNeutralMap[anchor] = neutral;
        }
        Debug.Log($"[AnchorRelativeSync] Cloud Anchor reference set: {anchor.name}, Neutral: {neutral?.name}");
    }
    
    void Awake()
    {
        if (LocalPeerId == null)
        {
            LocalPeerId = SystemInfo.deviceUniqueIdentifier;
        }
    }
    void Start()
    {
        context = NetworkScene.Register(this);
        
        if (cloudAnchorReference == null)
        {
            Debug.LogError("[AnchorRelativeSync] No Cloud Anchor reference set!");
            return;
        }

        // 自動尋找顏色目標
        if (colorTargetObject == null)
        {
            var debugCube = transform.Find("Interaction Affordance/Debug Cube");
            if (debugCube != null)
            {
                colorTargetObject = debugCube.gameObject;
            }
        }
        
        // 找到我的 neutral parent
        FindMyNeutralParent();
        // 初始化目標位置為當前相對位置
        UpdateTargetFromCurrent();
        // 2. 然后分配“基于 ID 的”颜色
        // AssignColorFromId();
        // 設置視覺化
        UpdateVisuals();
    }
    
    public void SetOwner(bool owner)
    {
        isOwner = owner;
        if (isOwner)
        {
            // 1) 本机的 creatorPeerId 就用 LocalPeerId
            creatorPeerId = LocalPeerId;

            // 2) 如果还没给这台机选过颜色，就生成一个随机颜色并缓存
            if (!peerColors.TryGetValue(creatorPeerId, out myColor))
            {
                var rnd = new System.Random(creatorPeerId.GetHashCode());
                myColor = new Color(
                    (float)(rnd.NextDouble() * 0.7 + 0.3),
                    (float)(rnd.NextDouble() * 0.7 + 0.3),
                    (float)(rnd.NextDouble() * 0.7 + 0.3),
                    1f
                );
                peerColors[creatorPeerId] = myColor;
            }
            
            UpdateVisuals();     // 这里马上上色
            
            // 廣播顏色資訊
            BroadcastColor();
            
            // 立即同步位置
            SendAnchorRelativeState();
        }

        // 更新本地视觉
        UpdateVisuals();
    }

    private Color GeneratePeerColor()
    {
        // 直接使用 SimplePeerColor 的顏色
        if (Application.isPlaying)
        {
            return SimplePeerColor.GetMyColor();
        }
        
        // 備用方案：生成隨機顏色
        return new Color(
            UnityEngine.Random.Range(0.3f, 1f),
            UnityEngine.Random.Range(0.3f, 1f),
            UnityEngine.Random.Range(0.3f, 1f),
            1f
        );
    }
    
    private void BroadcastColor()
    {
        var colorMsg = new ColorMessage
        {
            command = "color",
            networkId = NetworkId.ToString(),
            peerId = creatorPeerId,
            r = myColor.r,
            g = myColor.g,
            b = myColor.b,
            a = myColor.a
        };
        
        context.SendJson(colorMsg);
        Debug.Log($"[MyNetworkedObject] Broadcasting color {myColor} for peer {creatorPeerId}");
    }

    
    void Update()
    {
        if (cloudAnchorReference == null) return;
        
        if (isOwner)
        {
            // 擁有者：定期發送相對於 anchor 的位置
            if (Time.time >= nextSendTime)
            {
                SendAnchorRelativeState();
                nextSendTime = Time.time + sendRate;
            }
        }
        else
        {
            // 非擁有者：根據接收到的 anchor 相對位置更新世界位置
            ApplyAnchorRelativePosition();
        }
    }
    
    private void SendAnchorRelativeState()
    {
        if (Vector3.Distance(lastSentPosition, transform.position) < 0.01f &&
        Quaternion.Angle(lastSentRotation, transform.rotation) < 1f)
        {
            return;
        }
        // 計算相對於 Cloud Anchor 的位置和旋轉
        // 注意：這裡使用世界座標轉換，因為我們要跨越不同的層級結構
        Vector3 worldPos = transform.position;
        Quaternion worldRot = transform.rotation;
        

        // 計算相對於 Cloud Anchor 的位置和旋轉
        Vector3 relativePos = cloudAnchorReference.InverseTransformPoint(worldPos);
        Quaternion relativeRot = Quaternion.Inverse(cloudAnchorReference.rotation) * worldRot;
        
        var msg = new AnchorRelativeMessage
        {
            anchorRelativePos = relativePos,
            anchorRelativeRot = relativeRot
        };
        
        context.SendJson(msg);

            // 4) **顺带再发一次颜色**（如果我是拥有者）
        if (isOwner)
        {
            var colorMsg = new ColorMessage
            {
                command   = "color",
                networkId = NetworkId.ToString(),
                peerId    = creatorPeerId,
                r = myColor.r,
                g = myColor.g,
                b = myColor.b,
                a = myColor.a
            };
            context.SendJson(colorMsg);
        }
        
        Debug.Log($"[AnchorRelativeSync] Sent relative pos: {relativePos}");
    }
    
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // —— 尝试当做删除命令 —— 
        var del = message.FromJson<DeleteMessage>();
        if (del.command == "delete" && del.id == NetworkId.ToString())
        {
            Debug.Log($"[NetworkDelete] 收到删除命令，销毁 {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        // 2. 顏色命令
        var col = message.FromJson<ColorMessage>();
        if (col.command == "color" && col.networkId == NetworkId.ToString())
        {
                // 把远端 peerId + 颜色缓存起来
            creatorPeerId = col.peerId;
            myColor      = new Color(col.r, col.g, col.b, col.a);
            peerColors[creatorPeerId] = myColor;
    
            UpdateVisuals();
            Debug.Log($"[MyNetworkedObject] Received color {myColor} for peer {col.peerId}");
            return;
        }

        if (isOwner) return;
        
        var msg = message.FromJson<AnchorRelativeMessage>();
        targetAnchorRelativePos = msg.anchorRelativePos;
        targetAnchorRelativeRot = msg.anchorRelativeRot;
        
        Debug.Log($"[AnchorRelativeSync] Received relative pos: {msg.anchorRelativePos}");
    }
    
    
    private void FindMyNeutralParent()
    {
        // 向上尋找直到找到 SpawnRoot 或已知的 neutral transform
        Transform current = transform.parent;
        while (current != null)
        {
            if (current.name == "SpawnRoot" || anchorToNeutralMap.ContainsValue(current))
            {
                myNeutralParent = current;
                Debug.Log($"[AnchorRelativeSync] Found neutral parent: {current.name}");
                break;
            }
            current = current.parent;
        }
    }



    private void ApplyAnchorRelativePosition()
    {
        // 將 anchor 相對位置轉換為世界位置
        Vector3 worldPos = cloudAnchorReference.TransformPoint(targetAnchorRelativePos);
        Quaternion worldRot = cloudAnchorReference.rotation * targetAnchorRelativeRot;
        
        // 如果有 neutral parent，需要轉換為本地座標
        if (myNeutralParent != null)
        {
            Vector3 localPos = myNeutralParent.InverseTransformPoint(worldPos);
            Quaternion localRot = Quaternion.Inverse(myNeutralParent.rotation) * worldRot;
            
            // 平滑移動到目標本地位置
            transform.localPosition = Vector3.Lerp(transform.localPosition, localPos, Time.deltaTime * lerpSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, localRot, Time.deltaTime * lerpSpeed);
        }
        else
        {
            // 沒有 neutral parent，直接使用世界座標
            transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * lerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, worldRot, Time.deltaTime * lerpSpeed);
        }
    }
    
    private void UpdateTargetFromCurrent()
    {
        if (cloudAnchorReference != null)
        {
            Vector3 worldPos = transform.position;
            Quaternion worldRot = transform.rotation;
            
            targetAnchorRelativePos = cloudAnchorReference.InverseTransformPoint(worldPos);
            targetAnchorRelativeRot = Quaternion.Inverse(cloudAnchorReference.rotation) * worldRot;
        }
    }
    
    private void UpdateVisuals()
    {
        // 更新顏色
        if (colorTargetObject != null)
        {
            var renderer = colorTargetObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 1) 创建材质实例 —— 避免改到全局材质
                renderer.material = new Material(renderer.material);
                // 2) 给实例材质设置颜色
                renderer.material.SetColor(colorPropertyName, myColor);
            }
        }

        if (ownerMarker != null)
        {
            ownerMarker.SetActive(isOwner);
        }
    }



    public void DeleteSelf()
    {
        // 1. 先发网络命令告诉大家一起删
        var msg = new DeleteMessage
        {
            command = "delete",
            id = NetworkId.ToString()
        };
        context.SendJson(msg);

        // 2. 本地立即销毁
        Destroy(gameObject);
        Debug.Log($"[{name}] Destroy.");
    }

        // 公開方法：取得創建者的 peer ID
    public string GetCreatorPeerId()
    {
        return creatorPeerId;
    }
    
    // 公開方法：取得物件顏色
    public Color GetObjectColor()
    {
        return myColor;
    }

    // Debug 用
    void OnGUI()
    {
        if (!Application.isEditor && cloudAnchorReference != null)
        {
            int yOffset = 100;
            GUI.Label(new Rect(10, yOffset, 400, 20), 
                $"Object: {gameObject.name}");
            GUI.Label(new Rect(10, yOffset + 20, 400, 20), 
                $"World Pos: {transform.position}");
            GUI.Label(new Rect(10, yOffset + 40, 400, 20), 
                $"Anchor Relative: {targetAnchorRelativePos}");
            if (myNeutralParent != null)
            {
                GUI.Label(new Rect(10, yOffset + 60, 400, 20), 
                    $"Neutral Parent: {myNeutralParent.name}");
            }
        }
    }
}