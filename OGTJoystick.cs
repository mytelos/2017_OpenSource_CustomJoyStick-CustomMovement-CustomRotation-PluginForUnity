﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityStandardAssets.CrossPlatformInput
{
    public class OGTJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public static OGTJoystick Singleton;

        public int _halfRadius = 100;
        [SerializeField]
        float SnapAngle = 10f;

        [SerializeField]
        private Transform _origin = null;

        private Vector3 _xCoordinate = new Vector3(10, 0, 0);
        private Vector3 _yCoordinate = new Vector3(0, 10, 0);

        public delegate void OnDiectionChanged(float angle);
        public OnDiectionChanged DirectionChanged;

        void DirectionChangedEvent(float angle)
        {
            if (DirectionChanged != null)
            {
                DirectionChanged.Invoke(angle);
                //Debug.Log("DirectionChanged Invoked!");
            }
        }

        void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else if (Singleton != this)
            {
                Debug.LogWarning(this.GetType().ToString() + " has more tahn one Singletons, this is not allowed, destroying the 2nd instance...");
                Destroy(this);
            }
        }

        void Update()
        {
            DrawDebugLines();
        }

        private void DrawDebugLines()
        {
            DrawCoordinateSystem();
            DrawJoystickMaximumBorders();
            //Debug.DrawLine(Vector3.zero, transform.position);
            //Debug.DrawLine(Vector3.zero, _origin.position);
            //Vector3 originToNew = transform.position - _origin.position;
            //Debug.DrawLine(Vector3.zero, originToNew);
            Debug.DrawLine(_origin.position, transform.position);
        }

        //joystickimizin sinirlarini gosteren kareyi View'a cizdiren method
        private void DrawJoystickMaximumBorders()
        {
            for (int i = 0; i < 4; i++)
            {
                //Kare kenarlarımızın koordinat noktalari
                float xOne, yOne;
                //eger ilk veya son kenar ise; x = orjin - bir kenar / 2
                if (i == 0 | i == 3)
                {
                    xOne = _origin.position.x - _halfRadius;
                }
                else
                {
                    xOne = _origin.position.x + _halfRadius;
                }
                //eger 1. ve 2. kenari cizidiriyorsak (yani index 0 ve 1)
                if (i <= 1)
                {
                    //kenarlarin baslangic noktasi ayni ve karenin ust yarisinda olacak
                    yOne = _origin.position.y + _halfRadius;
                }
                else//3. ve 4. kenari cizidiriyorsak
                {
                    //kenarlarin baslangic noktasi ayni ve karenin alt kisminda olacak
                    yOne = _origin.position.y - _halfRadius;
                }
                //cizginin ilk noktasi
                Vector3 CoordinateOne = new Vector3(xOne, yOne, 0);
                //cizginin ikinci noktasi
                Vector3 CoordinateTwo = new Vector3(CoordinateOne.x, CoordinateOne.y, 0);

                //eger i cift ise yani 0 veya 2 ise; yani 1. veya 3. kenarlar cizilmekte ise
                if (i % 2 == 0)
                {
                    //1. kenar ise toplayarak bir kenar uzunlugu ekle 
                    //2.kenar ise cikararak bir kenar uzunlugu ekle 
                    CoordinateTwo.x = xOne + ((i == 2) ? (-(_halfRadius * 2)) : (_halfRadius * 2));
                }
                else //eger tek ise
                {
                    //y'sine bir kenar uzunlugu ekle (2r)
                    //ama i == 1 ise cikararak , 3 ise toplayarak ekle
                    CoordinateTwo.y = yOne + ((i == 1) ? (-_halfRadius * 2) : (_halfRadius * 2));
                }
                Debug.DrawLine(CoordinateOne, CoordinateTwo);
            }
        }

        //Aci hesabinda kullandiğimiz koordinat sistemimizi ekrana cizdiren method
        private void DrawCoordinateSystem()
        {
            Debug.DrawLine(new Vector3(_origin.position.x - _halfRadius * 2, _origin.position.y, 0), new Vector3(_origin.position.x + _halfRadius * 2, _origin.position.y, 0));
            Debug.DrawLine(new Vector3(_origin.position.x, _origin.position.y - _halfRadius * 2, 0), new Vector3(_origin.position.x, _origin.position.y + _halfRadius * 2, 0));
        }

        public void OnDrag(PointerEventData data)
        {
            transform.position = data.position;
            float currentX = transform.position.x;
            float currentY = transform.position.y;
            //x kontrol
            if (currentX > _origin.position.x + _halfRadius)
            {
                currentX = _origin.position.x + _halfRadius;
            }
            else if (currentX < _origin.position.x - _halfRadius)
            {
                currentX = _origin.position.x - _halfRadius;
            }
            //y kontrol
            if (currentY > /*OgtMathHelper.ConvertToPositive(*/_origin.position.y + _halfRadius/*)*/)
            {

                currentY = _origin.position.y + _halfRadius;
            }
            else if (currentY < _origin.position.y - _halfRadius)
            {
                currentY = _origin.position.y - _halfRadius;
            }

            Vector3 newPos = new Vector3(currentX, currentY);

            transform.position = newPos;

            //JOYSTİCK IN YENİ POZISYNUNDAN ACI HESABİ YAPIYOR        
            float angle = CalculateJoystickAngle(newPos);
            //Debug.Log("JOYSTICK ANGLE: " + angle);
            //VE KARAKTERIN ACISINI SET EDIYORUZ

            //OGTCharacterMovement.Singleton.DirectionPointAngle = angle;
            if (angle >= SnapAngle || angle <= SnapAngle)
            {
                //Debug.Log("event fired: " + angle);
                //CharacterAngle = angle;
                DirectionChangedEvent(angle);
            }
        }

        /// <summary>
        /// joystick in acisini buluyor
        /// </summary>
        private float CalculateJoystickAngle(Vector3 joyStickVector)
        {
            Vector3 originToNew = transform.position;//transform.position - _origin.position;
            float angle = 0;

            if (_origin.position.x < originToNew.x && _origin.position.y < originToNew.y) //(+ , +)
            {
                Debug.Log("+ , +");
                angle = Vector3.Angle(_xCoordinate, originToNew);
            }
            else if (_origin.position.x > originToNew.x && _origin.position.y < originToNew.y) //(- , +)
            {
                Debug.Log("- , +");
                angle = Vector3.Angle(_yCoordinate, originToNew) + 90f;
            }
            else if (_origin.position.x > originToNew.x && _origin.position.y > originToNew.y) //(- , -)
            {
                Debug.Log("- , -");
                angle = Vector3.Angle(_xCoordinate, originToNew) + 180f;
            }
            else if (_origin.position.x < originToNew.x && _origin.position.y < originToNew.x) //(+ , -)
            {
                Debug.Log("+ , -");
                angle = Vector3.Angle(_yCoordinate, originToNew) + 270f;
            }
            else
            {
                angle = 0;
            }
            //Debug.Log("angle" + angle);
            return angle;
            //float angleX = Vector3.Angle(_xCoordinate, originToNew);
            //float angleY = Vector3.Angle(_yCoordinate, originToNew);
            //Debug.Log("XAXIS: " + angleX);
            ////joystick Y-Ekseninin saginda ise direkt Y-Ekseni ile yaptigi aciyi dondur
            //angle = angleY;
            ////joystikc eger Y-Ekseninin sol tarafinda ise (yani X ekseni ile yaptigi aci 90-180 arasindaysa)
            //if (90 < angleX && angleX < 180)
            //{
            //    angle = 360 - angleY;
            //}
            ////Debug.Log("JOYSTICK-YAXIS: " + angle + " ANGLE");
            //return angle;
        }

        public void OnPointerUp(PointerEventData data)
        {
            _isDown = false;
            transform.position = _origin.position;

            //OGTCharacterMovement.Singleton.DirectionPointAngle = 0f;
            //CharacterAngle = 0f;
            //Debug.Log(OGTCharacterMovement.Singleton.DirectionPointAngle);
        }

        public static bool _isDown = false;
        public void OnPointerDown(PointerEventData data)
        {
            _isDown = true;
        }
    }
}