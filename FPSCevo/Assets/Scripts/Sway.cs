using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FPSCevo
{


public class Sway : MonoBehaviourPunCallbacks
{
        #region Değişkenler

        [SerializeField] float intensity;
        [SerializeField] float smooth;
        public bool isMine;

        private Quaternion origin_rotation;

        #endregion





        #region MonoBehaviourCallbacks

        void Start()
        {
            origin_rotation = transform.localRotation;
        }

        void Update()
        {
            //photonView.RPC("UpdateSway", RpcTarget.All);
            UpdateSway();
        }

        #endregion





        #region Özel Metodlar

        
        void UpdateSway()
        {
            //Kontrolleri alma
            float t_x_mouse = Input.GetAxis("Mouse X");
            float t_y_mouse = Input.GetAxis("Mouse Y");

            if(!isMine)
            {
                t_x_mouse = 0;
                t_y_mouse = 0;
            }

            //Hedef rotasyonu hesaplama
            Quaternion t_x_adj = Quaternion.AngleAxis(-intensity * t_x_mouse, Vector3.up);
            Quaternion t_y_adj = Quaternion.AngleAxis(intensity * t_y_mouse, Vector3.right);
            Quaternion target_rotation = origin_rotation * t_x_adj * t_y_adj;

            //Hedefe rotasyon ettirme
            transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.deltaTime * smooth);
        }

        #endregion
    }

}