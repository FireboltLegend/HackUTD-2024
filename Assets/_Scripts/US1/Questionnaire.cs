using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Questionnaire : MonoBehaviour
{
    [SerializeField] PhotonView photonView;
    [SerializeField] GameObject[] questionnaires;
    
    public void ToggleVisibility(bool val)
    {
        photonView.RPC(nameof(RPC_ToggleVisibility), RpcTarget.AllBufferedViaServer, val);
    }

    [PunRPC]
    void RPC_ToggleVisibility(bool val)
    {
        questionnaires[0].SetActive(val);
        questionnaires[1].SetActive(val);
    }
}
