using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

namespace FPSCevo
{
    public class BuildController : MonoBehaviour
    {
        public GameObject foundation;
        public GameObject foundationPreview;
        public Camera BuildCamera;
        public LayerMask canBuild;

        Transform socket;

        private void Update()
        {
            /*Transform hit_spawn = transform.Find("Cameras/Normal Camera");
            Vector3 direction = hit_spawn.position + hit_spawn.forward * 1000f;
            RaycastHit hit = new RaycastHit();
            

            if (Physics.Raycast(hit_spawn.position, direction, out hit, Mathf.Infinity, canBuild))
            {
                if (hit.transform.tag == "Socket")
                {
                    socket = hit.transform;
                    foundationPreview.transform.position = socket.transform.position;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        GameObject spawnFoundation = Instantiate(foundation, socket.transform.position, Quaternion.identity);
                    }
                }
                else
                {
                    foundationPreview.transform.position = hit.point;
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        GameObject spawnFoundation = Instantiate(foundation, hit.point, Quaternion.identity);
                    }
                }
            }*/

            RaycastHit hit;
            Ray ray = BuildCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                if (hit.transform.tag == "Socket")
                {
                    socket = hit.transform;
                    foundationPreview.transform.position = socket.transform.position;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        GameObject spawnFoundation = Instantiate(foundation, socket.transform.position, Quaternion.identity);
                    }
                }
                else
                {
                    foundationPreview.transform.position = hit.point;
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        GameObject spawnFoundation = Instantiate(foundation, hit.point, Quaternion.identity);
                    }
                }
            }
        }
    }
}

