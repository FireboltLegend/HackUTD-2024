using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MILab
{
    //Class for ObjectData (what we will receive from the database)
    public class ObjectData
    {
        public string Detected_Object;
        public string Object_name;
        public string Coordinate;
        public string LeftCoordinate;
        public string RightCoordinate;
        public bool IsDetected;
        public int IsDynamicUpdateEnabled;
        public string Position;
        public string Rotation;
        public float Angle;
    }
}
