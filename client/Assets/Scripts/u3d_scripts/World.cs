using KBEngine;
using UnityEngine;
using System; 
using System.IO;  
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using Complete;

public class World : MonoBehaviour 
{
	private UnityEngine.GameObject terrain = null;
	public UnityEngine.GameObject terrainPerfab;
	public UnityEngine.GameObject entityPerfab;
	public UnityEngine.GameObject avatarPerfab;
    private UnityEngine.GameObject m_FlowCortrol = null;
    private UnityEngine.GameObject player = null;

    // private TankManager player = null;
    void Awake()
	{
		DontDestroyOnLoad(transform.gameObject);
	}

	void Start () 
	{
		installEvents();
		PlayerPrefs.SetInt("relive_state", 0);
	}

	void installEvents()
	{
		// in world
		KBEngine.Event.registerOut("addSpaceGeometryMapping", this, "addSpaceGeometryMapping");
		KBEngine.Event.registerOut("onEnterWorld", this, "onEnterWorld");
		KBEngine.Event.registerOut("onLeaveWorld", this, "onLeaveWorld");
		KBEngine.Event.registerOut("set_position", this, "set_position");
		KBEngine.Event.registerOut("set_direction", this, "set_direction");
		KBEngine.Event.registerOut("updatePosition", this, "updatePosition");
		KBEngine.Event.registerOut("onControlled", this, "onControlled");
		
		// in world(register by scripts)
		KBEngine.Event.registerOut("onAvatarEnterWorld", this, "onAvatarEnterWorld");
		KBEngine.Event.registerOut("set_HP", this, "set_HP");
		KBEngine.Event.registerOut("set_MP", this, "set_MP");
		KBEngine.Event.registerOut("set_HP_Max", this, "set_HP_Max");
		KBEngine.Event.registerOut("set_MP_Max", this, "set_MP_Max");
		KBEngine.Event.registerOut("set_level", this, "set_level");
		KBEngine.Event.registerOut("set_name", this, "set_entityName");
		KBEngine.Event.registerOut("set_state", this, "set_state");
		KBEngine.Event.registerOut("set_moveSpeed", this, "set_moveSpeed");
		KBEngine.Event.registerOut("set_modelScale", this, "set_modelScale");
		KBEngine.Event.registerOut("set_modelID", this, "set_modelID");
		KBEngine.Event.registerOut("recvDamage", this, "recvDamage");
		KBEngine.Event.registerOut("otherAvatarOnFire", this, "otherAvatarOnFire");
		KBEngine.Event.registerOut("onAddSkill", this, "onAddSkill");
	}

	void OnDestroy()
	{
		KBEngine.Event.deregisterOut(this);
	}
	
	// Update is called once per frame
	void Update () 
	{
		createPlayer();
	}
	
	public void addSpaceGeometryMapping(string respath)
	{
		// 这个事件可以理解为服务器通知客户端加载指定的场景资源
		// 通过服务器的api KBEngine.addSpaceGeometryMapping设置到spaceData中，进入space的玩家就会被同步spaceData里面的内容
		Debug.Log("loading scene( " + respath + " )..."); 
		//UI.inst.info("spaceID = " + KBEngineApp.app.spaceID);
		//UI.inst.info("scene = " + respath + " ), spaceID = " + KBEngineApp.app.spaceID);
		if(terrain == null)
			terrain = Instantiate(terrainPerfab) as UnityEngine.GameObject;

		if(player)
			player.GetComponent<GameEntity>().entityEnable();
	}
	void OnGUI()
	{

	}
	public void onAvatarEnterWorld(UInt64 rndUUID, Int32 eid, KBEngine.Avatar avatar)
	{
		if(!avatar.isPlayer())
		{
			return;
		}

		//UI.inst.info("loading scene...(加载场景中...)");
		Debug.Log("loading scene...(加载场景中...)");
	}

	public void createPlayer()
	{
		// 需要等场景加载完毕再显示玩家
		if (player != null)
		{
			if(terrain != null)
				player.GetComponent<GameEntity>().entityEnable();
			return;
		}
		
		if (KBEngineApp.app.entity_type != "Avatar") {
			return;
		}

		KBEngine.Avatar avatar = (KBEngine.Avatar)KBEngineApp.app.player();
		if(avatar == null)
		{
			Debug.Log("wait create(palyer)!");
			return;
		}
        avatar.position.y = 0.0f;
		player = Instantiate(avatarPerfab, new Vector3(avatar.position.x, avatar.position.y, avatar.position.z), 
		    Quaternion.Euler(new Vector3(avatar.direction.y, avatar.direction.z, avatar.direction.x))) as UnityEngine.GameObject;
		player.name = "player";
		player.GetComponent<GameEntity>().entityDisable();
        player.GetComponent<GameEntity>().isPlayer = true;
        player.GetComponent<GameEntity>().Setup(avatar.id);
        avatar.renderObj = player;

		if(m_FlowCortrol == null){
			m_FlowCortrol = UnityEngine.GameObject.Find("CameraRig");
		}
		m_FlowCortrol.GetComponent<FlowCortrol>().AddTarget(((UnityEngine.GameObject)avatar.renderObj).transform);
        // 有必要设置一下，由于该接口由Update异步调用，有可能set_position等初始化信息已经先触发了
        // 那么如果不设置renderObj的位置和方向将为0，人物会陷入地下
        set_position(avatar);
		set_direction(avatar);
        set_entityName(avatar, avatar.name);
    }
 
	public void onAddSkill(KBEngine.Entity entity)
	{
		Debug.Log("World onAddSkill");
	}
	
	public void onEnterWorld(KBEngine.Entity entity)
	{
		Debug.Log("World onEnterWorld");
		if(entity.isPlayer())
			return;
		entity.position.y =  0.0f;

		entity.renderObj = Instantiate(entityPerfab, new Vector3(entity.position.x, entity.position.y, entity.position.z), 
			Quaternion.Euler(new Vector3(entity.direction.y, entity.direction.z, entity.direction.x))) as UnityEngine.GameObject;
		((UnityEngine.GameObject)entity.renderObj).name = entity.className + "_" + entity.id;
		if(entity.className == "Avatar"){
			if(m_FlowCortrol == null){
				m_FlowCortrol = UnityEngine.GameObject.Find("CameraRig");
			}
			m_FlowCortrol.GetComponent<FlowCortrol>().AddTarget(
				((UnityEngine.GameObject)entity.renderObj).transform
			);
			((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>().Setup(entity.id);
		}
	}
	
	public void onLeaveWorld(KBEngine.Entity entity)
	{
		if(entity.renderObj == null)
			return;

		UnityEngine.GameObject.Destroy((UnityEngine.GameObject)entity.renderObj);
		entity.renderObj = null;
	}

	public void set_position(KBEngine.Entity entity)
	{
		// if(entity.className != "Monster")
		// Debug.Log("World set_position "
        //     + "  entity.id = " + entity.id + " postion = " + entity.position);
		if(entity.renderObj == null)
			return;

		GameEntity gameEntity = ((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>();
		gameEntity.destPosition = entity.position;
		gameEntity.position = entity.position;
		gameEntity.spaceID = KBEngineApp.app.spaceID;
	}

	public void updatePosition(KBEngine.Entity entity)
	{
		// if(entity.className != "Monster")
		// Debug.Log("World updatePosition entity.className = " + entity.className
        //     + " entity.id = " + entity.id + " postion = " + entity.position);
		if(entity.renderObj == null)
			return;
		
		GameEntity gameEntity = ((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>();
		gameEntity.destPosition = entity.position;
		gameEntity.isOnGround = entity.isOnGround;
		gameEntity.spaceID = KBEngineApp.app.spaceID;
	}
	
	public void onControlled(KBEngine.Entity entity, bool isControlled)
	{
		if(entity.className != "Monster")
		Debug.Log("World onControlled entity.className = " + entity.className
            + " entity.id = " + entity.id + " isControlled = " + isControlled);
		if(entity.renderObj == null)
			return;
		
		GameEntity gameEntity = ((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>();
		gameEntity.isControlled = isControlled;
	}
	
	public void set_direction(KBEngine.Entity entity)
	{
		if(entity.className != "Monster")

		if(entity.renderObj == null)
			return;

		GameEntity gameEntity = ((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>();
		Vector3 direction = new Vector3(0.0f, entity.direction.z, 0.0f);
		gameEntity.destDirection = direction; 
		gameEntity.spaceID = KBEngineApp.app.spaceID;
	}

	public void set_HP(KBEngine.Entity entity, Int32 v, Int32 HP_Max)
	{
		Dbg.DEBUG_MSG(entity.className + "::set_HP: " + v + " => " + HP_Max); 
		if(entity.renderObj != null)
		{
			((UnityEngine.GameObject)entity.renderObj).GetComponent<TankHealth>().SetHealth((float)v, (float)HP_Max);
		}
	}
	
	public void set_MP(KBEngine.Entity entity, Int32 v, Int32 MP_Max)
	{

	}
	
	public void set_HP_Max(KBEngine.Entity entity, Int32 v, Int32 HP)
	{
		// if(entity.renderObj != null)
		// {
		// 	((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>().hp = HP + "/" + v;
		// }
	}
	
	public void set_MP_Max(KBEngine.Entity entity, Int32 v, Int32 MP)
	{

	}
	
	public void set_level(KBEngine.Entity entity, UInt16 v)
	{

	}
	public void set_entityName(KBEngine.Entity entity, string v)
	{
		if(entity.className != "Monster")
		Debug.Log("World set_entityName entity.className = " + entity.className
            + " entity.id = " + entity.id + " v= "+ v);
		if(entity.renderObj != null)
		{
			((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>().entity_name = v;
		}
	}
	
	public void set_state(KBEngine.Entity entity, SByte v)
	{
		Debug.Log("World set_state entity.className = " + entity.className
            + " entity.id = " + entity.id + " v= " + v);
        if (entity.renderObj != null)
		{
			((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>().set_state(v);
        }
        else
        {
            Debug.Log("World set_state entity.renderObj != null = ");
        }
		
		if(entity.isPlayer())
		{
			PlayerPrefs.SetInt("player_state", v);
			return;
		}
	}

	public void set_moveSpeed(KBEngine.Entity entity, Byte v)
	{
		if(entity.className != "Monster")
		Debug.Log("World set_moveSpeed  entity.className = " + entity.className
            + " entity.id = " + entity.id + " v= " + v);
        float fspeed = ((float)(Byte)v) / 10f;
		
		if(entity.renderObj != null)
		{
			((UnityEngine.GameObject)entity.renderObj).GetComponent<GameEntity>().speed = fspeed;
		}
	}
	
	public void set_modelScale(KBEngine.Entity entity, Byte v)
	{
		
	}
	
	public void set_modelID(KBEngine.Entity entity, UInt32 v)
	{

	}
	
	public void recvDamage(KBEngine.Entity entity, KBEngine.Entity attacker, Int32 skillID, Int32 damageType, Int32 damage)
	{

	}
	
	public void otherAvatarOnFire(KBEngine.Entity entity,Int32 value)
	{
		Debug.Log("World otherAvatarOnFire  entity.className = " + entity.className
            + " entity.id = " + entity.id);
		if(entity.renderObj != null)
		{
			((UnityEngine.GameObject)entity.renderObj).GetComponent<EnityTankShooting>().onFire((float)value);
		}
	}
}
