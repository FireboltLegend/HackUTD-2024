using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
public class Speak : MonoBehaviour
{
	[SerializeField] private GameObject avatar;
	[SerializeField, ReadOnly(true)] private AudioSource audioSource;
	[SerializeField] private TextAsset textFile;
	private bool isPlaying = false;
	// Start is called before the first frame update
	void Start()
	{
		
	}
	// Update is called once per frame
	void Update()
	{
		string filePath = Path.Combine(Application.dataPath, "sync.txt");
		AssetDatabase.Refresh();
		if (File.Exists(filePath))
		{
			string content = File.ReadAllText(filePath);
			if (content.Contains("a") && !isPlaying)
			{
				AssetDatabase.Refresh();
				audioSource = avatar.GetComponentInChildren<AudioSource>();
				PlayAudio(audioSource, "audio");
			}
		}
	}
	private void PlayAudio(AudioSource audioSource, string audioClipName)
	{
		AudioClip audioClip = Resources.Load<AudioClip>(audioClipName);
		if (audioSource != null && audioClip != null)
		{
			audioSource.clip = audioClip;
			audioSource.Play();
			isPlaying = true;
			StartCoroutine(CheckAudioPlayback(audioSource));
		}
	}
	private IEnumerator CheckAudioPlayback(AudioSource avatarAudioSource)
	{
		while (avatarAudioSource.isPlaying)
		{
			yield return null;
		}
		
		isPlaying = false;
		File.WriteAllText($"Assets/sync.txt", "b");
	}
}