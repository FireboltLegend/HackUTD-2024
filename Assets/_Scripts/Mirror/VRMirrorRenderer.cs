using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

public class VRMirrorRenderer: MonoBehaviour {
	[Header("Camera")]
	[SerializeField] private Camera _sourceCamOverride;
	[SerializeField] private Camera _renderCam;
	[SerializeField] private LayerMask _cameraViewMask = 1;
	[SerializeField] private int _renderTargetWidth = 1920;
	[SerializeField] private int _renderTargetHeight = 1080;
	[SerializeField] private float _cameraFov = 90.0f;

	[Header("Shader parameters")]
	[SerializeField] private string _eyeTexLParam = "EyeTexL";
	[SerializeField] private string _eyeTexRParam = "EyeTexR";
	[SerializeField] private string _eyeViewMatLParam = "EyeViewMatrixL";
	[SerializeField] private string _eyeViewMatRParam = "EyeViewMatrixR";
	[SerializeField] private string _eyeProjMatLParam = "EyeProjMatrixL";
	[SerializeField] private string _eyeProjMatRParam = "EyeProjMatrixR";
	[SerializeField] private Material _targetMaterial;

	[Header("Internals (do not touch)")]
	[SerializeField, ReadOnly] private Pose _deviceEyePoseL;
	[SerializeField, ReadOnly] private Pose _deviceEyePoseR;
	[SerializeField, ReadOnly] private Pose _worldEyePoseL;
	[SerializeField, ReadOnly] private Pose _worldEyePoseR;

	[SerializeField, ReadOnly] private GameObject _eyeDebugObjL;
	[SerializeField, ReadOnly] private GameObject _eyeDebugObjR;

	[SerializeField, ReadOnly] private RenderTexture _renderTexL;
	[SerializeField, ReadOnly] private RenderTexture _renderTexR;

	[SerializeField, ReadOnly] private Matrix4x4 _eyeProjL = Matrix4x4.identity;
	[SerializeField, ReadOnly] private Matrix4x4 _eyeProjR = Matrix4x4.identity;
	[SerializeField, ReadOnly] private Matrix4x4 _eyeViewL = Matrix4x4.identity;
	[SerializeField, ReadOnly] private Matrix4x4 _eyeViewR = Matrix4x4.identity;

	private Camera SrcCamera => _sourceCamOverride ? _sourceCamOverride: Camera.main;
	
	private List<XRNodeState> nodeStatesCache = new List<XRNodeState>();

	private void OnEnable() {
		_renderTexL = new RenderTexture(_renderTargetWidth, _renderTargetHeight, 16);
		_renderTexR = new RenderTexture(_renderTargetWidth, _renderTargetHeight, 16);
		_renderTexL.Create();
		_renderTexR.Create();

		if (_renderCam == null) {
			var renderCamObj = new GameObject("Render Camera")
			{
				hideFlags = HideFlags.DontSave
			};
			renderCamObj.transform.SetParent(transform);
			_renderCam = renderCamObj.AddComponent<Camera>();
			_renderCam.hideFlags = HideFlags.DontSave;
			var urpData = _renderCam.GetUniversalAdditionalCameraData();
			urpData.renderPostProcessing = true;
			urpData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
		}
		_renderCam.enabled = false;

		_eyeDebugObjL = new GameObject("DebugEyeL")
		{
			hideFlags = HideFlags.DontSave
		};
		_eyeDebugObjL.transform.SetParent(transform);
		_eyeDebugObjR = new GameObject("DebugEyeR")
		{
			hideFlags = HideFlags.DontSave
		};
		_eyeDebugObjR.transform.SetParent(transform);
	}

	private void OnDisable(){
		if (_renderTexL){
			_renderTexL.Release();
			_renderTexL = null;
		}
		if (_renderTexR){
			_renderTexR.Release();
			_renderTexR = null;
		}
	}

	private bool _renderedLastFrame;

	private void LateUpdate(){
		if (_renderedLastFrame) {
			_renderedLastFrame = false;
			return;
		}
		UpdateEyePos();
		RenderToTexture(_renderTexL, _worldEyePoseL, out _eyeViewL, out _eyeProjL);
		RenderToTexture(_renderTexR, _worldEyePoseR, out _eyeViewR, out _eyeProjR);
		SetShaderParams();
		_renderedLastFrame = true;
	}
	
	private bool TryGetNodeStateRotation(XRNode xrNode, out Vector3 position, out Quaternion rotation)
	{
		InputTracking.GetNodeStates(nodeStatesCache);
		foreach (var nodeState in nodeStatesCache.Where(nodeState => nodeState.nodeType == xrNode))
		{
			if (nodeState.TryGetPosition(out position) && nodeState.TryGetRotation(out rotation))
				return true;
		}
		// This is the fail case, where there was no center eye was available.
		position = Vector3.zero;
		rotation = Quaternion.identity;
		return false;
	}

	private void UpdateEyePos(){
		if (TryGetNodeStateRotation(XRNode.LeftEye, out var posL, out var rotL))
		{
			_deviceEyePoseL.position = posL;
			_deviceEyePoseL.rotation = rotL;
		}
		if (TryGetNodeStateRotation(XRNode.RightEye, out var posR, out var rotR))
		{
			_deviceEyePoseR.position = posR;
			_deviceEyePoseR.rotation = rotR;
		}
		//Debug.Log($"{deviceEyePoseL} {deviceEyePoseR}");
		var cam = SrcCamera;
		if (cam == null) return;
		var camParent = cam.transform.parent;
		if (!camParent)
		{
			_worldEyePoseL = _deviceEyePoseL;
			_worldEyePoseR = _deviceEyePoseR;
		}
		else
		{
			_worldEyePoseL.position = camParent.TransformPoint(_deviceEyePoseL.position);
			_worldEyePoseL.rotation = camParent.rotation * _deviceEyePoseL.rotation;//deviceEyePoseL.rotation * camParent.rotation;
			_worldEyePoseR.position = camParent.TransformPoint(_deviceEyePoseR.position);
			_worldEyePoseR.rotation = camParent.rotation * _deviceEyePoseR.rotation;//deviceEyePoseR.rotation * camParent.rotation;
		}
		_eyeDebugObjL.transform.position = _worldEyePoseL.position;
		_eyeDebugObjL.transform.rotation = _worldEyePoseL.rotation;
		_eyeDebugObjR.transform.position = _worldEyePoseR.position;
		_eyeDebugObjR.transform.rotation = _worldEyePoseR.rotation;
	}

	private void RenderToTexture(RenderTexture rt, Pose eyePose, out Matrix4x4 viewMat, out Matrix4x4 projMat)
	{
		viewMat = Matrix4x4.identity;
		projMat = Matrix4x4.identity;

		var srcCam = SrcCamera;
		if (srcCam == null) return;

		_renderCam.enabled = true;
		_renderCam.transform.position = eyePose.position;
		_renderCam.transform.rotation = eyePose.rotation;

		//_renderCam.nearClipPlane = srcCam.nearClipPlane;
		//_renderCam.farClipPlane = srcCam.farClipPlane;
		_renderCam.fieldOfView = _cameraFov;
		_renderCam.cullingMask = _cameraViewMask;

		viewMat = _renderCam.worldToCameraMatrix;
		var srcCoord = new Coord(transform);
		var dstCoord = srcCoord;
		
		var localEyePos = srcCoord.worldToLocalPos(eyePose.position);
		var localEyeUp = srcCoord.worldToLocalDir(eyePose.rotation * Vector3.up);
		var localEyeForward = srcCoord.worldToLocalDir(eyePose.rotation * Vector3.forward);

		var planeNormal = Vector3.up;
		localEyePos = Vector3.Reflect(localEyePos, planeNormal);
		localEyeUp = Vector3.Reflect(localEyeUp, planeNormal);
		localEyeForward = Vector3.Reflect(localEyeForward, planeNormal);

		var newEyePos = dstCoord.localToWorldPos(localEyePos);
		var newEyeUp = dstCoord.localToWorldDir(localEyeUp);
		var newEyeForward = dstCoord.localToWorldDir(localEyeForward);
		var newEyeRot = Quaternion.LookRotation(newEyeForward, newEyeUp);

		eyePose.position = newEyePos;
		eyePose.rotation = newEyeRot;
		_renderCam.transform.position = eyePose.position;
		_renderCam.transform.rotation = eyePose.rotation;

		projMat = _renderCam.projectionMatrix;
		projMat *= Matrix4x4.Scale(new Vector3(-1.0f, 1.0f, 1.0f));

		_renderCam.targetTexture = rt;

		_renderCam.Render();

		_renderCam.enabled = false;
	}

	private void SetShaderParams(){
		if (_targetMaterial){
			_targetMaterial.SetMatrix(_eyeProjMatLParam, _eyeProjL);
			_targetMaterial.SetMatrix(_eyeProjMatRParam, _eyeProjR);
			_targetMaterial.SetMatrix(_eyeViewMatLParam, _eyeViewL);
			_targetMaterial.SetMatrix(_eyeViewMatRParam, _eyeViewR);
			_targetMaterial.SetTexture(_eyeTexLParam, _renderTexL);
			_targetMaterial.SetTexture(_eyeTexRParam, _renderTexR);
		}
		Shader.SetGlobalMatrix(_eyeProjMatLParam, _eyeProjL);
		Shader.SetGlobalMatrix(_eyeProjMatRParam, _eyeProjR);
		Shader.SetGlobalMatrix(_eyeViewMatLParam, _eyeViewL);
		Shader.SetGlobalMatrix(_eyeViewMatRParam, _eyeViewR);
		Shader.SetGlobalTexture(_eyeTexLParam, _renderTexL);
		Shader.SetGlobalTexture(_eyeTexRParam, _renderTexR);
	}
}