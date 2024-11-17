using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace MILab
{
    public class ImageViewTransformSynchronizer : MonoBehaviourPun
    {
        //[SerializeField] Renderer imagePreviewRenderer;
        //Texture2D imgPreviewTexture;
        [SerializeField] ImagePreview imagePreviewQuad;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {
                photonView.RPC("SyncObjectTransform", RpcTarget.All, transform.position, transform.rotation, transform.localScale);
            }
        }

        public void LoadImageFromURL()
        {
            photonView.RPC("RPC_LoadImageFromURL", RpcTarget.All);
        }

        [PunRPC]
        void SyncObjectTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // Update the object's transform for all players
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
            //if (imagePreviewRenderer != null)
            //{
            //    imagePreviewRenderer.material.mainTexture = imgPreviewTexture;
            //    imagePreviewRenderer.material.SetColor("_BaseColor", Color.white);
            //    imagePreviewRenderer.material.SetColor("_EmissionColor", Color.white);
            //}
        }
        
        [PunRPC]
        void RPC_LoadImageFromURL()
        {
            imagePreviewQuad.LoadImagePreview();
        }
    }
}