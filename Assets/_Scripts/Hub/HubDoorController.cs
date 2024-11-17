using System;
using System.Collections.Generic;
using MILab;
using UnityEngine;

public class HubDoorController : MonoBehaviour
{
    public static Action<RoomHubCheck> OnLoadRoomDoor = delegate { };
    public static bool HubActive = false;
    
    [SerializeField] private List<HubDoor> _doors;
    [SerializeField] private Transform _playerRef;

    private void OnEnable()
    {
        //OnLoadRoomDoor += OnOnLoadRoomDoor;
    }

    private void OnDisable()
    {
        //OnLoadRoomDoor -= OnOnLoadRoomDoor;
    }

    private void Start()
    {
        HubActive = true;
    }

    private void DisableAllDoors()
    {
        
    }

    [Button]
    private void OnOnLoadRoomDoor(RoomHubCheck roomHubCheck)
    {
        if (!roomHubCheck.Valid) return;
        foreach (var hubDoor in _doors)
        {
            if (hubDoor.Valid && roomHubCheck.SceneName.Equals(hubDoor.SceneName))
            {
                // Does not work because the rooms are STATIC!
                //CaliManager.CalibrateTransforms(roomDoor.RoomParent, roomDoor.transform, hubDoor.transform);

                if (PlayerOVR.Instance)
                {
                    var player = PlayerOVR.Instance.transform;
                    _playerRef.position = player.position;
                    _playerRef.rotation = player.rotation;
                }
                
                CaliManager.CalibrateTransforms(transform, hubDoor.transform, roomHubCheck.transform);
                
                if (PlayerOVR.Instance)
                {
                    var player = PlayerOVR.Instance.transform;
                    player.position = _playerRef.position;
                    player.rotation = _playerRef.rotation;
                }
                return;
            }
        }
    }
}
