using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;

public class SwitchTVSlide : MonoBehaviour
{
	[SerializeField] private GameObject energyCon;
	[SerializeField] private GameObject energyTariff;
	[SerializeField] private GameObject waterConWeek;
	[SerializeField] private GameObject waterConMonth;
	[SerializeField] private GameObject waterCon2Month;
	[SerializeField] MeshRenderer mr;
	[SerializeField, ReadOnly(true)] private string filePath;
	[SerializeField, ReadOnly(true)] private string content;

	// Start is called before the first frame update
	void Start()
	{
		mr = GetComponent<MeshRenderer>();
		filePath = Path.Combine(Application.dataPath, "user.txt");
	}

	// Update is called once per frame
	void Update()
	{
		AssetDatabase.Refresh();
		if (File.Exists(filePath))
		{
			content = File.ReadAllText(filePath);
			if(Input.GetKeyDown(KeyCode.A))
			{
					energyTariff.SetActive(false);
					waterCon2Month.SetActive(false);
					waterConMonth.SetActive(false);
					waterConWeek.SetActive(false);
					energyCon.SetActive(true);
			}
			else if(Input.GetKeyDown(KeyCode.B))
			{
				energyCon.SetActive(false);
				waterCon2Month.SetActive(false);
				waterConMonth.SetActive(false);
				waterConWeek.SetActive(false);
				energyTariff.SetActive(true);
			}
			else if(Input.GetKeyDown(KeyCode.C))
			{
				energyCon.SetActive(false);
				waterCon2Month.SetActive(false);
				waterConMonth.SetActive(false);
				energyTariff.SetActive(false);
				waterConWeek.SetActive(true);
			}
			else if(Input.GetKeyDown(KeyCode.D))
			{
				energyCon.SetActive(false);
				waterCon2Month.SetActive(false);
				energyTariff.SetActive(false);
				waterConWeek.SetActive(false);
				waterConMonth.SetActive(true);
			}
			else if(Input.GetKeyDown(KeyCode.E))
			{
				energyCon.SetActive(false);
				energyTariff.SetActive(false);
				waterConMonth.SetActive(false);
				waterConWeek.SetActive(false);
				waterCon2Month.SetActive(true);
			}
		}
	}
}
