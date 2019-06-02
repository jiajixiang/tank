using UnityEngine;
using UnityEngine.UI;
using System;
namespace Complete
{
    public class TankShooting : MonoBehaviour
    {
        public Rigidbody m_Shell;                   // shell的预制体.
        public Transform m_FireTransform;           //产生shell的FireTransform
        public Slider m_AimSlider;                  // 瞄准滑块，注意，就是shell射程的长度。这里需要改变其长度
        public AudioSource m_ShootingAudio;         // 播放哪种音频 m_ChargingClip / m_FireClip
        public AudioClip m_ChargingClip;            // 蓄力音频
        public AudioClip m_FireClip;                // 发射音频
        public float m_MinLaunchForce = 15f;
        public float m_MaxLaunchForce = 30f;
        public float m_MaxChargeTime = 0.75f;       //在以最大力量发射之前，shell可以蓄力多长时间。


        private string m_FireButton;
        private float m_CurrentLaunchForce;         // 释放开火按钮时将给予shell的力。
        private float m_ChargeSpeed;                // 根据最大充电时间，发射力增加的速度。
        private bool m_Fired;                       // 锁

        private void OnEnable()
        {
            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;
        }

        private void Start ()
        {
            m_FireButton = "Fire";
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
        }

        public void FireStart()
        {
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;
            // 视音频重置，并开始播放
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play ();
        }
        private void FireRunning()
        {
            // 更新滑块，增加发射速度
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            m_AimSlider.value = m_CurrentLaunchForce;
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)  
            {
                m_CurrentLaunchForce = m_MaxLaunchForce;
                FireEnd();
            }
        }
        public void FireEnd()
        {
            if(!m_Fired)
            {
                Fire();
            }
        }
        private void Update ()
        {
            m_AimSlider.value = m_MinLaunchForce;
            // 当某个按钮 / 鼠标 被按下的那一帧返回true              || Input.GetMouseButtonDown (0)当按下鼠标的一瞬间返回true
            if (Input.GetButtonDown (m_FireButton)) 
            {
                FireStart();
            }
             // 当某个按钮 / 鼠标 持续按下，持续返回true        || Input.GetMouseButton (0) )
            else if (Input.GetButton (m_FireButton) && !m_Fired)   
            {
                // 更新滑块，增加发射速度
                FireRunning();
            }
             // 当松开某个按钮 / 鼠标 的那一帧返回true , 启动 shell || Input.GetMouseButtonUp (0)
            else if (Input.GetButtonUp (m_FireButton)  && !m_Fired) 
            {
                FireEnd();
            }
            if(!m_Fired){
                FireRunning();
            }
        }

        private void Fire ()
        {
            KBEngine.Event.fireIn("Fire", (Int32)m_CurrentLaunchForce);
            //设置已触发的标志，以便只调用一次Fire。
            m_Fired = true;
            //创建shell的实例并存储对其刚体的引用。
            Rigidbody shellInstance =
                Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
            //将炮弹的速度 = 发射力大小 * 方向
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;
            shellInstance.GetComponent<ShellExplosion>().isPlayer = true;
             // 将音频改为发射的音频并且播放
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play ();
            // 重置发射力。 这是在缺少按钮事件的情况下的预防措施。
            m_CurrentLaunchForce = m_MinLaunchForce;
        }
    }
}