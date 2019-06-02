using System.Collections;
using System.Collections.Generic;
using KBEngine;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
public class wordUI : MonoBehaviour
{
    // Start is called before the first frame update
    public int relive_state = 0;
    public UnityEngine.GameObject[] buttonList;
    private int i;
    private UnityEngine.GameObject obj;
    void Start()
    {
        
    }
    public void relive_click()
    {
        KBEngine.Event.fireIn("relive", (Byte)1);
    }
    public void exit_click()
    {
        Application.Quit();
    }
    // Update is called once per frame
    void Update()
    {
        relive_state = PlayerPrefs.GetInt("player_state");
        if(relive_state == 1){
            for (i = 0; i < buttonList.Length; i++)
                buttonList[i].SetActive(true);
            Text text = buttonList[buttonList.Length - 1].GetComponent<Text>();
            text.text = "很遗憾，你的坦克生命值已低于0......";
            if(obj == null)
            {
                obj = UnityEngine.GameObject.Find("player");
            }
            else
            {
                obj.SetActive(false);
            }
        }
        else{
            for (i = 0; i < buttonList.Length; i++)
                buttonList[i].SetActive(false);
        }
    }
}
