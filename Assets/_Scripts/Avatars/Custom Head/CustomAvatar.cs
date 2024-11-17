using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Metaverse/Custom Avatar")]
public class CustomAvatar : ScriptableObject
{
	[SerializeField] private int _avatarId;

	[Header("Head")]
	[SerializeField] private bool _disableOtherHead = true;
	[SerializeField] private GameObject _customHeadPrefab;
	[SerializeField] private Vector3 _positionOffset;
	[SerializeField] private Vector3 _rotationOffset;
	
	[Header("Materials")]
	[SerializeField] private bool _customMaterials;
	[SerializeField, ShowIf("_customMaterials")] private Texture _base;
	[SerializeField, ShowIf("_customMaterials")] private Texture _mask;
	
	[Header("Other")]
	[SerializeField] private bool _lipSyncEnabled;
	
	public int AvatarId => _avatarId;
	public bool DisableOtherHead => _disableOtherHead;
	public bool CustomMaterials => _customMaterials;
	public bool LipSyncEnabled => _lipSyncEnabled;
    
	public static readonly int MainTex = Shader.PropertyToID("_MainTex");
	public static readonly int PropertiesMap = Shader.PropertyToID("_PropertiesMap");
	
	public GameObject CreateAndPlaceCustomHead(Transform headJoint)
	{
		GameObject customHead = Instantiate(_customHeadPrefab);
		var head = customHead.transform;
		head.SetParent(headJoint);
		head.localPosition = _positionOffset;
		head.localRotation = Quaternion.Euler(_rotationOffset);
		customHead.SetActive(true);
		return customHead;
	}

	public void AssignCustomMaterialTextures(MeshRenderer renderer)
	{
		if (!_customMaterials) return;
		var mat = renderer.material;
		if (_base) mat.SetTexture(MainTex, _base);
		if (_mask) mat.SetTexture(PropertiesMap, _mask);
	}
}
