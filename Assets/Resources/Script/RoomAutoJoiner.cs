using System;
using Ubiq.Rooms;
using UnityEngine;

public class RoomAutoJoiner : MonoBehaviour
{
    [Tooltip("Room GUID to join at start")]
    public string roomGuid = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"; // 你要寫死的 Room UUID

    void Start()
    {
        var roomClient = RoomClient.Find(this);
        if (roomClient != null && Guid.TryParse(roomGuid, out Guid guid))
        {
            roomClient.Join(guid);
            Debug.Log($"Auto-joining room with GUID: {guid}");
        }
        else
        {
            Debug.LogError("RoomClient not found or invalid GUID.");
        }
    }
}
