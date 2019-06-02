using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

namespace Complete
{
	public class TankShootingUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public SimpleInput.ButtonInput button = new SimpleInput.ButtonInput();
	    private TankShooting m_Shooting;  

        private void Start()
        {
            UnityEngine.GameObject player = UnityEngine.GameObject.Find("player");
		    m_Shooting = player.GetComponent<TankShooting> ();
        }
		private void Awake()
		{
			Graphic graphic = GetComponent<Graphic>();
			if( graphic != null )
				graphic.raycastTarget = true;
		}

        private void OnEnable()
        {
            button.StartTracking();
        }

        private void OnDisable()
        {
            button.StopTracking();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            m_Shooting.FireStart();
            button.value = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_Shooting.FireEnd();
            button.value = false;
        }
    }
}