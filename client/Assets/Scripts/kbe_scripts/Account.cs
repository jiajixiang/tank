namespace KBEngine
{
  using UnityEngine;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	
  public class Account : AccountBase 
  {
		public Dictionary<UInt64, AVATAR_INFOS> avatars = new Dictionary<UInt64, AVATAR_INFOS>();
		
		public Account() : base()
		{
		}
		
		public override void __init__()
		{
			// 注册事件
			Event.registerIn("reqCreateAvatar", this, "reqCreateAvatar");
			Event.registerIn("reqRemoveAvatar", this, "reqRemoveAvatar");
			Event.registerIn("selectAvatarGame", this, "selectAvatarGame");

			// 触发登陆成功事件
			Event.fireOut("onLoginSuccessfully", new object[]{KBEngineApp.app.entity_uuid, id, this});
			
			// 向服务端请求获得角色列表
			baseEntityCall.reqAvatarList();
		}

		public override void onDestroy ()
		{
			KBEngine.Event.deregisterIn(this);
		}
		
		public override void onCreateAvatarResult(Byte retcode, AVATAR_INFOS info)
		{
			if(retcode == 0)
			{
				avatars.Add(info.dbid, info);
				Dbg.DEBUG_MSG("Account::onCreateAvatarResult: name=" + info.name);
			}
			else
			{
				Dbg.ERROR_MSG("Account::onCreateAvatarResult: retcode=" + retcode);
			}

			// ui event
			Event.fireOut("onCreateAvatarResult", new object[]{retcode, info, avatars});
		}
		
		public override void onRemoveAvatar(UInt64 dbid)
		{
			Dbg.DEBUG_MSG("Account::onRemoveAvatar: dbid=" + dbid);
			
			avatars.Remove(dbid);

			// ui event
			Event.fireOut("onRemoveAvatar", new object[]{dbid, avatars});
		}
		
		public override void onReqAvatarList(AVATAR_INFOS_LIST infos)
		{
			avatars.Clear();
				
			Dbg.DEBUG_MSG("Account::onReqAvatarList: avatarsize=" + infos.values.Count);
			for(int i=0; i< infos.values.Count; i++)
			{
				AVATAR_INFOS info = infos.values[i];
				Dbg.DEBUG_MSG("Account::onReqAvatarList: name" + i + "=" + info.name);
				avatars.Add(info.dbid, info);
			}
			
			// ui event
			Dictionary<UInt64, AVATAR_INFOS> avatarList = new Dictionary<UInt64, AVATAR_INFOS>(avatars);
			Event.fireOut("onReqAvatarList", new object[]{avatarList});

			if(infos.values.Count == 0)
				return;
			
			// selectAvatarGame(avatars.Keys.ToList()[0]);
		}

		public void reqCreateAvatar(Byte roleType, string name)
		{
			Dbg.DEBUG_MSG("Account::reqCreateAvatar: roleType=" + roleType);
			baseEntityCall.reqCreateAvatar(roleType, name);
		}

		public void reqRemoveAvatar(string name)
		{
			Dbg.DEBUG_MSG("Account::reqRemoveAvatar: name=" + name);
			baseEntityCall.reqRemoveAvatar(name);
		}
		
		public void selectAvatarGame(UInt64 dbid)
		{
			Dbg.DEBUG_MSG("Account::selectAvatarGame: dbid=" + dbid);
			baseEntityCall.selectAvatarGame(dbid);
		}
  }
} 
