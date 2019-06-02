using KBEngine;
using UnityEngine;
using UnityEngine.UI;
using System; 
using System.IO;  
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class loginUI : MonoBehaviour {
	public InputField accountInput;
	public InputField passwordInput;
	private Color labelColor = Color.green;
	private string labelMsg = " 欢迎 ";
	private string stringAccount, stringPasswd;
	public Text info_text;
	bool startRelogin = false;

	void Start () 
	{   
		Debug.Log(" Login Start ");
		installEvents();
		Resolution[] resolutions = Screen.resolutions;
        Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);
        Screen.fullScreen = true;  //设置成全屏
	}

	void installEvents()
	{
		// common
		KBEngine.Event.registerOut("onKicked", this, "onKicked");
		KBEngine.Event.registerOut("onDisconnected", this, "onDisconnected");
		KBEngine.Event.registerOut("onConnectionState", this, "onConnectionState");

		// login
		KBEngine.Event.registerOut("onCreateAccountResult", this, "onCreateAccountResult");
		KBEngine.Event.registerOut("onLoginFailed", this, "onLoginFailed");
		KBEngine.Event.registerOut("onVersionNotMatch", this, "onVersionNotMatch");
		KBEngine.Event.registerOut("onScriptVersionNotMatch", this, "onScriptVersionNotMatch");
		KBEngine.Event.registerOut("onLoginBaseappFailed", this, "onLoginBaseappFailed");
		KBEngine.Event.registerOut("onLoginSuccessfully", this, "onLoginSuccessfully");
		KBEngine.Event.registerOut("onReloginBaseappFailed", this, "onReloginBaseappFailed");
		KBEngine.Event.registerOut("onReloginBaseappSuccessfully", this, "onReloginBaseappSuccessfully");
		KBEngine.Event.registerOut("onLoginBaseapp", this, "onLoginBaseapp");
		KBEngine.Event.registerOut("Loginapp_importClientMessages", this, "Loginapp_importClientMessages");
		KBEngine.Event.registerOut("Baseapp_importClientMessages", this, "Baseapp_importClientMessages");
		KBEngine.Event.registerOut("Baseapp_importClientEntityDef", this, "Baseapp_importClientEntityDef");
	}
	void OnDestroy()
	{
		KBEngine.Event.deregisterOut(this);
	}
	//登陆
	public void accountInputUpdate()
	{
		Debug.Log(" Login accountInputUpdate ");
		stringAccount = accountInput.text;
	}
	public void passwordInputUpdate()
	{
		Debug.Log(" Login passwordInputUpdate ");
		stringPasswd = passwordInput.text;
	}
	public void loginClick()
	{
		Debug.Log(" Login loginClick ");
		if(stringAccount.Length > 0 && stringPasswd.Length > 5){
			KBEngine.Event.fireIn("login", stringAccount, stringPasswd, System.Text.Encoding.UTF8.GetBytes("kbengine_unity3d_demo"));
		}
		else{
			err("账号或者密码错误，长度必须大于5!");
		}
    }
	public void registeredClick()
	{
		Debug.Log(" Login registeredClick ");
		if(stringAccount.Length > 0 && stringPasswd.Length > 5){
			KBEngine.Event.fireIn("createAccount", stringAccount, stringPasswd, System.Text.Encoding.UTF8.GetBytes("kbengine_unity3d_demo"));
		}else{
			err("账号或者密码错误，长度必须大于5!");
		}
    }
	public void Update()
	{
		if(KBEngineApp.app != null && KBEngineApp.app.serverVersion != "" 
			&& KBEngineApp.app.serverVersion != KBEngineApp.app.clientVersion)
		{
			labelMsg = "version not match(curr=" + KBEngineApp.app.clientVersion + ", srv=" + KBEngineApp.app.serverVersion + " )(版本不匹配)";
            labelMsg += "\nExecute the gensdk script to generate matching client SDK in the server-assets directory.";
            labelMsg += "\n(在服务端的资产目录下执行gensdk脚本生成匹配的客户端SDK)";
        }
		else if(KBEngineApp.app != null && KBEngineApp.app.serverScriptVersion != "" 
			&& KBEngineApp.app.serverScriptVersion != KBEngineApp.app.clientScriptVersion)
		{
			labelMsg = "scriptVersion not match(curr=" + KBEngineApp.app.clientScriptVersion + ", srv=" + KBEngineApp.app.serverScriptVersion + " )(脚本版本不匹配)";
		}
		Text text = info_text.GetComponent<Text>();
		text.text = labelMsg;
		text.color = labelColor;
		Debug.Log("Login text.text = " + text.text);
	}
	public void err(string s)
	{
		labelColor = Color.red;
		labelMsg = s;
		Debug.Log("Login ------- err = " + s);
	}
	public void info(string s)
	{
		labelColor = Color.green;
		labelMsg = s;
		Debug.Log("Login ------- info = " + s);
	}

	public void onCreateAccountResult(UInt16 retcode, byte[] datas)
	{
		if(retcode != 0){
			err("注册账号错误! err=" + KBEngineApp.app.serverErr(retcode));
			return;
		}
		if(KBEngineApp.validEmail(stringAccount)){
			info("注册账号成功，请激活Email!");
		}
		else{
			info("注册账号成功!");
		}
	}
	
	public void onConnectionState(bool success)
	{
		if(!success)
			err("connect(" + KBEngineApp.app.getInitArgs().ip + ":" + KBEngineApp.app.getInitArgs().port + ") is error! (连接错误)");
		else
			info("连接成功，请等候...");
	}
	
	public void onLoginFailed(UInt16 failedcode)
	{
		if(failedcode == 20){
			err("登陆失败, err=" + KBEngineApp.app.serverErr(failedcode) + ", " + System.Text.Encoding.ASCII.GetString(KBEngineApp.app.serverdatas()));
		}
		else{
			err("登陆失败, err=" + KBEngineApp.app.serverErr(failedcode));
		}
	}
	
	public void onVersionNotMatch(string verInfo, string serVerInfo)
	{
		err("");
	}

	public void onScriptVersionNotMatch(string verInfo, string serVerInfo)
	{
		err("");
	}
	
	public void onLoginBaseappFailed(UInt16 failedcode)	{
		err("登陆网关失败, err=" + KBEngineApp.app.serverErr(failedcode));
	}
	
	public void onLoginBaseapp()
	{
		info("连接到网关, 请稍后...");
	}

	public void onReloginBaseappFailed(UInt16 failedcode)
	{
		err("重连网关失败, err=" + KBEngineApp.app.serverErr(failedcode));
		startRelogin = false;
	}
	
	public void onReloginBaseappSuccessfully()
	{
		info("重连成功!");
		startRelogin = false;
	}
	
	public void onLoginSuccessfully(UInt64 rndUUID, Int32 eid, Account accountEntity)
	{
		info("登陆成功!");
		PlayerPrefs.SetInt("ui_state", 1);
		SceneManager.LoadScene("selavatars");
	}

	public void onKicked(UInt16 failedcode)
	{
		err("kick, disconnect!, reason=" + KBEngineApp.app.serverErr(failedcode));
		PlayerPrefs.SetInt("ui_state", 0);
		SceneManager.LoadScene("login");
	}

	public void Loginapp_importClientMessages()
	{
		info("Loginapp_importClientMessages ...");
	}

	public void Baseapp_importClientMessages()
	{
		info("Baseapp_importClientMessages ...");
	}
	
	public void Baseapp_importClientEntityDef()
	{
		info("importClientEntityDef ...");
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
}
