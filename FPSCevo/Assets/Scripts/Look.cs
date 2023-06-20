using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FPSCevo
{
    public class Look : MonoBehaviourPunCallbacks
    {
        #region Değişkenler
        public static bool cursorLock = true;
        [SerializeField] Transform player;
        [SerializeField] Transform normalCam;
        [SerializeField] Transform weaponCam;
        [SerializeField] Transform weapon;

        [SerializeField] float xSensitivity;
        [SerializeField] float ySensitivity;
        [SerializeField] float maxAngle;

        private float sideRecoil;
        private float upRecoil;
        public Vector3 recoilShow;

        private Quaternion camCenter;
        #endregion

        #region MonoBehaviorCallbacks
        void Start()
        {
            camCenter = normalCam.localRotation;
        }

        void Update()
        {
            if (!photonView.IsMine) return;
            if (Pause.paused) return;

            SetY();
            SetX();

            UpdateCursorLock();

            weaponCam.rotation = normalCam.rotation;
        }
#endregion

        #region Özel Metodlar
        void SetY()
        {
            float t_yLook = sideRecoil + Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
            sideRecoil = 0;
            Quaternion t_adj = Quaternion.AngleAxis(t_yLook, -Vector3.right);
            Quaternion t_delta = normalCam.localRotation * t_adj;
            if(Quaternion.Angle(camCenter, t_delta) < maxAngle )
            {
                normalCam.localRotation = t_delta;
                weapon.localRotation = t_delta;
            }

            weaponCam.rotation = normalCam.rotation;
            //t_yLook = Mathf.Clamp(t_yLook, -maxAngle, maxAngle);
        }

        void SetX()
        {
            float t_xLook = upRecoil + Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
            upRecoil = 0;
            Quaternion t_adj = Quaternion.AngleAxis(t_xLook, Vector3.up);
            Quaternion t_delta = player.localRotation * t_adj;
            player.localRotation = t_delta;
        }

        void UpdateCursorLock()
        {
            if(cursorLock)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if(Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorLock = false;
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorLock = true;
                }
            }
        }

        public void AddRecoil(float _upRecoil, float _sideRecoil)
        {
            upRecoil += _upRecoil;
            sideRecoil += _sideRecoil;
            //recoilShow()
        }
        #endregion
    }
}
