using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCevo
{
    public class groundChecker : MonoBehaviour
    {
        Player player;
        private void Awake()
        {
            player = this.GetComponentInParent<Player>();
        }
        private void OnTriggerEnter(Collider other)
        {
            player.isGrounded = other.transform.tag != "Player";

            if(player.isGrounded && player.highestVel<-27)
            {
                player.TakeDamage(Mathf.Abs((int)player.highestVel), -1);
            }
            else if(player.isGrounded && player.highestVel < -40)
            {
                player.TakeDamage(100, -1);
            }
            player.highestVel = 0;
        }
        private void OnTriggerExit(Collider other)
        {
            player.isGrounded = false;
        }
    }
}