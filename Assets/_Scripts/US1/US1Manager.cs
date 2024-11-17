using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MILab
{
    public class US1Manager : MonoBehaviour
    {
        PhotonPeer peer;
        bool callingAPI;

        [SerializeField, ReadOnly] Transform player;
        [SerializeField] Transform playerMRSpace;

        Vector3 originalPosition;
        Vector3 originalRotation;

        int prevAngle = 0;

        [Header("Silhouettes")]
        [SerializeField] GameObject[] placeables;

        [Header("Moveable Objects")]
        [SerializeField] Transform[] moveableObjects;
        Vector3[] ogMvblPos;
        Quaternion[] ogMvblRot;
        [SerializeField] Error_calculator errorCalc;

        [Header("Is Local or Remote")]
        [SerializeField] bool isLocalPlayer = true;

        [Header("Originals List")]
        [SerializeField] List<GameObject> originalsList1;
        [SerializeField] List<GameObject> originalsList2;
        [SerializeField] List<GameObject> originalsList3;
        [SerializeField] List<GameObject> originalsList4;
        [SerializeField] List<GameObject> originalsList5;
        [SerializeField] List<GameObject> originalsList6;
        [SerializeField] List<GameObject> originalsList7;
        [SerializeField] List<GameObject> originalsList8;
        [SerializeField] List<GameObject> originalsList9;

        float currentPosOffset = 0;
        float currentRotOffset = 0;
        int currentComboID = 0;
        int currentTimeLag = 0;

        // Start is called before the first frame update
        void Start()
        {
            peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
            peer.IsSimulationEnabled = true;
            //placeables = new GameObject[6];

            ogMvblPos = new Vector3[moveableObjects.Length];
            ogMvblRot = new Quaternion[moveableObjects.Length];

            for (int i = 0; i < moveableObjects.Length; i += 1)
            {
                ogMvblPos[i] = moveableObjects[i].localPosition;
                ogMvblRot[i] = moveableObjects[i].localRotation;
            }

            callingAPI = false;
        }

        // Update is called once per frame
        void Update()
        {
            if(!callingAPI)
                StartCoroutine(GetObjectData());
        }

        // Coroutine to make GET request
        [Obsolete]
        IEnumerator GetObjectData()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(String.Format("http://172.16.136.143:8080/US_1/US1")))
            {

                callingAPI = true;

                yield return request.Send();
                while (!request.isDone)
                    yield return null;
                Debug.Log(request.result);

                byte[] result = request.downloadHandler.data;
                string ObjectJSON = System.Text.Encoding.Default.GetString(result);
                US1Data info = JsonUtility.FromJson<US1Data>(ObjectJSON);

                // Update lag
                SetLag(info.Temporal_lag);

                //if (isLocalPlayer)
                //{
                // Update transform for player
                SetPlayerTransformAndResetMoveables(info.Position_offset, info.Rotation_offset);

                //}

                // Update combination
                SetCombination(info.Combination);

                callingAPI = false;

            } //end of request

        } // end of GetObjectData

        void SetLag(int lag = 0)
        {
            peer.NetworkSimulationSettings.OutgoingLag = lag;
            peer.NetworkSimulationSettings.IncomingLag = lag;

            currentTimeLag = lag;
        }

        void SetPlayerTransformAndResetMoveables(float posOffset = 0, float rotOffset = 0)
        {
            //if (player == null)
            //{
            //    player = GameObject.Find("CustomMRTK-Quest_OVRCameraRig(Clone)").transform; //CustomMRTK-Quest_OVRCameraRig(Clone) //MixedRealityPlayspace


            //    originalPosition = Vector3.zero; //player.position;
            //    originalRotation = player.eulerAngles; // Vector3.zero; //player.eulerAngles;
            //}
            if (posOffset == 3 || posOffset == 3.0f)
                posOffset = 2.5f;

            if (rotOffset == 3 || rotOffset == 3.0f)
                rotOffset = 2.5f;

            currentPosOffset = posOffset;
            currentRotOffset = rotOffset;

            if (playerMRSpace == null)
            {
                playerMRSpace = GameObject.Find("Room_02").transform;
            }

                //Vector3 ogPos = playerMRSpace.position;

                //player.eulerAngles = new Vector3(
                //    originalRotation.x,
                //    originalRotation.y + rotOffset,
                //    originalRotation.z
                //    );

                playerMRSpace.eulerAngles = new Vector3(
                0,
                180.0f - rotOffset,
                0
                );

            //player.position = ogPos;

            //if (rotOffset != prevAngle)
            //{
            //    //player.transform.Rotate(Vector3.up, prevAngle, Space.Self);
            //    //player.transform.Rotate(Vector3.up, -rotOffset, Space.Self);
            //    player.RotateAround(player.position, Vector3.up, -prevAngle);
            //    player.RotateAround(player.position, Vector3.up, rotOffset);
            //    prevAngle = rotOffset;
            //}

            //player.localPosition = new Vector3(
            //    originalPosition.x + posOffset * 0.01f,
            //    originalPosition.y,
            //    originalPosition.z + posOffset * 0.01f
            //    );

            playerMRSpace.localPosition = new Vector3(
                posOffset * 0.01f,
                0,
                posOffset * 0.01f
                );

        }

        void SetCombination(int comboID)
        {
            //if (placeables[0] == null)
            //    placeables[0] = GameObject.Find("");
            //if (placeables[1] == null)
            //    placeables[1] = GameObject.Find("");
            //if (placeables[2] == null)
            //    placeables[2] = GameObject.Find("");
            //if (placeables[3] == null)
            //    placeables[3] = GameObject.Find("");
            //if (placeables[4] == null)
            //    placeables[4] = GameObject.Find("");
            //if (placeables[5] == null)
            //    placeables[5] = GameObject.Find("");

            currentComboID = comboID;

            switch (comboID)
            {
                case 0: errorCalc.UpdateOriginalList(originalsList1); break;
                case 1: errorCalc.UpdateOriginalList(originalsList2); break;
                case 2: errorCalc.UpdateOriginalList(originalsList3); break;
                case 3: errorCalc.UpdateOriginalList(originalsList4); break;
                case 4: errorCalc.UpdateOriginalList(originalsList5); break;
                case 5: errorCalc.UpdateOriginalList(originalsList6); break;
                case 6: errorCalc.UpdateOriginalList(originalsList7); break;
                case 7: errorCalc.UpdateOriginalList(originalsList8); break;
                case 8: errorCalc.UpdateOriginalList(originalsList9); break;
            }

            if (!isLocalPlayer)
            {
                placeables[0].SetActive(comboID == 0);
                placeables[1].SetActive(comboID == 1);
                placeables[2].SetActive(comboID == 2);
                placeables[3].SetActive(comboID == 3);
                placeables[4].SetActive(comboID == 4);
                placeables[5].SetActive(comboID == 5);
                placeables[6].SetActive(comboID == 6);
                placeables[7].SetActive(comboID == 7);
                placeables[8].SetActive(comboID == 8);
            }
                        
        }

        [Button]
        public void ResetMoveableObjects()
        {
            for (int i = 0; i < moveableObjects.Length; i += 1)
            {
                moveableObjects[i].GetComponent<PhotonView>().RequestOwnership();
                moveableObjects[i].localPosition = ogMvblPos[i];
                moveableObjects[i].localRotation = ogMvblRot[i];
            }
        }

        public (float, float, int, int) GetConditionParams()
        {
            return (currentPosOffset, currentRotOffset, currentComboID, currentTimeLag);
        }

        public int GetCombinationID()
        {
            return currentComboID;
        }
    }

    public class US1Data
    {
        public string Device_name;
        public int Temporal_lag;
        public float Position_offset;
        public float Rotation_offset;
        public int Combination;
    }
}