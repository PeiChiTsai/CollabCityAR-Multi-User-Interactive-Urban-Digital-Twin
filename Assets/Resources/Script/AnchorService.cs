using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;

/// <summary>
/// AnchorService handles hosting and resolving Google Cloud Anchors using ARCore Extensions.
/// Attach this script to the same GameObject that has ARAnchorManager and ARCoreExtensions components.
/// </summary>
public class AnchorService : MonoBehaviour
{
    [Header("AR Components (Auto-Assign)")]
    [Tooltip("Reference to the AR Anchor Manager (ARCore Extensions)")]
    public ARAnchorManager arAnchorManager;
    [Tooltip("Reference to the ARCore Extensions component")]
    public ARCoreExtensions arCoreExtensions;

    private void Reset()
    {
        // Automatically assign required components when the script is added or Reset is invoked
        arAnchorManager = GetComponent<ARAnchorManager>();
        arCoreExtensions = GetComponent<ARCoreExtensions>();
    }

    /// <summary>
    /// Hosts a local ARAnchor to the Google Cloud and returns the Cloud Anchor ID.
    /// </summary>
    /// <param name="localAnchor">The locally created ARAnchor to host</param>
    /// <param name="ttlDays">Time-to-live in days (1 to 365)</param>
    /// <returns>Cloud Anchor ID string</returns>
    public async Task<string> HostCloudAnchorAsync(ARAnchor localAnchor, int ttlDays = 1)
    {
        if (localAnchor == null)
            throw new ArgumentNullException(nameof(localAnchor));

        // Initiate hosting request
        ARCloudAnchor cloudAnchor = arAnchorManager.HostCloudAnchor(localAnchor, ttlDays);

        // Wait until the hosting process is complete
        while (cloudAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
        {
            await Task.Yield();
        }

        // Check the result state
        if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            Debug.Log($"[AnchorService] Hosted successfully! ID={cloudAnchor.cloudAnchorId}");
            return cloudAnchor.cloudAnchorId;
        }
        else
        {
            string msg = $"[AnchorService] Host failed: {cloudAnchor.cloudAnchorState}";
            Debug.LogError(msg);
            throw new Exception(msg);
        }
    }

    /// <summary>
    /// Resolves a Cloud Anchor ID to an ARCloudAnchor instance.
    /// </summary>
    /// <param name="anchorId">The Cloud Anchor ID to resolve</param>
    /// <returns>Resolved ARCloudAnchor instance</returns>
    public async Task<ARCloudAnchor> ResolveCloudAnchorAsync(string anchorId)
    {
        if (string.IsNullOrEmpty(anchorId))
            throw new ArgumentException("anchorId cannot be null or empty", nameof(anchorId));

        // Initiate resolving request
        ARCloudAnchor cloudAnchor = arAnchorManager.ResolveCloudAnchorId(anchorId);

        // Wait until the resolving process is complete
        while (cloudAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
        {
            await Task.Yield();
        }

        // Check the result state
        if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            Debug.Log($"[AnchorService] Resolved successfully! Pose={cloudAnchor.transform.position}");
            return cloudAnchor;
        }
        else
        {
            string msg = $"[AnchorService] Resolve failed: {cloudAnchor.cloudAnchorState}";
            Debug.LogError(msg);
            throw new Exception(msg);
        }
    }
}
