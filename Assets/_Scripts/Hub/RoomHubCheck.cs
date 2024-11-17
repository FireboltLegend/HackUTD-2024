using System.Collections.Generic;
using UnityEngine;

public class RoomHubCheck : MonoBehaviour
{
    [SerializeField] private Transform _roomParent;
    [SerializeField] private List<GameObject> _objsToDisable;
    [SerializeField] private DigitalTwinScene _scene;

    public bool Valid => _scene.Valid;
    public Transform RoomParent => _roomParent;
    public string SceneName => _scene.SceneName;
    
    private void Start()
    {
        HubDoorController.OnLoadRoomDoor?.Invoke(this);
        if (HubDoorController.HubActive)
        {
            foreach (var obj in _objsToDisable)
            {
                obj.SetActive(false);
            }
        }
    }
}
