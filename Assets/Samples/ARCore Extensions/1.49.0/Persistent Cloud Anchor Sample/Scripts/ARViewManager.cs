//-----------------------------------------------------------------------
// <copyright file="ARViewManager.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    
    using Unity.XR.CoreUtils;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    using UnityEngine.XR.Interaction.Toolkit;
    using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
    using Ubiq;
    using Ubiq.Messaging;
    using Ubiq.Spawning;
    

    public class ARViewManager : MonoBehaviour
    {
        [Header("Anchor Events")]
        public UnityEvent OnAnchorHosted;
        public UnityEvent OnAnchorResolved;


        public XROrigin xrOrigin;
        public PersistentCloudAnchorsController Controller;
        public GameObject CloudAnchorPrefab;

        /// <summary>
        /// The game object that includes <see cref="MapQualityIndicator"/> to visualize
        /// map quality result.
        /// </summary>
        public GameObject MapQualityIndicatorPrefab;

        /// <summary>
        /// The UI element that displays the instructions to guide hosting experience.
        /// </summary>
        public GameObject InstructionBar;

        /// <summary>
        /// The UI panel that allows the user to name the Cloud Anchor.
        /// </summary>
        public GameObject NamePanel;

        /// <summary>
        /// The UI element that displays warning message for invalid input name.
        /// </summary>
        public GameObject InputFieldWarning;

        /// <summary>
        /// The input field for naming Cloud Anchor.
        /// </summary>
        public InputField NameField;

        /// <summary>
        /// The instruction text in the top instruction bar.
        /// </summary>
        public Text InstructionText;

        /// <summary>
        /// Display the tracking helper text when the session in not tracking.
        /// </summary>
        public Text TrackingHelperText;

        /// <summary>
        /// The debug text in bottom snack bar.
        /// </summary>
        public Text DebugText;

        /// <summary>
        /// The button to save the typed name.
        /// </summary>
        public Button SaveButton;

        /// <summary>
        /// The button to save current cloud anchor id into clipboard.
        /// </summary>
        public Button ShareButton;

        [Header("Root for all AR objects")]
        [Tooltip("Assign the GameObject named 'ARObject' here.")]
        public Transform arObjectLayer;
        public Spawner Spawner;

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Initializing">.</see>
        /// </summary>
        private const string _initializingMessage = "Tracking is being initialized.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Relocalizing">.</see>
        /// </summary>
        private const string _relocalizingMessage = "Tracking is resuming after an interruption.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientLight">.</see>
        /// </summary>
        private const string _insufficientLightMessage = "Too dark. Try moving to a well-lit area.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientLight">
        /// in Android S or above.</see>
        /// </summary>
        private const string _insufficientLightMessageAndroidS =
            "Too dark. Try moving to a well-lit area. " +
            "Also, make sure the Block Camera is set to off in system settings.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientFeatures">.</see>
        /// </summary>
        private const string _insufficientFeatureMessage =
            "Can't find anything. Aim device at a surface with more texture or color.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.ExcessiveMotion">.</see>
        /// </summary>
        private const string _excessiveMotionMessage = "Moving too fast. Slow down.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Unsupported">.</see>
        /// </summary>
        private const string _unsupportedMessage = "Tracking lost reason is not supported.";

        /// <summary>
        /// The time between enters AR View and ARCore session starts to host or resolve.
        /// </summary>
        private const float _startPrepareTime = 3.0f;

        /// <summary>
        /// Android 12 (S) SDK version.
        /// </summary>
        private const int _androidSSDKVesion = 31;

        /// <summary>
        /// Pixel Model keyword.
        /// </summary>
        private const string _pixelModel = "pixel";

        /// <summary>
        /// The timer to indicate whether the AR View has passed the start prepare time.
        /// </summary>
        private float _timeSinceStart;

        /// <summary>
        /// True if the app is in the process of returning to home page due to an invalid state,
        /// otherwise false.
        /// </summary>
        private bool _isReturning;

        /// <summary>
        /// The MapQualityIndicator that attaches to the placed object.
        /// </summary>
        private MapQualityIndicator _qualityIndicator = null;

        /// <summary>
        /// The history data that represents the current hosted Cloud Anchor.
        /// </summary>
        private CloudAnchorHistory _hostedCloudAnchor;

        /// <summary>
        /// An ARAnchor indicating the 3D object has been placed on a flat surface and
        /// is waiting for hosting.
        /// </summary>
        private ARAnchor _anchor = null;

        /// <summary>
        /// The promise for the async hosting operation, if any.
        /// </summary>
        private HostCloudAnchorPromise _hostPromise = null;

        /// <summary>
        /// The result of the hosting operation, if any.
        /// </summary>
        private HostCloudAnchorResult _hostResult = null;

        /// <summary>
        /// The coroutine for the hosting operation, if any.
        /// </summary>
        private IEnumerator _hostCoroutine = null;

        /// <summary>
        /// The promises for the async resolving operations, if any.
        /// </summary>
        private List<ResolveCloudAnchorPromise> _resolvePromises =
            new List<ResolveCloudAnchorPromise>();

        /// <summary>
        /// The results of the resolving operations, if any.
        /// </summary>
        private List<ResolveCloudAnchorResult> _resolveResults =
            new List<ResolveCloudAnchorResult>();

        /// <summary>
        /// The coroutines of the resolving operations, if any.
        /// </summary>
        private List<IEnumerator> _resolveCoroutines = new List<IEnumerator>();

        private Color _activeColor;
        private AndroidJavaClass _versionInfo;

        private ARSessionOrigin _sessionOrigin;
        private Transform     _resolvedAnchorTransform;

        private bool hasAligned = false;

        /// <summary>
        /// Get the camera pose for the current frame.
        /// </summary>
        /// <returns>The camera pose of the current frame.</returns>
        public Pose GetCameraPose()
        {
            return new Pose(Controller.MainCamera.transform.position,
                Controller.MainCamera.transform.rotation);
        }

        /// <summary>
        /// Callback handling the validation of the input field.
        /// </summary>
        /// <param name="inputString">The current value of the input field.</param>
        public void OnInputFieldValueChanged(string inputString)
        {
            // Cloud Anchor name should only contains: letters, numbers, hyphen(-), underscore(_).
            var regex = new Regex("^[a-zA-Z0-9-_]*$");
            InputFieldWarning.SetActive(!regex.IsMatch(inputString));
            SetSaveButtonActive(!InputFieldWarning.activeSelf && inputString.Length > 0);
        }

        /// <summary>
        /// Callback handling "Ok" button click event for input field.
        /// </summary>
        public void OnSaveButtonClicked()
        {
            _hostedCloudAnchor.Name = NameField.text;
            Controller.SaveCloudAnchorHistory(_hostedCloudAnchor);

            DebugText.text = string.Format("Saved Cloud Anchor:\n{0}.", _hostedCloudAnchor.Name);
            ShareButton.gameObject.SetActive(true);
            NamePanel.SetActive(false);
        }

        /// <summary>
        /// Callback handling "Share" button click event.
        /// </summary>
        public void OnShareButtonClicked()
        {
            GUIUtility.systemCopyBuffer = _hostedCloudAnchor.Id;
            DebugText.text = "Copied cloud id: " + _hostedCloudAnchor.Id;
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _activeColor = SaveButton.GetComponentInChildren<Text>().color;
            _versionInfo = new AndroidJavaClass("android.os.Build$VERSION");
            _sessionOrigin = FindObjectOfType<ARSessionOrigin>();
        }

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            _timeSinceStart = 0.0f;
            _isReturning = false;
            _anchor = null;
            _qualityIndicator = null;
            _hostPromise = null;
            _hostResult = null;
            _hostCoroutine = null;
            _resolvePromises.Clear();
            _resolveResults.Clear();
            _resolveCoroutines.Clear();

            InstructionBar.SetActive(true);
            NamePanel.SetActive(false);
            InputFieldWarning.SetActive(false);
            ShareButton.gameObject.SetActive(false);
            UpdatePlaneVisibility(true);

            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Ready:
                    ReturnToHomePage("Invalid application mode, returning to home page...");
                    break;
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    InstructionText.text = "Detecting flat surface...";
                    DebugText.text = "ARCore is preparing for " + Controller.Mode;
                    break;
            }
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            if (_qualityIndicator != null)
            {
                Destroy(_qualityIndicator.gameObject);
                _qualityIndicator = null;
            }

            if (_anchor != null)
            {
                Destroy(_anchor.gameObject);
                _anchor = null;
            }

            if (_hostCoroutine != null)
            {
                StopCoroutine(_hostCoroutine);
            }

            _hostCoroutine = null;

            if (_hostPromise != null)
            {
                _hostPromise.Cancel();
                _hostPromise = null;
            }

            _hostResult = null;

            foreach (var coroutine in _resolveCoroutines)
            {
                StopCoroutine(coroutine);
            }

            _resolveCoroutines.Clear();

            foreach (var promise in _resolvePromises)
            {
                promise.Cancel();
            }

            _resolvePromises.Clear();

            foreach (var result in _resolveResults)
            {
                if (result.Anchor != null)
                {
                    Destroy(result.Anchor.gameObject);
                }
            }

            _resolveResults.Clear();
            UpdatePlaneVisibility(false);
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (!hasAligned && _resolvedAnchorTransform != null)
            {
                AlignContentToAnchor(_resolvedAnchorTransform);
                hasAligned = true;
            }
        
            // Give ARCore some time to prepare for hosting or resolving.
            if (_timeSinceStart < _startPrepareTime)
            {
                _timeSinceStart += Time.deltaTime;
                if (_timeSinceStart >= _startPrepareTime)
                {
                    UpdateInitialInstruction();
                }

                return;
            }

            ARCoreLifecycleUpdate();
            if (_isReturning)
            {
                return;
            }

            if (_timeSinceStart >= _startPrepareTime)
            {
                DisplayTrackingHelperMessage();
            }

            if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Resolving)
            {
                ResolvingCloudAnchors();
            }
            else if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Hosting)
            {
                // Perform hit test and place an anchor on the hit test result.
                if (_anchor == null)
                {
                    // If the player has not touched the screen then the update is complete.
                    Touch touch;
                    if (Input.touchCount < 1 ||
                        (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
                    {
                        return;
                    }

                    // Ignore the touch if it's pointing on UI objects.
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return;
                    }

                    // Perform hit test and place a pawn object.
                    PerformHitTest(touch.position);
                }

                HostingCloudAnchor();
            }
        }

        private void PerformHitTest(Vector2 touchPos)
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            Controller.RaycastManager.Raycast(
                touchPos, hitResults, TrackableType.PlaneWithinPolygon);
            if (hitResults.Count == 0)
            {
                return;
            }

            // If there was an anchor placed, then instantiate the corresponding object.
            var planeType = PlaneAlignment.HorizontalUp;
            if (hitResults.Count > 0)
            {
                ARPlane plane = Controller.PlaneManager.GetPlane(hitResults[0].trackableId);
                if (plane == null)
                {
                    Debug.LogWarningFormat("Failed to find the ARPlane with TrackableId {0}",
                        hitResults[0].trackableId);
                    return;
                }

                planeType = plane.alignment;
                var hitPose = hitResults[0].pose;
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // Point the hitPose rotation roughly away from the raycast/camera
                    // to match ARCore.
                    hitPose.rotation.eulerAngles =
                        new Vector3(0.0f, Controller.MainCamera.transform.eulerAngles.y, 0.0f);
                }

                _anchor = Controller.AnchorManager.AttachAnchor(plane, hitPose);
                Debug.Log($"[Hierarchy] Anchor placed at: {GetFullHierarchyPath(_anchor.transform)}");
            }

            if (_anchor != null)
            {
                ///_anchor.transform.SetParent(arObjectLayer, worldPositionStays: true);
                Spawner.spawnParent = _anchor.transform;

                var cloudGO = Instantiate(CloudAnchorPrefab,
                    _anchor.transform.position,
                    _anchor.transform.rotation,
                    _anchor.transform);

                Transform anchorRoot = cloudGO.transform;
                
                // 在它下面再找出你自己放的“CesiumGeoreference”节点（第2层）
                //var geo = cloudGO.transform.Find("CesiumGeoreference");
                var geo = anchorRoot.Find("CesiumGeoreference");

                if (geo != null)
                {
                    anchorRoot = geo;
                }

                // 2) 创建 neutral 容器
                var neutral = CreateNeutralSpawnRoot(anchorRoot);

                // 3) 把 spawnParent 指向 neutral
                Spawner.spawnParent = neutral;






                //Instantiate(CloudAnchorPrefab, _anchor.transform);

                // Attach map quality indicator to this anchor.
                var indicatorGO =
                    Instantiate(MapQualityIndicatorPrefab, _anchor.transform);
                _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                _qualityIndicator.DrawIndicator(planeType, Controller.MainCamera);

                InstructionText.text = " To save this location, walk around the object to " +
                    "capture it from different angles";
                DebugText.text = "Waiting for sufficient mapping quaility...";

                // Hide plane generator so users can focus on the object they placed.
                UpdatePlaneVisibility(false);
            }
        }

        private void HostingCloudAnchor()
        {
            // There is no anchor for hosting.
            if (_anchor == null)
            {
                return;
            }

            // There is a pending or finished hosting task.
            if (_hostPromise != null || _hostResult != null)
            {
                return;
            }

            // Update map quality:
            int qualityState = 2;
            // Can pass in ANY valid camera pose to the mapping quality API.
            // Ideally, the pose should represent users’ expected perspectives.
            FeatureMapQuality quality =
                Controller.AnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
            DebugText.text = "Current mapping quality: " + quality;
            qualityState = (int)quality;
            _qualityIndicator.UpdateQualityState(qualityState);

            // Hosting instructions:
            var cameraDist = (_qualityIndicator.transform.position -
                Controller.MainCamera.transform.position).magnitude;
            if (cameraDist < _qualityIndicator.Radius * 1.5f)
            {
                InstructionText.text = "You are too close, move backward.";
                return;
            }
            else if (cameraDist > 10.0f)
            {
                InstructionText.text = "You are too far, come closer.";
                return;
            }
            else if (_qualityIndicator.ReachTopviewAngle)
            {
                InstructionText.text =
                    "You are looking from the top view, move around from all sides.";
                return;
            }
            else if (!_qualityIndicator.ReachQualityThreshold)
            {
                InstructionText.text = "Save the object here by capturing it from all sides.";
                return;
            }

            // Start hosting:
            InstructionText.text = "Processing...";
            DebugText.text = "Mapping quality has reached sufficient threshold, " +
                "creating Cloud Anchor.";
            DebugText.text = string.Format(
                "FeatureMapQuality has reached {0}, triggering CreateCloudAnchor.",
                Controller.AnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose()));

            // Creating a Cloud Anchor with lifetime = 1 day.
            // This is configurable up to 365 days when keyless authentication is used.
            var promise = Controller.AnchorManager.HostCloudAnchorAsync(_anchor, 1);
            if (promise.State == PromiseState.Done)
            {
                Debug.LogFormat("Failed to host a Cloud Anchor.");
                OnAnchorHostedFinished(false);
            }
            else
            {
                _hostPromise = promise;
                _hostCoroutine = HostAnchor();
                StartCoroutine(_hostCoroutine);
            }
        }

        private IEnumerator HostAnchor()
        {
            yield return _hostPromise;
            _hostResult = _hostPromise.Result;
            _hostPromise = null;

            if (_hostResult.CloudAnchorState == CloudAnchorState.Success)
            {
                int count = Controller.LoadCloudAnchorHistory().Collection.Count;
                _hostedCloudAnchor =
                    new CloudAnchorHistory("CloudAnchor" + count, _hostResult.CloudAnchorId);
                OnAnchorHostedFinished(true, _hostResult.CloudAnchorId);
            }
            else
            {
                OnAnchorHostedFinished(false, _hostResult.CloudAnchorState.ToString());
            }
        }

        private void ResolvingCloudAnchors()
        {
            // No Cloud Anchor for resolving.
            if (Controller.ResolvingSet.Count == 0)
            {
                return;
            }

            // There are pending or finished resolving tasks.
            if (_resolvePromises.Count > 0 || _resolveResults.Count > 0)
            {
                return;
            }

            // ARCore session is not ready for resolving.
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            Debug.LogFormat("Attempting to resolve {0} Cloud Anchor(s): {1}",
                Controller.ResolvingSet.Count,
                string.Join(",", new List<string>(Controller.ResolvingSet).ToArray()));
            foreach (string cloudId in Controller.ResolvingSet)
            {
                var promise = Controller.AnchorManager.ResolveCloudAnchorAsync(cloudId);
                if (promise.State == PromiseState.Done)
                {
                    Debug.LogFormat("Faild to resolve Cloud Anchor " + cloudId);
                    OnAnchorResolvedFinished(false, cloudId);
                }
                else
                {
                    _resolvePromises.Add(promise);
                    var coroutine = ResolveAnchor(cloudId, promise);
                    StartCoroutine(coroutine);
                }
            }

            Controller.ResolvingSet.Clear();
        }


        void AlignContentToAnchor(Transform anchor)
        {
            var origin = FindObjectOfType<XROrigin>();
            if (origin == null)
            {
                Debug.LogError("找不到 XROrigin！");
                return;
            }

            xrOrigin.transform.position = -anchor.position;
            xrOrigin.transform.rotation = Quaternion.Inverse(anchor.rotation);
                
            // // 1. 取得 XR Camera 與 Origin 的相對位置
            // var cameraInOrigin = xrOrigin.Camera.transform.localPosition;
            // var cameraInWorld = xrOrigin.Camera.transform.position;

            // // 2. 目標是：讓 anchor 成為新的 (0,0,0)，所以 XR Origin 要移動到相對 offset
            // var offset = cameraInWorld - anchor.position;
            // xrOrigin.transform.position -= offset;

            // // 3. 如果你還希望方向一致，也可以套用旋轉（可選）
            // xrOrigin.transform.rotation = Quaternion.Inverse(anchor.rotation);

        }



        public static Transform CreateNeutralSpawnRoot(Transform anchorRoot)
        {
            // 新建空物体
            var neutralGO = new GameObject("SpawnRoot");
            // 将它 parent 到 CloudAnchorPrefab 上，不保留世界坐标
            neutralGO.transform.SetParent(anchorRoot, worldPositionStays: false);
            // localPosition 归零
            neutralGO.transform.localPosition = Vector3.zero;
            // localRotation 设置为 anchorRoot 旋转的逆，从而使 neutralGO 的 worldRotation = identity
            neutralGO.transform.localRotation =
                Quaternion.Inverse(anchorRoot.rotation);
            return neutralGO.transform;
        }


        private IEnumerator ResolveAnchor(string cloudId, ResolveCloudAnchorPromise promise)
        {
            yield return promise;
            var result = promise.Result;
            
            if (result.CloudAnchorState == CloudAnchorState.Success)
            {
                OnAnchorResolvedFinished(true, cloudId);
                
                var anchor = result.Anchor;
                _resolvedAnchorTransform = anchor.transform;

                // *** 關鍵：設置這個 anchor 為全域參考點 ***
                // MyNetworkedObject.SetCloudAnchorReference(anchor.transform);


                Debug.Log($"[Hierarchy] Resolved Anchor at: {GetFullHierarchyPath(anchor.transform)}");
                Debug.Log($"[Resolve] Resolved Anchor WorldPos = {anchor.transform.position}");
                Debug.Log($"[Resolve] Resolved Anchor Rotation (Euler) = {anchor.transform.rotation.eulerAngles}");

                var go = Instantiate(CloudAnchorPrefab);
                go.transform.SetParent(anchor.transform, worldPositionStays: false); // 不保留世界座標
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;



                // 定位到可能的 CesiumGeoreference
                Transform anchorRoot = go.transform;
                var geo = anchorRoot.Find("CesiumGeoreference");
                if (geo != null)
                {
                    anchorRoot = geo;
                }

                // 插入 neutral 容器
                var neutral = CreateNeutralSpawnRoot(anchorRoot);

                // *** 關鍵：設置 anchor 和 neutral 為參考點 ***
                MyNetworkedObject.SetCloudAnchorReference(anchor.transform, neutral);

                // 最后设置 spawnParent
                Spawner.spawnParent = neutral;

                // 然后再启动允许 spawn
                Spawner.SetReadyToSpawn(true);
                                
                Debug.Log($"[Resolve] Instantiated CloudAnchorPrefab worldPos = {go.transform.position}");
                Debug.Log($"[Resolve] CloudAnchorPrefab worldRot = {go.transform.rotation.eulerAngles}");
            }
            else
            {
                OnAnchorResolvedFinished(false, cloudId, result.CloudAnchorState.ToString());
            }
        }

        private void OnAnchorHostedFinished(bool success, string response = null)
        {
            if (success)
            {
                // 找到 neutral transform
                Transform neutral = null;
                var spawnRoot = _anchor.GetComponentInChildren<Transform>();
                foreach (Transform child in _anchor.GetComponentsInChildren<Transform>())
                {
                    if (child.name == "SpawnRoot")
                    {
                        neutral = child;
                        break;
                    }
                }
                MyNetworkedObject.SetCloudAnchorReference(_anchor.transform, neutral);

                OnAnchorHosted?.Invoke();
                InstructionText.text = "Finish!";
                Invoke("DoHideInstructionBar", 1.5f);
                DebugText.text =
                    string.Format("Succeed to host the Cloud Anchor: {0}.", response);

                // Display name panel and hide instruction bar.
                NameField.text = _hostedCloudAnchor.Name;
                NamePanel.SetActive(true);
                SetSaveButtonActive(true);
            }
            else
            {
                InstructionText.text = "Host failed.";
                DebugText.text = "Failed to host a Cloud Anchor" + (response == null ? "." :
                    "with error " + response + ".");
            }
        }

        private void OnAnchorResolvedFinished(bool success, string cloudId, string response = null)
        {
            if (success)
            {
                OnAnchorResolved?.Invoke();
                InstructionText.text = "Resolve success!";
                DebugText.text =
                    string.Format("Succeed to resolve the Cloud Anchor: {0}.", cloudId);
            }
            else
            {
                InstructionText.text = "Resolve failed.";
                DebugText.text = "Failed to resolve Cloud Anchor: " + cloudId +
                    (response == null ? "." : "with error " + response + ".");
            }
        }

        private void UpdateInitialInstruction()
        {
            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                    // Initial instruction for hosting flow:
                    InstructionText.text = "Tap to place an object.";
                    DebugText.text = "Tap a vertical or horizontal plane...";
                    return;
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    // Initial instruction for resolving flow:
                    InstructionText.text =
                        "Look at the location you expect to see the AR experience appear.";
                    DebugText.text = string.Format("Attempting to resolve {0} anchors...",
                        Controller.ResolvingSet.Count);
                    return;
                default:
                    return;
            }
        }

        private void UpdatePlaneVisibility(bool visible)
        {
            foreach (var plane in Controller.PlaneManager.trackables)
            {
                plane.gameObject.SetActive(visible);
            }
        }

        private void ARCoreLifecycleUpdate()
        {
            // Only allow the screen to sleep when not tracking.
            var sleepTimeout = SleepTimeout.NeverSleep;
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }

            Screen.sleepTimeout = sleepTimeout;

            if (_isReturning)
            {
                return;
            }

            // Return to home page if ARSession is in error status.
            if (ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                ReturnToHomePage(string.Format(
                    "ARCore encountered an error state {0}. Please start the app again.",
                    ARSession.state));
            }
        }

        private void DisplayTrackingHelperMessage()
        {
            if (_isReturning || ARSession.notTrackingReason == NotTrackingReason.None)
            {
                TrackingHelperText.gameObject.SetActive(false);
            }
            else
            {
                TrackingHelperText.gameObject.SetActive(true);
                switch (ARSession.notTrackingReason)
                {
                    case NotTrackingReason.Initializing:
                        TrackingHelperText.text = _initializingMessage;
                        return;
                    case NotTrackingReason.Relocalizing:
                        TrackingHelperText.text = _relocalizingMessage;
                        return;
                    case NotTrackingReason.InsufficientLight:
                        if (_versionInfo.GetStatic<int>("SDK_INT") < _androidSSDKVesion)
                        {
                            TrackingHelperText.text = _insufficientLightMessage;
                        }
                        else
                        {
                            TrackingHelperText.text = _insufficientLightMessageAndroidS;
                        }

                        return;
                    case NotTrackingReason.InsufficientFeatures:
                        TrackingHelperText.text = _insufficientFeatureMessage;
                        return;
                    case NotTrackingReason.ExcessiveMotion:
                        TrackingHelperText.text = _excessiveMotionMessage;
                        return;
                    case NotTrackingReason.Unsupported:
                        TrackingHelperText.text = _unsupportedMessage;
                        return;
                    default:
                        TrackingHelperText.text =
                            string.Format("Not tracking reason: {0}", ARSession.notTrackingReason);
                        return;
                }
            }
        }

        private void ReturnToHomePage(string reason)
        {
            Debug.Log("Returning home for reason: " + reason);
            if (_isReturning)
            {
                return;
            }

            _isReturning = true;
            DebugText.text = reason;
            Invoke("DoReturnToHomePage", 3.0f);
        }

        private void DoReturnToHomePage()
        {
            Controller.SwitchToHomePage();
        }

        private void DoHideInstructionBar()
        {
            InstructionBar.SetActive(false);
        }

        private void SetSaveButtonActive(bool active)
        {
            SaveButton.enabled = active;
            SaveButton.GetComponentInChildren<Text>().color = active ? _activeColor : Color.gray;
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
    }
}