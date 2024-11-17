using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Metaverse/Digital Twin Scene")]
public class DigitalTwinScene : ScriptableObject
{
    [SerializeField] private SerializedScene _scene;
    [SerializeField] private Material _skybox;
    [SerializeField, ReadOnly] private bool _valid;
    [SerializeField, ReadOnly] private string _sceneName;

    public bool Valid => _valid;
    public string SceneName => _sceneName;

    public Material Skybox => _skybox;

    private void OnValidate()
    {
        _valid = _scene._scene != null;
        _sceneName = _valid ? _scene.SceneName : "";
    }
}
