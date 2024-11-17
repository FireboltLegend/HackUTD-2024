using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class LagSim : MonoBehaviour
{
    PhotonPeer peer;

    void Start()
    {
        // Access PhotonPeer to set network simulation
        peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
        peer.IsSimulationEnabled = true;
        // Configure network conditions
        //peer.NetworkSimulationSettings.IncomingLag = 50s0;  // Latency in ms
        //peer.NetworkSimulationSettings.IncomingJitter = 50; // Jitter in ms
        //peer.NetworkSimulationSettings.IncomingLossPercentage = 10; // Packet loss %
        peer.NetworkSimulationSettings.OutgoingLag = 500;
        //peer.NetworkSimulationSettings.OutgoingJitter = 50;
        //peer.NetworkSimulationSettings.OutgoingLossPercentage = 10;
        Debug.Log("Network simulation enabled with custom conditions.");
    }

    [Button]
    public void SetOutgoingLag(int lag)
    {
        peer.NetworkSimulationSettings.OutgoingLag = lag;
    }
}