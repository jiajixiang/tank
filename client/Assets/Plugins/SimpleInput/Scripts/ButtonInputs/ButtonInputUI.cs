using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleInputNamespace
{
	public class ButtonInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public SimpleInput.ButtonInput button = new SimpleInput.ButtonInput();

		private void Awake()
		{
			Graphic graphic = GetComponent<Graphic>();
			if( graphic != null )
				graphic.raycastTarget = true;
		}

        private void OnEnable()
        {
            Debug.Log("-------- OnEnable ----------");
            button.StartTracking();
        }

        private void OnDisable()
        {
            Debug.Log("-------- OnDisable ----------");
            button.StopTracking();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("-------- OnPointerDown ----------");
            button.value = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("-------- OnPointerUp ----------");
            button.value = false;
        }
    }
}