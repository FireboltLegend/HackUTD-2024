// Copyright (c) 2016 Unity Technologies. MIT license - license_unity.txt
// #NVJOB Simple Boids. MIT license - license_nvjob.txt
// #NVJOB Nicholas Veselov - https://nvjob.github.io
// #NVJOB Simple Boids v1.1.1 - https://nvjob.github.io/unity/nvjob-boids

using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[HelpURL("https://nvjob.github.io/unity/nvjob-boids")]

public class BoidController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private Vector2 _behavioralCh = new Vector2(2.0f, 6.0f);
    [SerializeField] private Vector3 _origin;
    [SerializeField] private Vector3 _originBounds;
    [SerializeField] private float _maxDistFromOrigin = 50;
    [SerializeField] private bool _debug;
    [SerializeField] private bool _moveFrame;

    [Header("Flock Settings")]
    [SerializeField, Range(1, 150)] private int _flockNum = 2;
    [SerializeField, Range(0, 25)] private float _flockTravelDistance = 1;
    [SerializeField, Range(0, 10)] private float _flockMaxHeight = 0.5f;
    [SerializeField, Range(0, 1.0f)] private float _migrationFrequency = 0.1f;
    [SerializeField, Range(0, 1.0f)] private float _posChangeFrequency = 0.5f;
    [SerializeField, Range(0, 100)] private float _smoothChFrequency = 0.5f;

    [Header("Bird Settings")]
    [SerializeField] private GameObject _birdPref;
    [SerializeField, Range(1, 9999)] private int _birdsNum = 10;
    [SerializeField, Range(0, 150)] private float _birdSpeed = 1;
    [SerializeField, Range(0, 100)] private int _fragmentedBirds = 10;
    [SerializeField, Range(0, 1)] private float _fragmentedBirdsYLimit = 1;
    [SerializeField, Range(0, 10)] private float _soaring = 0.5f;
    [SerializeField, Range(0.01f, 500)] private float _verticalWave = 20;
    [SerializeField] private bool _rotationClamp;
    [SerializeField, Range(0, 360)] private float _rotationClampValue = 50;
    [SerializeField] private Vector2 _scaleRandom = new Vector2(1.0f, 1.5f);

    [Header("Danger Settings (one flock)")]
    [SerializeField] private bool _danger;
    [SerializeField] private float _dangerRadius = 15;
    [SerializeField] private float _dangerSpeed = 1.5f;
    [SerializeField] private float _dangerSoaring = 0.5f;
    [SerializeField] private LayerMask _dangerLayer;

    //-------------- 

    private Transform _thisTransform;
    private Transform _dangerTransform;
    private int _dangerBird;
    private Transform[] _birdsTransform;
    private Transform[] _flocksTransform;
    private Vector3[] _rdTargetPos;
    private Vector3[] _flockPos;
    private Vector3[] _velFlocks;
    private float[] _birdsSpeed;
    private float[] _birdsSpeedCur;
    private float[] _spVelocity;
    private int[] _currentFlock;
    private float _dangerSpeedCh;
    private float _timeTime;
    private static WaitForSeconds _delay0;


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private void Awake()
    {
        _thisTransform = transform;
        CreateFlock();
        CreateBird();
        StartCoroutine(BehavioralChange());
        StartCoroutine(Danger());
        _timeTime += Random.value * 5;
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private void Update()
    {
        _moveFrame = !_moveFrame;
        if (_moveFrame)
        {
            FlocksMove();
            BirdsMove();
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private void FlocksMove()
    {
        for (int f = 0; f < _flockNum; f++)
        {
            _flocksTransform[f].position = Vector3.SmoothDamp(_flocksTransform[f].position, _flockPos[f], ref _velFlocks[f], _smoothChFrequency);
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private void BirdsMove()
    {
        float deltaTime = Time.deltaTime;
        _timeTime += deltaTime;
        var translateCur = Vector3.forward * (_birdSpeed * _dangerSpeedCh * deltaTime);
        var verticalWaveCur = Vector3.up * ((_verticalWave * 0.5f) - Mathf.PingPong(_timeTime * 0.5f, _verticalWave));
        float soaringCur = _soaring * _dangerSoaring * deltaTime;

        for (int b = 0; b < _birdsNum; b++)
        {
            if (Math.Abs(_birdsSpeedCur[b] - _birdsSpeed[b]) > 0.1f) _birdsSpeedCur[b] = Mathf.SmoothDamp(_birdsSpeedCur[b], _birdsSpeed[b], ref _spVelocity[b], 0.5f);
            _birdsTransform[b].Translate(translateCur * _birdsSpeed[b]);
            var tpCh = _flocksTransform[_currentFlock[b]].position + _rdTargetPos[b] + verticalWaveCur - _birdsTransform[b].position;
            var rotationCur = Quaternion.LookRotation(Vector3.RotateTowards(_birdsTransform[b].forward, tpCh, soaringCur, 0));
            if (_rotationClamp == false) _birdsTransform[b].rotation = rotationCur;
            else _birdsTransform[b].localRotation = BirdsRotationClamp(rotationCur, _rotationClampValue);
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private IEnumerator Danger()
    {
        if (_danger)
        {
            _delay0 = new WaitForSeconds(1.0f);

            while (true)
            {
                if (Random.value > 0.9f) _dangerBird = Random.Range(0, _birdsNum);
                _dangerTransform.localPosition = _birdsTransform[_dangerBird].localPosition;

                if (Physics.CheckSphere(_dangerTransform.position, _dangerRadius, _dangerLayer))
                {
                    _dangerSpeedCh = _dangerSpeed;
                    yield return _delay0;
                }
                else _dangerSpeedCh = 1;

                yield return _delay0;
            }
        }
        _dangerSpeedCh = 1;
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private Vector3 RandomFlockPos(Vector3 currentPos)
    {
        Log("Start: " + currentPos);
        var rand = Random.onUnitSphere * (_flockTravelDistance + (Random.value * 0.25f + 0.75f));
        Log("Rand" + rand);
        currentPos += rand;
        if (Vector3.Distance(currentPos, _origin) > _maxDistFromOrigin)
        {
            currentPos = Vector3.Lerp(currentPos, _origin, 0.25f);
            Log("Out of Range: " + currentPos);
        }

        bool inX = Mathf.Abs(currentPos.x - _origin.x) < _originBounds.x;
        bool inZ = Mathf.Abs(currentPos.z - _origin.z) < _originBounds.z;
        if (inX && inZ)
        {
            if (currentPos.x > _origin.x)
            {
                currentPos.x = _origin.x + _originBounds.x + 1;
            }
            else if (currentPos.x < _origin.x)
            {
                currentPos.x = _origin.x - _originBounds.x - 1;
            }

            if (currentPos.z > _origin.z)
            {
                currentPos.z = _origin.z + _originBounds.z + 1;
            }
            else if (currentPos.z < _origin.z)
            {
                currentPos.z = _origin.z - _originBounds.z - 1;
            }
        }
        currentPos.y = Mathf.Clamp(currentPos.y, 0, _flockMaxHeight);
        Log(currentPos.ToString());
        return currentPos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_origin, _originBounds * 2);
    }


    private IEnumerator BehavioralChange()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_behavioralCh.x, _behavioralCh.y));
            
            if (_debug) Debug.Log("Update Behaviour");

            //---- Flocks

            for (int f = 0; f < _flockNum; f++)
            {
                if (Random.value < _posChangeFrequency)
                {
                    _flockPos[f] = RandomFlockPos(_flockPos[f]);
                }
            }

            //---- Birds

            for (int b = 0; b < _birdsNum; b++)
            {
                _birdsSpeed[b] = Random.Range(3.0f, 7.0f);
                var lpv = Random.insideUnitSphere * _fragmentedBirds;
                _rdTargetPos[b] = new Vector3(lpv.x, lpv.y * _fragmentedBirdsYLimit, lpv.z);
                if (Random.value < _migrationFrequency) _currentFlock[b] = Random.Range(0, _flockNum);
            } 
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private void CreateFlock()
    {
        _flocksTransform = new Transform[_flockNum];
        _flockPos = new Vector3[_flockNum];
        _velFlocks = new Vector3[_flockNum];
        _currentFlock = new int[_birdsNum];

        for (int f = 0; f < _flockNum; f++)
        {
            var nobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nobj.SetActive(_debug);
            _flocksTransform[f] = nobj.transform;
            _flocksTransform[f].position = _thisTransform.position;
            _flockPos[f] = RandomFlockPos(transform.position);
            _flocksTransform[f].parent = _thisTransform;
        }

        if (_danger)
        {
            var dobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dobj.GetComponent<MeshRenderer>().enabled = _debug;
            dobj.layer = gameObject.layer;
            _dangerTransform = dobj.transform;
            _dangerTransform.parent = _thisTransform;
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    private void CreateBird()
    {
        _birdsTransform = new Transform[_birdsNum];
        _birdsSpeed = new float[_birdsNum];
        _birdsSpeedCur = new float[_birdsNum];
        _rdTargetPos = new Vector3[_birdsNum];
        _spVelocity = new float[_birdsNum];

        for (int b = 0; b < _birdsNum; b++)
        {
            _birdsTransform[b] = Instantiate(_birdPref, _thisTransform).transform;
            var lpv = Random.insideUnitSphere * _fragmentedBirds;
            _birdsTransform[b].localPosition = _rdTargetPos[b] = new Vector3(lpv.x, lpv.y * _fragmentedBirdsYLimit, lpv.z);
            _birdsTransform[b].localScale = Vector3.one * Random.Range(_scaleRandom.x, _scaleRandom.y);
            _birdsTransform[b].localRotation = Quaternion.Euler(0, Random.value * 360, 0);
            _currentFlock[b] = Random.Range(0, _flockNum);
            _birdsSpeed[b] = Random.Range(3.0f, 7.0f);
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private static Quaternion BirdsRotationClamp(Quaternion rotationCur, float rotationClampValue)
    {
        var angleClamp = rotationCur.eulerAngles;
        rotationCur.eulerAngles = new Vector3(Mathf.Clamp((angleClamp.x > 180) ? angleClamp.x - 360 : angleClamp.x, -rotationClampValue, rotationClampValue), angleClamp.y, 0);
        return rotationCur;
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void Log(string message)
    {
        if (_debug) Debug.Log(message, gameObject);
    }
}