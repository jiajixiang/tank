using KBEngine;
using UnityEngine;
using System; 
using System.IO;  
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour 
{

	void Awake() 
	{
		DontDestroyOnLoad(transform.gameObject);
	}
	void Start ()
	{
		PlayerPrefs.SetInt("ui_state", 0);
		SceneManager.LoadScene("login");
	}
}