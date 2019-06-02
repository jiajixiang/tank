using UnityEngine;
using UnityEngine.UI;
using System;

namespace Complete
{
    public class EnityTankShooting : MonoBehaviour
    {
        public Rigidbody m_Shell;                   // shell的预制体.
        public Transform m_FireTransform;           //产生shell的FireTransform
        public AudioSource m_ShootingAudio;         // 播放哪种音频 m_ChargingClip / m_FireClip
        public AudioClip m_FireClip;                // 发射音频

        public void onFire(float value)
        {
            Rigidbody shellInstance =
                Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
            //将炮弹的速度 = 发射力大小 * 方向
            shellInstance.velocity = value * m_FireTransform.forward; 
            // 将音频改为发射的音频并且播放
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play ();
        }
    }
}