using System;
using UnityEngine;

namespace MILab
{
    [CreateAssetMenu(menuName = "Metaverse/Health Channel")]
    public class HealthChannelSO : ScriptableObject
    {
        public bool _debug;
        public string PulsoidToken;
        
        public int _activeMenu;
        public Action OnShowHealth = delegate {  };
        public Action OnShowFitness = delegate {  };
        public Action OnShowSleep = delegate {  };
        public Action OnHideAll = delegate {  };
        
        [Button(Mode = ButtonMode.InPlayMode)]
        public void RaiseShowHealth()
        {
            Log("Show Health");
            _activeMenu = 0;
            OnShowHealth?.Invoke();
        }

        [Button(Mode = ButtonMode.InPlayMode)]
        public void RaiseShowFitness()
        {
            Log("Show Fitness");
            _activeMenu = 1;
            OnShowFitness?.Invoke();
        }

        [Button(Mode = ButtonMode.InPlayMode)]
        public void RaiseShowSleep()
        {
            Log("Show Sleep");
            _activeMenu = 2;
            OnShowSleep?.Invoke();
        }

        [Button(Mode = ButtonMode.InPlayMode)]
        public void RaiseHideAll()
        {
            Log("Hide All");
            _activeMenu = -1;
            OnHideAll?.Invoke();
        }

        private void Log(string message)
        {
            if (_debug) Debug.Log("[CVS] " + message);
        }
    }
}
