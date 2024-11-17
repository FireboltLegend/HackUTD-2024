using System.Collections;
using MILab;
using UnityEngine;

public class LocalCustomAvatar : MonoBehaviour
{
    public static LocalCustomAvatar Instance;
    
	[Header("Custom Head")]
	[SerializeField] private CustomAvatar _temoc;
	[SerializeField] private CustomAvatar _professor;

    [Header("LipSync")]
    [SerializeField] private VoiceHandler _handler;
	[SerializeField, ReadOnly] private MouthMovement _mouthMovement;
    
	[Header("Debug")]
	[SerializeField, ReadOnly] private CustomAvatar _avatar;
	[SerializeField, ReadOnly] private GameObject _customHead;
	[SerializeField, ReadOnly] private Transform _headJoint;
	[SerializeField, ReadOnly] private MeshRenderer _meshRenderer;
    
    public static int TemocAvatarNumber => Instance._temoc.AvatarId;
    public static int ProfessorAvatarNumber => Instance._professor.AvatarId;
    public static readonly int PropertiesMap = Shader.PropertyToID("_PropertiesMap");
    public static readonly int MainTex = Shader.PropertyToID("_MainTex");

    #region Properties
    
	public CustomAvatar TemocAvatar => _temoc;
	public CustomAvatar ProfessorAvatar => _professor;
	public static CustomAvatar Avatar => Instance._avatar;
    
    private Transform HeadJoint
    {
        get
        {
            if (_headJoint) return _headJoint;
            var parent = PlayerOVR.LocalAvatarEntity.transform;
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
            var parent = PlayerOVR.LocalAvatarEntity.transform;
            if (!parent) return null;
            
            var obj = parent.Find("LOD0");
            if (!obj) return null;
            
            var mesh = obj.GetChild(0);
            if (!mesh) return null;
            
            _meshRenderer = mesh.GetComponent<MeshRenderer>();
            return _meshRenderer;
        }
    }

    private MouthMovement Mouth
    {
        get
        {
            if (_mouthMovement || !_customHead) return _mouthMovement;
            _mouthMovement = _customHead.GetComponentInChildren<MouthMovement>();
            return _mouthMovement;
        }
    }

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
	    if (_avatar && _avatar.LipSyncEnabled && _customHead) LipSync();
    }

    // TODO: Lip Sync currently does not work for some reason
    private void LipSync()
    {
        var mouth = Mouth;
        if (mouth)
        {
            mouth.SetTalking(_handler.Volume > 0.25f);
        }
    }

	public static void LoadCustomAvatar(int avatarId)
    {
        if (!Instance)
        {
            Debug.LogError("Cannot Find Local Custom Avatar!");
            return;
        }
        bool temoc = avatarId == TemocAvatarNumber;
	    Instance._avatar = temoc ? Instance._temoc : Instance._professor;
        Instance.StartCoroutine(Instance.LoadCustomAvatarRoutine());
    }

    private IEnumerator LoadCustomAvatarRoutine()
    {
        yield return null;
        while (!CheckLoadCustomHeadActive())
        {
            yield return null;
        }
    }
    
    private bool CheckLoadCustomHeadActive()
	{
		if (_avatar.DisableOtherHead)
		{
			PlayerOVR.LocalAvatarEntity.DisableHead(_avatar.DisableOtherHead);
		}
		if (HeadJoint && !_customHead)
        {
        	if (_customHead) Destroy(_customHead);
	        _customHead = _avatar.CreateAndPlaceCustomHead(_headJoint);
        }
	    if (_avatar.CustomMaterials && MeshRenderer)
        {
		    _avatar.AssignCustomMaterialTextures(_meshRenderer);
        }
        return _customHead && (!_avatar.CustomMaterials || _meshRenderer);
    }
}
