using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MILab
{
    public class LampData
    {
        public string Device_name;
        public float Brightness;
        public ColorList ColorList;
        public string Color;
        public Pattern Pattern;
        public bool lamp_status;
    }
    public class ColorList
    {
        public bool ColorList_status;
        public float ColorList_speed;
        public string ColorList_list;
    }
    public class Pattern
    {
        public bool Pattern_status;
        public float Pattern_code;
        public float Pattern_speed;
    }
}