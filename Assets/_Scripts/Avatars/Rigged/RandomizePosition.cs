using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizePosition : MonoBehaviour
{
    [SerializeField] private Vector2 _minMaxDelay = new Vector2(0.2f, 1f);
    [SerializeField] private Vector2 _minMaxPosX = new Vector2(-1f, 1f);
    [SerializeField] private Vector2 _minMaxPosY = new Vector2(-1f, 1f);
    [SerializeField] private Vector2 _minMaxPosZ = new Vector2(-1f, 1f);
    
    private float _timer;

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer < 0)
        {
            Randomize();
        }
    }

    private void Randomize()
    {
        _timer = GetRandom(_minMaxDelay);
        transform.localPosition = new Vector3(GetRandom(_minMaxPosX), GetRandom(_minMaxPosY), GetRandom(_minMaxPosZ));
    }

    private static float GetRandom(Vector2 minMax) => Random.Range(minMax.x, minMax.y);
}
