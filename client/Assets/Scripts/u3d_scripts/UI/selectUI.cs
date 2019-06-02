using KBEngine;
using UnityEngine;
using UnityEngine.UI;
using System; 
using System.IO;  
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class selectUI : MonoBehaviour {
	public UnityEngine.GameObject[] Manager;
	public Button[] roleList;
	public Button enterGame;
	public InputField avatarInfoInput;
	public Text info_text;
	bool startRelogin = false;
	private Dictionary<UInt64, AVATAR_INFOS> ui_avatarList = null;
	private UInt64 selAvatarDBID = 0;
	private Color labelColor = Color.green;
	private string labelMsg;
	private string stringAvatarName;
	void Start () 
	{
		installEvents();
        //设置当前分辨率 
		Resolution[] resolutions = Screen.resolutions;
        Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);
        Screen.fullScreen = true;  //设置成全屏
		new_ui_avatarList();
		setAvatarList();
	}

	void installEvents()
	{
		// common
		KBEngine.Event.registerOut("onKicked", this, "onKicked");
		KBEngine.Event.registerOut("onDisconnected", this, "onDisconnected");
		KBEngine.Event.registerOut("onConnectionState", this, "onConnectionState");

		KBEngine.Event.registerOut("onReqAvatarList", this, "onReqAvatarList");
		KBEngine.Event.registerOut("onCreateAvatarResult", this, "onCreateAvatarResult");
		KBEngine.Event.registerOut("onRemoveAvatar", this, "onRemoveAvatar");
	}

	void OnDestroy()
	{
		KBEngine.Event.deregisterOut(this);
	}

	void new_ui_avatarList()
	{
		if(KBEngineApp.app.entity_type == "Account")
		{
			KBEngine.Account account = (KBEngine.Account)KBEngineApp.app.player();
			if(account != null)
				ui_avatarList = new Dictionary<UInt64, AVATAR_INFOS>(account.avatars);
		}
	}

	void setAvatarList()
    {
        int idx = 0;
        if (ui_avatarList != null && ui_avatarList.Count > 0)
		{
			foreach(UInt64 dbid in ui_avatarList.Keys)
			{
				roleList[idx].enabled = true;
				Text text = roleList[idx].GetComponentInChildren<Text>().GetComponent<Text>();
				AVATAR_INFOS info = ui_avatarList[dbid];
				text.text = info.name;
				if(selAvatarDBID == info.dbid)
					roleList[idx].GetComponent<Image>().color = Color.green;
				else
					roleList[idx].GetComponent<Image>().color = Color.white;
				roleList[idx].transform.localPosition = new Vector3(0, 200 - 200 * idx, 0);
				idx++;
			}
		}
		else
		{
			new_ui_avatarList();
		}
        for (; idx < 3; idx++)
        {
            roleList[idx].transform.localPosition = new Vector3(0, 1000, 0);
        }
    }
	public void selectRoleIdx(int value)
	{
		Debug.Log(" -------selectRoleIdx ----" + value);
		int idx = 0;
		foreach(UInt64 dbid in ui_avatarList.Keys)
		{
			AVATAR_INFOS info = ui_avatarList[dbid];
			if(idx == value){
				selAvatarDBID = info.dbid;
			}
			if(selAvatarDBID == info.dbid){
				roleList[idx].GetComponent<Image>().color = Color.green;
			}
			else{
				roleList[idx].GetComponent<Image>().color = Color.white;
			}
			idx++;
		}
	}
	// Update is called once per frame
	void Update () 
	{
		setAvatarList();
		Text text = info_text.GetComponent<Text>();
		text.text = labelMsg;
		text.color = labelColor;
	}

	public void createAvaterOnLick()
	{
		if(ui_avatarList.Count < 3)
		{
			Manager[0].SetActive(false);
			Manager[1].SetActive(true);
		}
	}

	public void deleteAvaterOnLick()
	{
		if(selAvatarDBID == 0)
		{
			err("请选择角色!");
		}
		else
		{
			info("请稍后...");
			if(ui_avatarList != null && ui_avatarList.Count > 0)
			{
				AVATAR_INFOS avatarinfo = ui_avatarList[selAvatarDBID];
				if(avatarinfo == null) 
					return;
				KBEngine.Event.fireIn("reqRemoveAvatar", avatarinfo.name);
                selAvatarDBID = 0;
            }
		}
	}
	public void enterGameOnLick()
	{
		if(selAvatarDBID == 0)
        {
			err("请选择角色!");
        }
        else
        {
			info("请稍后...");
			KBEngine.Event.fireIn("selectAvatarGame", selAvatarDBID);
			PlayerPrefs.SetInt("ui_state", 2);
			SceneManager.LoadScene("world");
		}
	}

	public void avatarCreatedOnLick()
	{
		Manager[0].SetActive(true);
		Manager[1].SetActive(false);
		if(ui_avatarList.Count < 3){
			KBEngine.Event.fireIn("reqCreateAvatar", (Byte)1, stringAvatarName);
		}
	}

	public void avatarInfoInputUpdate()
	{
		stringAvatarName = avatarInfoInput.text;
	}

	public void err(string s)
	{
		labelColor = Color.red;
		labelMsg = s;
		Debug.Log("select ------- err = " + s);
	}
	public void info(string s)
	{
		labelColor = Color.green;
		labelMsg = s;
		Debug.Log("select ------- info = " + s);
	}
	public void onKicked(UInt16 failedcode)
	{
		err("kick, disconnect!, reason=" + KBEngineApp.app.serverErr(failedcode));
		SceneManager.LoadScene("login");
	}

	public void onReqAvatarList(Dictionary<UInt64, AVATAR_INFOS> avatarList)
	{
		ui_avatarList = avatarList;
		Debug.Log(" -------- " + ui_avatarList.Count);
	}

	public void onCreateAvatarResult(UInt64 retcode, AVATAR_INFOS info, Dictionary<UInt64, AVATAR_INFOS> avatarList)
	{
		if(retcode != 0)
		{
			err("创建角色失败, errcode=" + retcode);
			return;
		}
		onReqAvatarList(avatarList);
	}
	
	public void onRemoveAvatar(UInt64 dbid, Dictionary<UInt64, AVATAR_INFOS> avatarList)
	{
		if(dbid == 0)
		{
			err("删除角色错误!");
			return;
		}
		onReqAvatarList(avatarList);
	}

	public void onDisconnected()
	{
		err("你已掉线，尝试重连中!");
		startRelogin = true;
		Invoke("onReloginBaseappTimer", 1.0f);
	}
	
	public void onReloginBaseappTimer() 
	{
		if(PlayerPrefs.GetInt("ui_state") == 0)
		{
			err("你已掉线!");
			return;
		}
	
		KBEngineApp.app.reloginBaseapp();
		
		if(startRelogin)
			Invoke("onReloginBaseappTimer", 3.0f);
	}
	public void onConnectionState(bool success)
	{
		if(!success)
			err("connect(" + KBEngineApp.app.getInitArgs().ip + ":" + KBEngineApp.app.getInitArgs().port + ") 连接错误");
		else
			info("连接成功，请等候...");
	}
}
