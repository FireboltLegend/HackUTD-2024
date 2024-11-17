using System;
using System.Collections;
using System.Collections.Generic;
using MILab;
using UnityEngine;

public class CvsVisualsResponse : MonoBehaviour
{
    [SerializeField] private HealthChannelSO _responses;
    [SerializeField] private GameObject _health;
    [SerializeField] private GameObject _fitness;
    [SerializeField] private GameObject _sleep;

    private void OnEnable()
    {
        _responses.OnHideAll += OnHideAll;
        _responses.OnShowHealth += OnShowHealth;
        _responses.OnShowFitness += OnShowFitness;
        _responses.OnShowSleep += OnShowSleep;

        OnHideAll();
        switch (_responses._activeMenu)
        {
            case 0:
                //OnShowHealth();
                break;
            case 1:
                //OnShowFitness();
                break;
            case 2:
                //OnShowSleep();
                break;
        }
    }

    private void OnDisable()
    {
        _responses.OnHideAll -= OnHideAll;
        _responses.OnShowHealth -= OnShowHealth;
        _responses.OnShowFitness -= OnShowFitness;
        _responses.OnShowSleep -= OnShowSleep;
    }

    private void OnShowHealth()
    {
        _health.SetActive(true);
    }

    private void OnShowFitness()
    {
        _fitness.SetActive(true);
    }

    private void OnShowSleep()
    {
        _sleep.SetActive(true);
    }

    private void OnHideAll()
    {
        _health.SetActive(false);
        _fitness.SetActive(false);
        _sleep.SetActive(false);
    }
}
