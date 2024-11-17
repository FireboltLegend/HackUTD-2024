using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Photon.Pun;

namespace MILab
{
    public class ChangeSlide : MonoBehaviour, IPunObservable
    {
        public Material[] slides;

        private int slideIndex;
        private int slideIndexPrev;


        private PhotonView PV;
        private MeshRenderer meshRenderer;


        // Start is called before the first frame update
        void Start()
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            PV = gameObject.GetComponent<PhotonView>();
            SetSlide(0);
        }


        void Update()
        {

            if (Input.GetKeyDown(KeyCode.RightArrow))// || OVRInput.GetDown(OVRInput.Button.One))
            {
                ForwardSlide();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))// || OVRInput.GetDown(OVRInput.Button.Three))
            {
                BackwardSlide();
            }

        }

        void SetSlide(int index)
        {
            meshRenderer.material = slides[index];
        }


        [PunRPC]
        void ChangeSlideRPC(int index)
        {
            Debug.Log(string.Format("ChangeSlideRPC {0}", index));
            SetSlide(index);
        }

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(slideIndex);
                stream.SendNext(slideIndexPrev);

            }
            else
            {
                slideIndex = (int)stream.ReceiveNext();
                slideIndexPrev = (int)stream.ReceiveNext();

            }

        }

        public void ForwardSlide()
        {
            slideIndex++;

            if (slideIndex > slides.Length - 1)
            {
                slideIndex = slides.Length - 1;
            }

            if (PhotonNetwork.IsConnected)
            {
                PV.RPC("ChangeSlideRPC", RpcTarget.AllBuffered, slideIndex);
            }
            else
            {
                SetSlide(slideIndex);
            }
        }

        public void BackwardSlide()
        {
            slideIndex--;

            if (slideIndex < 0)
            {
                slideIndex = 0;
            }

            if (PhotonNetwork.IsConnected)
            {
                PV.RPC("ChangeSlideRPC", RpcTarget.AllBuffered, slideIndex);
            }
            else
            {
                SetSlide(slideIndex);
            }
        }
    }
}
