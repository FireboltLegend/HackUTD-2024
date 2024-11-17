using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SingleMirrorRenderer: MonoBehaviour {
	[Header("Camera")]
	[SerializeField] public Camera _sourceCamOverride;
	[SerializeField] private Camera _renderCam;
	[SerializeField] private LayerMask _cameraViewMask = 1;
	[SerializeField] private int _renderTargetWidth = 1920;
	[SerializeField] private int _renderTargetHeight = 1080;
	[SerializeField] private float _cameraFov = 90.0f;

	[Header("Shader parameters")]
	[SerializeField] private string _eyeTexParam = "EyeTexL";
	[SerializeField] private string _eyeViewMatParam = "EyeViewMatrixL";
	[SerializeField] private string _eyeProjMatParam = "EyeProjMatrixL";
	[SerializeField] private Material _targetMaterial;

	[Header("Internals (do not touch)")]
	[SerializeField, ReadOnly] private Pose _worldEyePose;

	[SerializeField, ReadOnly] private GameObject _eyeDebugObj;

	[SerializeField, ReadOnly] private RenderTexture _renderTex;

	[SerializeField, ReadOnly] private Matrix4x4 _eyeProj = Matrix4x4.identity;
	[SerializeField, ReadOnly] private Matrix4x4 _eyeView = Matrix4x4.identity;

	private Camera SrcCamera => _sourceCamOverride ? _sourceCamOverride: Camera.main;

	private void OnEnable() {
		_renderTex = new RenderTexture(_renderTargetWidth, _renderTargetHeight, 16);
		_renderTex.Create();

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

		_eyeDebugObj = new GameObject("DebugEye")
		{
			hideFlags = HideFlags.DontSave
		};
		_eyeDebugObj.transform.SetParent(transform);
	}

	private void OnDisable(){
		if (_renderTex){
			_renderTex.Release();
			_renderTex = null;
		}
	}

	private bool _renderedLastFrame;

	private void LateUpdate(){
		if (_renderedLastFrame) {
			_renderedLastFrame = false;
			return;
		}
		UpdateEyePos();
		RenderToTexture(_renderTex, _worldEyePose, out _eyeView, out _eyeProj);
		SetShaderParams();
		_renderedLastFrame = true;
	}

	private void UpdateEyePos(){
		var cam = SrcCamera;
		if (cam == null) return;
		_worldEyePose.position = cam.transform.position;
		_worldEyePose.rotation = cam.transform.rotation;
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
			_targetMaterial.SetMatrix(_eyeProjMatParam, _eyeProj);
			_targetMaterial.SetMatrix(_eyeViewMatParam, _eyeView);
			_targetMaterial.SetTexture(_eyeTexParam, _renderTex);
		}
		Shader.SetGlobalMatrix(_eyeProjMatParam, _eyeProj);
		Shader.SetGlobalMatrix(_eyeViewMatParam, _eyeView);
		Shader.SetGlobalTexture(_eyeTexParam, _renderTex);
	}
}