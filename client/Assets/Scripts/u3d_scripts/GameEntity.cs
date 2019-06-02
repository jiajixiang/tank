using UnityEngine;
using UnityEngine.UI;
using KBEngine;
using System;
using System.Xml;
using System.Collections.Generic;
using Complete;

public class GameEntity : MonoBehaviour 
{
	public bool isPlayer = false;
	
	private Vector3 _position = Vector3.zero;
	private Vector3 _eulerAngles = Vector3.zero;
	private Vector3 _scale = Vector3.zero;
	private UInt32 _spaceID = 0;
	public Int32 id;
	public Vector3 destPosition = Vector3.zero;
	public Vector3 destDirection = Vector3.zero;
	
	private float _speed = 0f;
	private float currY = 1.0f;
	
	public string entity_name;
	float npcHeight = 3.0f;
	public bool isOnGround = true; // 在地面
	public bool isControlled = false; // 是受控制的
	public bool entityEnabled = true; // 实体已启用
	private Camera[] m_CameraList;
	private Camera playerCamera = null;         
    private TankHealth m_Health;
	private GUIStyle style;
	void Start(){
		style = new GUIStyle ();
		style.normal.background = null; //这是设置背景填充的
		style.normal.textColor = Color.red; //设置字体颜色的
		style.fontSize = 50;
	}
	public void Setup(Int32 entityID)
	{
        m_Health = GetComponent<TankHealth> ();
		id = entityID;
    }

	void OnGUI()
	{
        Vector3 worldPosition = new Vector3 (transform.position.x , transform.position.y + npcHeight, transform.position.z);
		if(playerCamera == null){
        	m_CameraList = Camera.allCameras;
			playerCamera = m_CameraList[0];
		}
		Vector2 uiposition = playerCamera.WorldToScreenPoint(worldPosition);
		//得到真实NPC头顶的2D坐标
		uiposition = new Vector2 (uiposition.x, Screen.height - uiposition.y);
		Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(entity_name));
		GUI.Label(new Rect(uiposition.x - nameSize.x * 2, uiposition.y - nameSize.y - 5.0f, nameSize.x, nameSize.y), entity_name, style);
	}
    public Vector3 position {
			get
			{
				return _position;
			}

			set
			{
				_position = value;
				
				if(gameObject != null)
					gameObject.transform.position = _position;
			}    
    }  
  
    public Vector3 eulerAngles {  
			get
			{
				return _eulerAngles;
			}

			set
			{
				_eulerAngles = value;
				
				if(gameObject != null)
				{
					gameObject.transform.eulerAngles = _eulerAngles;
				}
			}
    }  

    public Quaternion rotation {  
			get
			{
				return Quaternion.Euler(_eulerAngles);
			}

			set
			{
				eulerAngles = value.eulerAngles;
			}    
    }  
    
    public Vector3 scale {  
			get
			{
				return _scale;
			}

			set
			{
				_scale = value;
				
				if(gameObject != null)
					gameObject.transform.localScale = _scale;
			}    
    } 

    public float speed {  
			get
			{
				return _speed;
			}

			set
			{
				_speed = value;
			}    
    } 

    public UInt32 spaceID {  
			get
			{
				return _spaceID;
			}

			set
			{
				_spaceID = value;
			}    
    } 
    
	public void entityEnable()
	{
		entityEnabled = true;
	}

	public void entityDisable()
	{
		entityEnabled = false;
	}

	public void set_state(sbyte v)
	{
        Debug.Log(" GameEntity set_state  state = " + v);
		if (v == 3)
		{

		} 
		else if (v == 0) 
		{
            gameObject.SetActive(true);
		} 
		else if (v == 1)
		{
			m_Health.OnDeath();
            gameObject.SetActive(false);
		}
	}
    void FixedUpdate () 
    {
		if (!entityEnabled || KBEngineApp.app == null)
			return;
			
		if(isPlayer == isControlled)
			return;

        KBEngine.Event.fireIn("updatePlayer", spaceID,
            gameObject.transform.position.x,
            gameObject.transform.position.y,
            gameObject.transform.position.z,
            gameObject.transform.rotation.eulerAngles.y
        );
    }

	void Update ()
	{
		if (!entityEnabled) 
		{
			position = destPosition;
			return;
		}

		float deltaSpeed = (speed * Time.deltaTime);
		
		if(isPlayer == true && isControlled == false)
		{
			return;
		}
		
		if(Vector3.Distance(eulerAngles, destDirection) > 0.0004f)
		{
			rotation = Quaternion.Slerp(rotation, Quaternion.Euler(destDirection), 8f * Time.deltaTime);
		}

		float dist = 0.0f;
		// 如果isOnGround为true，服务端同步其他实体到客户端时为了节省流量并不同步y轴，客户端需要强制将实体贴在地面上
		// 由于这里的地面位置就是0，所以直接填入0，如果是通过navmesh不规则地表高度寻路则需要想办法得到地面位置
		if(isOnGround)
		{
			dist = Vector3.Distance(new Vector3(destPosition.x, 0f, destPosition.z), 
				new Vector3(position.x, 0f, position.z));
		}
		else
		{
			dist = Vector3.Distance(destPosition, position);
		}
		if(dist > 0.01f)
		{
			Vector3 pos = position;

			Vector3 movement = destPosition - pos;
			movement.y = 0f;
			movement.Normalize();
			
			movement *= deltaSpeed;
			
			if(dist > deltaSpeed || movement.magnitude > deltaSpeed)
				pos += movement;
			else
				pos = destPosition;
			
			if(isOnGround)
				pos.y = currY;
			
			position = pos;
		}
		else
		{
			position = destPosition;
		}

	}
	public void Reset ()
    {
        gameObject.SetActive(false);
		gameObject.SetActive(true);
    }
}