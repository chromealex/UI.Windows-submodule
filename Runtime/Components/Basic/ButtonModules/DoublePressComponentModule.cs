using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows
{

    public class DoublePressComponentModule : ButtonComponentModule, IPointerDownHandler
    {

        private bool isPressed;
        private float pressTimer;

        public float pressTime = 1f;
        public Action onDoublePressed;

        public void LateUpdate()
        {

            if (isPressed && Time.realtimeSinceStartup - pressTimer > pressTime)
            {
                isPressed = false;
            }

        }

        public void OnPointerDown(PointerEventData eventData)
        {

            if (isPressed == false)
            {
                isPressed = true;

                pressTimer = Time.realtimeSinceStartup;
            }
            else
            {
                if (Time.realtimeSinceStartup - pressTimer <= pressTime)
                {
                    this.onDoublePressed?.Invoke();
                    isPressed = false;
                }
            }

        }

    }

}