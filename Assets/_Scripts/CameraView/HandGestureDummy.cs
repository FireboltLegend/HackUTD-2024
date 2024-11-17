using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Photon.Pun;

namespace MILab
{
    public class HandGestureDummy : MonoBehaviour
    {
        public enum PlayerType
        {
            Local,
            Remote
        }

        [Header("Remote or Local Player")]
        [SerializeField] PlayerType playerType = PlayerType.Local;

        [Header("Camera and Image View")]
        [SerializeField] GameObject cameraView;
        [SerializeField] GameObject imagePreview;
        [SerializeField] ImagePreview imagePreviewQuad;
        Vector3 imagePreviewScale;

        [Header("Triggers and Thresholds")]
        [SerializeField] bool isTriggered = false;
        [SerializeField] float maximumHandDistanceNoZoom = 0.4f;
        [SerializeField] float maximumHandDistanceZoom = 0.6f;
        [SerializeField] float maxHandDistanceForCameraView = 0.9f;
        [SerializeField] float minZoom = 1.0f;
        [SerializeField] float maxZoom = 2.5f;
        [SerializeField] float minDistanceCameraImageViews = 0.1f;
        [SerializeField, ReadOnly] float zoomLevel = 1.0f;

        [Header("Player and Hands")]
        [SerializeField, ReadOnly] Transform centerEye;
        [SerializeField, ReadOnly] GameObject leftPalmCenter;
        [SerializeField, ReadOnly] GameObject rightPalmCenter;
        [SerializeField, ReadOnly] float distanceBetweenHands;

        //[Header("Photon View")]
        //[SerializeField] PhotonView photonView;

        void CheckPlayerPalmNullAndApplyReference()
        {
            if (centerEye == null)
                centerEye = GameObject.Find("CenterEyeAnchor").transform;
            if (leftPalmCenter == null) 
                leftPalmCenter = GameObject.Find("OVRHandPrefab_Left");
            if (rightPalmCenter == null) 
                rightPalmCenter = GameObject.Find("OVRHandPrefab_Right");
        }

        public void ShowCameraView(bool show)
        {
            isTriggered = show;
            //if (show)
            //    photonView.RPC(nameof(RPC_ShowCameraView), RpcTarget.AllBufferedViaServer);
            //else
            //    photonView.RPC(nameof(RPC_HideCameraView), RpcTarget.AllBufferedViaServer);
        }

        //public void SwitchToImagePreview()
        //{
        //    photonView.RPC(nameof(RPC_SwitchToImagePreview), RpcTarget.AllBufferedViaServer);
        //}

        //public void EnableCameraView()
        //{
        //    photonView.RPC(nameof(RPC_ShowCameraView), RpcTarget.AllBufferedViaServer);
        //}

        [Button]
        void TestCameraViewTeleport()
        {
            cameraView.transform.position = new Vector3(-0.86f, 1.162f, 1.343f);
        }

        // Start is called before the first frame update
        void Start()
        {
            CheckPlayerPalmNullAndApplyReference();
            //cameraView.SetActive(false);
            cameraView.transform.position = new Vector3(-2.132f, 1.62f, -5.263f);
            cameraView.GetComponent<CameraView>().UpdateCameraZoom(1.0f);

            //imagePreview.SetActive(false);
            imagePreview.transform.position = new Vector3(-2.132f, 2.62f, -5.263f);
            imagePreviewScale = new Vector3(0.24f, 0.18f, 0.01192952f);
            //photonView = GetComponent<PhotonView>();
        }

        // Update is called once per frame
        void Update()
        {
            CheckPlayerPalmNullAndApplyReference();

            distanceBetweenHands = Vector3.Distance(leftPalmCenter.transform.position, rightPalmCenter.transform.position);

            if (playerType == PlayerType.Local)
            {
                if (distanceBetweenHands > maxHandDistanceForCameraView)
                    isTriggered = false;

                if (isTriggered && !(imagePreview.active && Vector3.Distance(imagePreview.transform.position, (leftPalmCenter.transform.position + rightPalmCenter.transform.position) / 2.0f) < minDistanceCameraImageViews))
                {
                    //cameraView.SetActive(true);
                    //EnableCameraView();

                    cameraView.transform.position = (leftPalmCenter.transform.position + rightPalmCenter.transform.position) / 2.0f;

                    cameraView.transform.LookAt(centerEye);

                    if (distanceBetweenHands < maximumHandDistanceNoZoom)
                    {
                        zoomLevel = minZoom;
                    }

                    else if (distanceBetweenHands < maximumHandDistanceZoom)
                    {
                        zoomLevel = Mathf.Lerp(minZoom, maxZoom, Mathf.InverseLerp(maximumHandDistanceNoZoom, maximumHandDistanceZoom, distanceBetweenHands));
                    }

                    else
                    {
                        zoomLevel = maxZoom;
                    }

                    //cameraView.GetComponent<CameraView>().UpdateCameraZoom(zoomLevel);
                    cameraView.transform.localScale = Vector3.one * 0.1f * zoomLevel; //0.46967

                    if (
                        (
                         HandPoseUtils.CalculateIndexPinch(Handedness.Left) > 0.85f &&
                         HandPoseUtils.CalculateIndexPinch(Handedness.Left) < 0.98f
                        ) || (
                         HandPoseUtils.CalculateIndexPinch(Handedness.Right) > 0.85f &&
                         HandPoseUtils.CalculateIndexPinch(Handedness.Right) < 0.98f
                        )
                    )
                    {
                        Debug.LogFormat("<color=green> Capture Pinch Gesture detected! </color>");
                        cameraView.GetComponent<CameraView>().Capture();
                        //cameraView.SetActive(false);
                        cameraView.transform.position = new Vector3(-2.132f, 1.62f, -5.263f);
                        isTriggered = false;

                        //imagePreview.SetActive(true);
                        imagePreview.transform.position = (leftPalmCenter.transform.position + rightPalmCenter.transform.position) / 2.0f;
                        imagePreview.transform.localScale = imagePreviewScale * zoomLevel;
                        imagePreview.transform.LookAt(centerEye);
                        //imagePreviewQuad.LoadImagePreview();
                        imagePreview.GetComponent<ImageViewTransformSynchronizer>().LoadImageFromURL();
                        //SwitchToImagePreview();

                    }

                }
                else
                {
                    //cameraView.SetActive(false);
                    cameraView.transform.position = new Vector3(-2.132f, 1.62f, -5.263f);
                }
            }
            
            

            
        }

        //#region Pun RPCs

        //[PunRPC]
        //public void RPC_ShowCameraView()
        //{
        //    isTriggered = true;
        //}

        //public void RPC_HideCameraView()
        //{
        //    isTriggered = false;
        //}

        //[PunRPC]
        //public void RPC_EnableCameraView()
        //{
        //    cameraView.SetActive(true);
        //}

        //[PunRPC]
        //public void RPC_SwitchToImagePreview()
        //{
        //    cameraView.GetComponent<CameraView>().Capture();
        //    cameraView.SetActive(false);
        //    //isTriggered = false;

        //    imagePreview.SetActive(true);
        //    imagePreview.transform.position = (leftPalmCenter.transform.position + rightPalmCenter.transform.position) / 2.0f;
        //    imagePreview.transform.LookAt(centerEye);
        //    imagePreviewQuad.LoadImagePreview();
        //}

        //#endregion
    }
}