using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace FPSCevo
{
    public class FallDamageNew : MonoBehaviour
    {

        public float exitY;
        public float enterY;
        Player player;
        public Rigidbody rb;


        private void Awake()
        {
            player = GetComponent<Player>();
        }
        //private void OnTriggerEnter(Collider col)
        //{
        //    if (col.tag == "Zemin")
        //    {
        //        enterY = rb.velocity.y;
        //    }
        //}

        //private void OnTriggerExit(Collider col)
        //{
        //    if (col.tag == "Zemin")
        //    {
        //        exitY = rb.velocity.y;
        //    } 
        //}
    }
}

