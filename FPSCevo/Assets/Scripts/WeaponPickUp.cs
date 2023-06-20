using FPSCevo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FPSCevo
{
    public class WeaponPickUp : MonoBehaviourPunCallbacks
    {
        Transform rotatingWeapon;
        public GameObject weaponToPickup;
        private GameObject currentWeapon;

        // Start is called before the first frame update
        void Start()
        {
            rotatingWeapon = transform.Find("Anchor");
        }

        // Update is called once per frame
        void Update()
        {
            rotatingWeapon.transform.Rotate(0, 3, 0, Space.World);
        }

        [PunRPC]
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "LocalPlayer")
            {
               Destroy(gameObject);
               photonView.RPC("Equip", RpcTarget.All, 1);
            }
        }
    }
}

