using UnityEngine;

namespace Complete
{
	public class TankCamera : MonoBehaviour {
        public float roate_Speed = 100.0f;
		public int m_PlayerNumber = 1;
        public GameObject m_TankTurret; // 炮台
        private Vector2 m_MousePositionLast;//用于计算移动方向的起始位置
		private Vector2 m_MousePositionNew;//用于计算移动方向的结尾位置
		private Vector2 m_MouseMoveDirection;//用于表示移动方向
        private void Awake () {
        }
        private void Start () {
        }
        private void Update () {
			m_MousePositionLast = m_MousePositionNew;
			m_MousePositionNew = Input.mousePosition;
			m_MouseMoveDirection = m_MousePositionNew - m_MousePositionLast;
			
			transform.Rotate(0, m_MouseMoveDirection.x * 0.5f, 0);
            m_TankTurret.transform.Rotate(0, m_MouseMoveDirection.x * 0.5f, 0);
        }
    }
}