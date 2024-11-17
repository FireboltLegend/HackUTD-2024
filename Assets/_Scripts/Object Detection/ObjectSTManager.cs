using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// The zones are defined as follows:
/// 
///  ============================================================
///  |              |              |                            |
///  |              |              |                            |
///  |      4       |      3       |                            |       A
///  |              |              |                            |       |   
///  |              |              |                            |       |
///  |==============|==============|  +0.5f                     |       |
///  |==============|==============|  -0.5f                     |       s
///  |              |              |                            |       i
///  |              |              |                            D       x
///  |      2       |      1       |                            O       a
///  |              |              |                            O       |
///  |              |              |                            R       z
///  ============================================================
///  -3.483f    -2.005f        -0.251f                      x-axis ------------->
/// </summary>

namespace MILab
{
    public class ObjectSTManager : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] PassthroughCtrl PassthroughController;

        [Header("Zones (TEST)")]
        [SerializeField] NewPositionUpdater[] monitorList;
        [SerializeField] PeripheralPositionUpdater[] keyboardList;
        [SerializeField] PeripheralPositionUpdater[] mouseList;

        /*[Header("Zone 1")]
        [SerializeField] NewPositionUpdater monitorZone1;
        [SerializeField] PeripheralPositionUpdater keyboardZone1;
        [SerializeField] PeripheralPositionUpdater mouseZone1;

        [Header("Zone 2")]
        [SerializeField] NewPositionUpdater monitorZone2;
        [SerializeField] PeripheralPositionUpdater keyboardZone2;
        [SerializeField] PeripheralPositionUpdater mouseZone2;

        [Header("Zone 3")]
        [SerializeField] NewPositionUpdater monitorZone3;
        [SerializeField] PeripheralPositionUpdater keyboardZone3;
        [SerializeField] PeripheralPositionUpdater mouseZone3;*/

        [Header("DEBUG")]
        [SerializeField, ReadOnly] int currentZone = -1;

        GameObject centerEye;

        void CheckPlayer()
        {
            if (centerEye == null)
                centerEye = GameObject.Find("CenterEyeAnchor");
        }

        public int GetCurrentZone()
        {
            return currentZone;
        }

        [Button]
        public void Zone0Dynamic()
        {
            monitorList[0].ToggleDynamicCalibrationMode();
            keyboardList[0].ToggleDynamicCalibrationMode();
            mouseList[0].ToggleDynamicCalibrationMode();
        }

        [Button]
        public void Zone1Dynamic()
        {
            monitorList[1].ToggleDynamicCalibrationMode();
            keyboardList[1].ToggleDynamicCalibrationMode();
            mouseList[1].ToggleDynamicCalibrationMode();
        }

        [Button]
        public void Zone0OneShot()
        {
            monitorList[0].ToggleCalibrationMode();
            keyboardList[0].ToggleCalibrationMode();
            mouseList[0].ToggleCalibrationMode();
        }

        [Button]
        public void Zone1OneShot()
        {
            monitorList[1].ToggleCalibrationMode();
            keyboardList[1].ToggleCalibrationMode();
            mouseList[1].ToggleCalibrationMode();
        }

        [Button]
        public void ToggleOneShotCalibrationMode()
        {
            if (currentZone == -1)
                return;
            monitorList[currentZone].ToggleCalibrationMode();
            keyboardList[currentZone].ToggleCalibrationMode();
            mouseList[currentZone].ToggleCalibrationMode();
        }

        [Button]
        public void ToggleDynamicCalibrationMode()
        {
            if (currentZone == -1)
                return;
            monitorList[currentZone].ToggleDynamicCalibrationMode();
            keyboardList[currentZone].ToggleDynamicCalibrationMode();
            mouseList[currentZone].ToggleDynamicCalibrationMode();
        }

        void TestingCheckAndUpdateZone()
        {
            CheckPlayer();
            //Debug.LogFormat("<color=yellow> Current Zone: " + currentZone + "</color>");
            if (centerEye.transform.eulerAngles.y < 165 && centerEye.transform.eulerAngles.y > 25)
                currentZone = 0;
            else if (centerEye.transform.eulerAngles.y > 180 && centerEye.transform.eulerAngles.y < 250)
                currentZone = 1;
            else
                currentZone = -1;
        }

        void CheckAndUpdateZone()
        {
            CheckPlayer();

            if (centerEye.transform.position.x < -0.251f &&
                centerEye.transform.position.x > -2.005f &&
                centerEye.transform.position.z < -0.5f)
                currentZone = 1;

            else if (centerEye.transform.position.x < -1.954f &&
                     centerEye.transform.position.x > -3.263f &&
                     centerEye.transform.position.y < -0.5f)
                currentZone = 2;

            else if (centerEye.transform.position.x < -0.191f &&
                centerEye.transform.position.x > -1.954f &&
                centerEye.transform.position.y < -0.5f)
                currentZone = 3;

            else if (centerEye.transform.position.x < -1.954f &&
                     centerEye.transform.position.x > -3.263f &&
                     centerEye.transform.position.y < -0.5f)
                currentZone = 4;

            else
                currentZone = -1;
        }

        // Start is called before the first frame update
        void Start()
        {
            centerEye = GameObject.Find("CenterEyeAnchor");
        }

        // Update is called once per frame
        void Update()
        {
            TestingCheckAndUpdateZone();
        }
    } 
}
