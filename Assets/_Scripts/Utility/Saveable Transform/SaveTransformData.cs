using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MILab
{
    [System.Serializable]
    public class SaveTransformData
    {

        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public void LoadFromJson(string a_Json)
        {
            JsonUtility.FromJsonOverwrite(a_Json, this);
        }

    }
}