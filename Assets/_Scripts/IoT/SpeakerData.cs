using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MILab
{
    public class SpeakerData
    {
        public string Device_name;
        public bool is_playing;
        public float volume;
        public int song_id;
        public string[] songs = new string[] { "lamp.mp3", "Monitor.mp3", "ds_song.mp3" };
    }
}