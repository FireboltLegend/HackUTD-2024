using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MILab;

public class ObjectSTController : MonoBehaviour
{
    GameObject monitor;
    GameObject peripheral;
    GameObject peripheral2;

    GameObject objSTManager;
    [SerializeField] GameObject toggleCalibrationButtonBackplate;
    [SerializeField] GameObject toggleDynamicButtonBackplate;

    void CheckObject()
    {
        /*if (monitor == null)
            monitor = GameObject.Find("PosRotTestMonitor");
        if (peripheral == null)
            peripheral = GameObject.Find("TestKeyboardMouseSet").transform.Find("Keyboard_Mechanical").gameObject;
        if (peripheral2 == null)
            peripheral2 = GameObject.Find("TestKeyboardMouseSet").transform.Find("Mouse").gameObject;*/
        if (objSTManager == null)
            objSTManager = GameObject.Find("ObjectSTManager");
    }

    // ======================== ALL =============================

    public void ResetPositionAndRotation()
    {
        CheckObject();

        monitor.GetComponent<NewPositionUpdater>().UpdatePositionAndPose();
        peripheral.GetComponent<PeripheralPositionUpdater>().UpdatePositionAndPose();
        peripheral2.GetComponent<PeripheralPositionUpdater>().UpdatePositionAndPose();
    }

    [Button]
    public void ToggleObjectCalibration()
    {
        CheckObject();
        /*bool isCalibrationModeActive = monitor.GetComponent<NewPositionUpdater>().ToggleCalibrationMode();
        toggleCalibrationButtonBackplate.SetActive(isCalibrationModeActive);

        isCalibrationModeActive = peripheral.GetComponent<PeripheralPositionUpdater>().ToggleCalibrationMode();
        toggleCalibrationButtonBackplate.SetActive(isCalibrationModeActive);

        isCalibrationModeActive = peripheral2.GetComponent<PeripheralPositionUpdater>().ToggleCalibrationMode();
        toggleCalibrationButtonBackplate.SetActive(isCalibrationModeActive);*/

        objSTManager.GetComponent<ObjectSTManager>().ToggleOneShotCalibrationMode();
    }

    [Button]
    public void ToggleObjectDynamicAndManual()
    {
        CheckObject();
        /*bool isDynamicActive = monitor.GetComponent<NewPositionUpdater>().ToggleDynamicCalibrationMode();
        toggleDynamicButtonBackplate.SetActive(isDynamicActive);

        isDynamicActive = peripheral.GetComponent<PeripheralPositionUpdater>().ToggleDynamicCalibrationMode();
        toggleDynamicButtonBackplate.SetActive(isDynamicActive);

        isDynamicActive = peripheral2.GetComponent<PeripheralPositionUpdater>().ToggleDynamicCalibrationMode();
        toggleDynamicButtonBackplate.SetActive(isDynamicActive);*/

        objSTManager.GetComponent<ObjectSTManager>().ToggleDynamicCalibrationMode();
    }

    
    // ======================== MONITOR =============================
    public void ResetMonitorPositionAndRotation()
    {
        CheckObject();

        monitor.GetComponent<NewPositionUpdater>().UpdatePositionAndPose();
    }

    [Button]
    public void ToggleCalibration()
    {
        CheckObject();
        bool isCalibrationModeActive = monitor.GetComponent<NewPositionUpdater>().ToggleCalibrationMode();
        toggleCalibrationButtonBackplate.SetActive(isCalibrationModeActive);
    }

    [Button]
    public void ToggleDynamicAndManual()
    {
        CheckObject();
        bool isDynamicActive = monitor.GetComponent<NewPositionUpdater>().ToggleDynamicCalibrationMode();
        toggleDynamicButtonBackplate.SetActive(isDynamicActive);
    }


    // ============== PERIPHERAL ========================================

    public void ResetPeripheralPositionAndRotation()
    {
        CheckObject();

        peripheral.GetComponent<PeripheralPositionUpdater>().UpdatePositionAndPose();
    }

    [Button]
    public void TogglePeripheralCalibration()
    {
        CheckObject();
        bool isCalibrationModeActive = peripheral.GetComponent<PeripheralPositionUpdater>().ToggleCalibrationMode();
        toggleCalibrationButtonBackplate.SetActive(isCalibrationModeActive);
    }

    [Button]
    public void TogglePeripheralDynamicAndManual()
    {
        CheckObject();
        bool isDynamicActive = peripheral.GetComponent<PeripheralPositionUpdater>().ToggleDynamicCalibrationMode();
        toggleDynamicButtonBackplate.SetActive(isDynamicActive);
    }

    // Start is called before the first frame update
    void Start()
    {
        monitor = GameObject.Find("PosRotTestMonitor");
        peripheral = GameObject.Find("TestKeyboardMouseSet").transform.Find("Keyboard_Mechanical").gameObject;
        peripheral2 = GameObject.Find("TestKeyboardMouseSet").transform.Find("Mouse").gameObject;
        objSTManager = GameObject.Find("ObjectSTManager");
    }

}
