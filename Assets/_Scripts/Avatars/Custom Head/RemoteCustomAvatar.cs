using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class RemoteCustomAvatar : MonoBehaviour
{
    [SerializeField, ReadOnly] private Transform _localAvatarParent;
    [SerializeField, ReadOnly] private GameObject _customHead;
    [SerializeField, ReadOnly] private Transform _headJoint;
    [SerializeField, ReadOnly] private MeshRenderer _meshRenderer;
    [SerializeField, ReadOnly] private MeshRenderer _meshRenderer2;
    
    // TODO: Remote Temoc Lip Syncing!
    
    #region Properties
    
    private Transform Parent
    {
        get
        {
            if (_localAvatarParent) return _localAvatarParent;
            var parent = GameObject.Find("RemoteAvatar");
            if (parent) _localAvatarParent = parent.transform;
            return _localAvatarParent;
        }
    }
    
    private Transform HeadJoint
    {
        get
        {
            if (_headJoint) return _headJoint;
            var parent = Parent;
            if (!parent) return null;
            
            _headJoint = parent.Find("Joint Head");
            return _headJoint;
        }
    }

    private MeshRenderer MeshRenderer
    {
        get
        {
            if (_meshRenderer) return _meshRenderer;
            var parent = Parent;
            if (!parent) return null;
            
            var obj = parent.Find("LOD0");
            if (!obj) return null;
            
            var mesh = obj.GetChild(0);
            if (!mesh) return null;
            
            _meshRenderer = mesh.GetComponent<MeshRenderer>();
            return _meshRenderer;
        }
    }

    private MeshRenderer MeshRenderer2
    {
        get
        {
            if (_meshRenderer2) return _meshRenderer2;
            var parent = Parent;
            if (!parent) return null;

            var obj = parent.Find("LOD1");
            if (!obj) return null;

            var mesh = obj.GetChild(0);
            if (!mesh) return null;

            _meshRenderer2 = mesh.GetComponent<MeshRenderer>();
            return _meshRenderer2;
        }
    }

    #endregion

	public static void LoadCustomAvatar(int avatarId, int id)
	{
		var controller = LocalCustomAvatar.Instance;
        if (!controller)
        {
            Debug.LogError("Cannot Find Local Custom Avatar!");
            return;
        }
        bool temoc = avatarId == LocalCustomAvatar.TemocAvatarNumber;
		var avatar = temoc ? controller.TemocAvatar : controller.ProfessorAvatar;
		controller.StartCoroutine(LoadCustomAvatarRoutine(avatar, id));
    }

	private static IEnumerator LoadCustomAvatarRoutine(CustomAvatar avatar, int id)
    {
        yield return null;
        var remoteUsers = FindObjectsOfType<PhotonView>();
        RemoteCustomAvatar controller = null;
        foreach (var user in remoteUsers)
        {
            if (user.ViewID == id)
            {
                controller = user.GetComponent<RemoteCustomAvatar>();
                if (!controller)
                {
                    controller = user.gameObject.AddComponent<RemoteCustomAvatar>();
                }
                break;
            }
        }
        if (!controller)
        {
            Debug.LogWarning("Unable to find Remote Avatar");
            yield break;
        }
        while (!controller.CheckLoadCustomHeadActive(avatar))
        {
            yield return null;
        }
    }
    
    [Button]
	private bool CheckLoadCustomHeadActive(CustomAvatar avatar)
	{
        if (HeadJoint && !_customHead)
        {
	        _customHead = avatar.CreateAndPlaceCustomHead(_headJoint);
        }
		if (avatar.CustomMaterials)
        {
            if (MeshRenderer)
            {
	            avatar.AssignCustomMaterialTextures(_meshRenderer);
            }
            if (MeshRenderer2)
            {
	            avatar.AssignCustomMaterialTextures(_meshRenderer2);
            }
        }
		return _customHead && (!avatar.CustomMaterials || (_meshRenderer && _meshRenderer2));
    }
}
