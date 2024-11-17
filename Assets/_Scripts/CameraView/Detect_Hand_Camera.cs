using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MILab
{
    public enum HandType
    {
        Left,
        Right
    }

    [System.Serializable]
    public struct Gesture
    {
        public string name;
        public HandType handType; // Added field to store the hand type
        public List<Vector3> fingerDatas;
        public UnityEvent onRecognized;
    }

    public class Detect_Hand_Camera : MonoBehaviour
    {
        public float threshold = 0.1f;
        [SerializeField] OVRSkeleton leftSkeleton;
        [SerializeField] OVRSkeleton rightSkeleton;
        public List<Gesture> gestures;
        [SerializeField] int leftvalue = -1;
        [SerializeField] int rightvalue = -1;

        [SerializeField] List<string> leftFingerBoneNames;
        [SerializeField] List<string> rightFingerBoneNames;
        [SerializeField] int sum = 0;

        [SerializeField, ReadOnly] HandGestureDummy handGestureController;


        private Gesture previousGesture;

        void Start()
        {
            previousGesture = new Gesture();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }

            UpdateFingerBoneNames();

            RecognizeGesture(leftSkeleton, leftFingerBoneNames, HandType.Left, ref leftvalue);
            RecognizeGesture(rightSkeleton, rightFingerBoneNames, HandType.Right, ref rightvalue);
            checktwohands();
        }

        void Save()
        {
            SaveGesture(leftSkeleton, leftFingerBoneNames, HandType.Left);
            SaveGesture(rightSkeleton, rightFingerBoneNames, HandType.Right);
        }

        void UpdateFingerBoneNames()
        {
            // Update finger bone names for left hand
            leftFingerBoneNames = GetBoneNames(leftSkeleton);

            // Update finger bone names for right hand
            rightFingerBoneNames = GetBoneNames(rightSkeleton);
        }

        List<string> GetBoneNames(OVRSkeleton skeleton)
        {
            List<string> boneNames = new List<string>();

            if (skeleton != null && skeleton.Bones != null)
            {
                foreach (var bone in skeleton.Bones)
                {
                    boneNames.Add(bone.Id.ToString());
                }
            }

            return boneNames;
        }

        void SaveGesture(OVRSkeleton skeleton, List<string> fingerBoneNames, HandType handType)
        {
            Gesture g = new Gesture();
            g.name = "New Gesture";
            g.handType = handType;
            g.fingerDatas = new List<Vector3>();

            if (skeleton != null && skeleton.Bones != null)
            {
                foreach (var bone in skeleton.Bones)
                {
                    g.fingerDatas.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
                }
            }
            else
            {
                Debug.LogWarning("Skeleton or Bones not properly initialized.");
            }

            gestures.Add(g);
        }

        void RecognizeGesture(OVRSkeleton skeleton, List<string> fingerBoneNames, HandType handType, ref int currvalue)
        {
            currvalue = -1;


            foreach (var gesture in gestures)
            {
                if (gesture.handType != handType)
                {
                    continue; // Skip gestures not belonging to the current hand type
                }

                float sumDistance = 0;
                bool isDiscarded = false;

                for (int i = 0; i < gesture.fingerDatas.Count; i++)
                {
                    Vector3 currentData = skeleton.transform.InverseTransformPoint(skeleton.Bones[i].Transform.position);
                    float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);

                    if (distance > threshold)
                    {
                        isDiscarded = true;
                        break;
                    }

                    sumDistance += distance;
                }

                if (!isDiscarded && sumDistance < threshold)
                {
                    if (!gesture.Equals(previousGesture))
                    {


                        Debug.Log("New gesture found: " + gesture.name + " from " + handType.ToString());
                        //Debug.Log("current index: " + gestures.IndexOf( gesture));
                        if (currvalue == -1)
                        {
                            currvalue = gestures.IndexOf(gesture);
                        }

                        previousGesture = gesture;
                        gesture.onRecognized.Invoke();



                    }
                }
            }




        }

        void checktwohands()
        {
            if (handGestureController == null)
                handGestureController = GameObject.Find("HandGestureController").GetComponent<HandGestureDummy>();

            if ((leftvalue == 0 || leftvalue == 2 || leftvalue == 4 || leftvalue == 6) || (rightvalue == 1 || rightvalue == 3 || rightvalue == 5) || rightvalue == 7)
            {
                Debug.Log("Activate Camera");
                handGestureController.ShowCameraView(true);
            }
           /* else
            {
                handGestureController.ShowCameraView(false);
            }*/
        }



    } 
}