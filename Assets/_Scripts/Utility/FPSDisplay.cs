using UnityEngine;
using UnityEngine.UI;
 
public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private TextMesh _fpsText;
    [SerializeField] private float _updateInterval = 0.5f;
    
    private float _fps;
    private float _accum;
    private int _frames;
    private float _timeLeft;

    
    private void Update() {
        _timeLeft -= Time.deltaTime;
        _accum += Time.timeScale / Time.deltaTime;
        ++_frames;

        if (_timeLeft <= 0.0) {
            _fps = (_accum / _frames);
            _timeLeft = _updateInterval;
            _accum = 0.0f;
            _frames = 0;
        }
        
        if (_fpsText != null) _fpsText.text = "FPS: " + _fps;
    }
}
