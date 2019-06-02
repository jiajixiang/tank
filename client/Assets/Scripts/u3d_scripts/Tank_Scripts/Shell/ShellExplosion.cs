using UnityEngine;
using System;

namespace Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask m_TankMask;                        // 用于过滤爆炸影响的内容，应将其设置为 Player。
        public ParticleSystem m_ExplosionParticles;         //爆炸时播放的粒子特效
        public AudioSource m_ExplosionAudio;                // 爆炸时播放的音频
        public float m_MaxDamage = 100f;                    // 爆炸以坦克为中心的伤害量
        public float m_ExplosionForce = 1000f;              // 在爆炸中心的爆炸力量。
        public float m_MaxLifeTime = 2f;                    // 最大存在时间
        public float m_ExplosionRadius = 5f;                // 爆炸半径
        public bool isPlayer = false;
        private void Start ()
        {
            // 如果没有被破坏， 在其MaxLifeTime后破坏
            Destroy(gameObject, m_MaxLifeTime);
        }

        private void OnTriggerEnter (Collider other)
        {
			// 从 炮弹的当前位置收集爆炸半径球体中的所有物理碰撞体
            Collider[] colliders = Physics.OverlapSphere (transform.position, m_ExplosionRadius, m_TankMask);
            for (int i = 0; i < colliders.Length; i++)
            {
                // 找到刚体， 如果没有，下一个
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody> ();
                if (!targetRigidbody)
                    continue;

                //爆炸对其添加推动力量，这是物理引擎上的操作
                targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius,1.0f);

                // 找到与刚体关联的TankHealth脚本。如果没有，下一个
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
                if (!targetHealth)
                    continue;
                Int32 receiverID = targetRigidbody.GetComponent<GameEntity>().id;
                // 计算伤害
                float damage = CalculateDamage(targetRigidbody.position);
                Debug.Log(" OnTriggerEnter  name = " + colliders[i].name + " i = " + i + " receiverID = " + receiverID + " damage = " + damage);
                Debug.Log(" ------- isPlayer ------" + isPlayer);
                if(isPlayer == true){
                    KBEngine.Event.fireIn("takeDamage", receiverID, (Int32)damage);
                }
            }
            //从shell中取消颗粒特效。
            m_ExplosionParticles.transform.parent = null;

            // 播放爆炸的粒子特效，音频， 粒子特效每次只播一个，没播玩的算了。
            m_ExplosionParticles.Play();
            m_ExplosionAudio.Play();

            // 如果粒子特效播完了，销毁粒子特效所在的GameObject,然后消耗整个shell
            ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
            Destroy (m_ExplosionParticles.gameObject, mainModule.duration);
            Destroy (gameObject);
        }

        // 计算伤害
        private float CalculateDamage (Vector3 targetPosition)
        {
            //创建从shell到目标的向量，计算从shell到目标的距离。根据爆炸半径和爆炸中心伤害，从而按shell到目标的距离:爆炸半径的比例计算出伤害
            Vector3 explosionToTarget = targetPosition - transform.position;
            float explosionDistance = explosionToTarget.magnitude;
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;
            float damage = relativeDistance * m_MaxDamage;

            //确保最小伤害始终为0。
            damage = Mathf.Max (0f, damage);
            return damage;
        }
    }
}