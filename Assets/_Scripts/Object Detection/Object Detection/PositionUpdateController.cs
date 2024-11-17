using UnityEngine;

public class PositionUpdateController : MonoBehaviour
{
    public MonoBehaviour script;

    public void StartUpdating()
    {
        // Enable the specified script
        script.enabled = true;
    }

    public void StopUpdating()
    {
        // Disable the specified script
        script.enabled = false;
    }
}