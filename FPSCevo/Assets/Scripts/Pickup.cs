using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FPSCevo
{
    public class Pickup : MonoBehaviourPunCallbacks
    {
        public Gun weapon;
        public float cooldown;
        public GameObject gunDisplay;
        public List<GameObject> targets;
        private bool isDisabled;
        public float wait;

        private void Start()
        {
            foreach (Transform t in gunDisplay.transform) Destroy(t.gameObject);
            GameObject newDisplay = Instantiate(weapon.display, gunDisplay.transform.position, gunDisplay.transform.rotation) as GameObject;
            newDisplay.transform.SetParent(gunDisplay.transform);
        }

        private void Update()
        {
            if(isDisabled)
            {
                if (wait > 0)
                {
                    wait -= Time.deltaTime;
                }
                else
                {
                    //tekrar aktif et
                    Enable();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null) return;
            if(other.attachedRigidbody.gameObject.tag.Equals("Player"))
            {
                Weapon weaponController = other.attachedRigidbody.gameObject.GetComponent<Weapon>();
                weaponController.photonView.RPC("PickUpWeapon", RpcTarget.All, weapon.name);
                photonView.RPC("Disable", RpcTarget.All);
                weapon.Initialize();
            }
        }

        [PunRPC]
        public void Disable()
        {
            isDisabled = true;
            wait = cooldown;

            foreach (GameObject a in targets) a.SetActive(false);
        }

        public void Enable()
        {
            isDisabled = false;
            wait = 0;

            foreach (GameObject a in targets) a.SetActive(true);
        }
    }
}