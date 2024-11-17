using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;

public class PlaceableObjectController : MonoBehaviourPun, IPunOwnershipCallbacks
{
    [SerializeField]
    [Tooltip("Should the object's manipulation bounds be created based on its renderers?")]
    private bool automaticBounds = false;

    BoxCollider boxCollider;
    BoundsControl boundsControl;
    Rigidbody rb;
    private bool connectedToHandMenu = false;
    private bool manipulating = false;
    private bool requesting = false;

    Microsoft.MixedReality.Toolkit.UI.ObjectManipulator objManip;

    void Awake()
    {
        ObjectEditingEvents editEvents = (ObjectEditingEvents)FindObjectOfType(typeof(ObjectEditingEvents), true);
        if (editEvents != null)
        {
            editEvents.placables.Add(this);
        }


    }

    void OnEnable()
    {
        boxCollider = transform.Find("Bounds").GetComponent<BoxCollider>();
        boundsControl = GetComponent<BoundsControl>();
        rb = GetComponent<Rigidbody>();
        objManip = GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
        boundsControl.enabled = false;
        objManip.enabled = false;
        PhotonNetwork.AddCallbackTarget(this);
        if (automaticBounds && boxCollider != null)
        {
            CalculateBounds();
        }
        ConnectToHandMenu();
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void Update()
    {
        //Cannot edit without owenership
        if (!base.photonView.IsMine)
        {
            //Debug.Log("ForceEnd");
            objManip.ForceEndManipulation();
        }
    }

  
    
    void ConnectToHandMenu()
    {
     
        ObjectEditingEvents editEvents = (ObjectEditingEvents)FindObjectOfType(typeof(ObjectEditingEvents), true);
        if (editEvents != null)
        {
            editEvents.OnHandMenuChange.AddListener(HandMenuChangeHandler);
            Debug.Log(editEvents);
            connectedToHandMenu = true;
        }
        else
        {
            Debug.LogError("Couldn't find hand menu controller");
            Invoke("ConnectToHandMenu", 2f);
        }
    }

    public void HandMenuChangeHandler(string activeMenu)
    {
        if(activeMenu == "Object Editor")
        {
            if (boundsControl)
            {
                boundsControl.enabled = true;
            }

            
            
            objManip.enabled = true;
        } else
        {
            if (boundsControl)
            {
                boundsControl.enabled = false;
            }

            

            objManip.enabled = false;
        }
    }

    static Bounds GetLocalBoundsForObject(GameObject go)
    {
        var referenceTransform = go.transform;
        var b = new Bounds(Vector3.zero, Vector3.zero);
        RecurseEncapsulate(referenceTransform, ref b);
        return b;

        void RecurseEncapsulate(Transform child, ref Bounds bounds)
        {
            var mesh = child.GetComponent<MeshFilter>();
            if (mesh)
            {
                var lsBounds = mesh.sharedMesh.bounds;
                var wsMin = child.TransformPoint(lsBounds.center - lsBounds.extents);
                var wsMax = child.TransformPoint(lsBounds.center + lsBounds.extents);
                bounds.Encapsulate(referenceTransform.InverseTransformPoint(wsMin));
                bounds.Encapsulate(referenceTransform.InverseTransformPoint(wsMax));
            }
            foreach (Transform grandChild in child.transform)
            {
                RecurseEncapsulate(grandChild, ref bounds);
            }
        }
    }

    [Button]
    void CalculateBounds()
    {
        var bounds = GetLocalBoundsForObject(gameObject);
        boxCollider.center = bounds.center;
        boxCollider.size = bounds.size;
    }

    public void OnManipulationStarted()
    {
        if (!photonView.IsMine)
        {
            requesting = true;
            photonView.RequestOwnership();
            
        } else
        {
            manipulating = true;

            if (rb)
            {
                rb.constraints = RigidbodyConstraints.None;
            }
        }
    }

    public void OnHoverStarted()
    {
        if (!photonView.IsMine)
        {
            photonView.RequestOwnership();
            requesting = true;
        }
    }

    public void OnManipulationEnded()
    {
        manipulating = false;
        if (rb)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        Debug.Log("Request");
        if (targetView != base.photonView)
        {
            return;
        }

        if (!manipulating)
        {
            photonView.TransferOwnership(requestingPlayer);
        }

    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if(targetView != base.photonView)
        {
            return;
        }
        requesting = false;
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        if (targetView != base.photonView)
        {
            return;
        }
        requesting = false;
    }
}
