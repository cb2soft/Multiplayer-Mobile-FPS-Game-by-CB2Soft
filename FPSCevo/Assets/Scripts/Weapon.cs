using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Animations;
using Photon.Pun;
using UnityEditor;
using ExitGames.Client.Photon.StructWrapping;

namespace FPSCevo
{
    public class Weapon : MonoBehaviourPunCallbacks
    {
        #region Değişkenler

        [HideInInspector] public Gun currentGunData;
        public List<Gun> loadout;
        [SerializeField] Player player;
        //public GameObject[] loadout;
        public Transform weaponParent;
        public GameObject bulletHolePrefab;
        public LayerMask canBeShot;
        public AudioSource sfx;
        public AudioClip hitmarkerSound;
        public int currentIndex;
        private float currentCooldown; //reload cooldown
        private GameObject currentWeapon;
        private Image hitmarkerImage;
        private float hitmarker_wait;
        public bool isAaiming = false;
        //public New_Weapon_Recoil_Script recoil;
        public bool isShooting;
        public Look look;
        public float RecoilControlindex;
        private Color ClearWhite = new Color(1, 1, 1, 0);

        //[SerializeField] Camera normalCam;
        //[SerializeField] Camera weaponCamera;

        //private Quaternion originNormalCam;
        //private Quaternion originWeaponCam;

        private bool isReloading;
        #endregion

        #region MonoBehaviourCallbacks

        void Start()
        {
            if(photonView.IsMine)
            {
                foreach (Gun a in loadout) a.Initialize();
            }
            hitmarkerImage = GameObject.Find("HUD/HitMarker/Image").GetComponent<Image>();
            hitmarkerImage.color = ClearWhite;
            Equip(0);
            //originNormalCam = normalCam.transform.localRotation;
            //originWeaponCam = normalCam.transform.localRotation;
        }

        void Update()
        {

            //recoil.Fire();
            if (Pause.paused && photonView.IsMine) return;

            isShooting = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) && !isReloading;

            if(!isShooting)
            {
                //StartCoroutine(RecoilRefreshTime(loadout[currentIndex].RecoilRefreshTime));
                RecoilControlindex = 0;
            }


            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
            {
                photonView.RPC("Equip", RpcTarget.All, 0);
            } //RpcTarget.All veriyi herkese gönderiyor All bölümü değiştirilebilir.
            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
            {
                photonView.RPC("Equip", RpcTarget.All, 1);
            }
            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha3))
            {
                photonView.RPC("Equip", RpcTarget.All, 2);
            }
            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha4))
            {
                photonView.RPC("Equip", RpcTarget.All, 3);
            }

            if (currentWeapon != null)
            {

                if(photonView.IsMine)
                {
                    //Aim(Input.GetMouseButton(1));

                    if(loadout[currentIndex].burst != 1)
                    {
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
                        {
                            if (!isReloading) //reload yapılıyorsa firebullet çalışmıyor. Böylece mermi bugu ortadan kalktı.
                            {
                                if (loadout[currentIndex].FireBullet())
                                    photonView.RPC("Shoot", RpcTarget.All);
                                else //StartCoroutine(Reload(loadout[currentIndex].reloadSpeed));
                                    photonView.RPC("ReloadRPC", RpcTarget.All);
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButton(0) && currentCooldown <= 0)
                        {
                            if (!isReloading) //reload yapılıyorsa firebullet çalışmıyor. Böylece mermi bugu ortadan kalktı.
                            {
                                if (loadout[currentIndex].FireBullet())
                                    photonView.RPC("Shoot", RpcTarget.All);
                                else //StartCoroutine(Reload(loadout[currentIndex].reloadSpeed));
                                    photonView.RPC("ReloadRPC", RpcTarget.All);
                            }
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        photonView.RPC("ReloadRPC", RpcTarget.All);
                        //StartCoroutine(Reload(loadout[currentIndex].reloadSpeed));
                    }
                    //bekleme süresi 
                    if (currentCooldown > 0) currentCooldown -= Time.deltaTime;

                }

                //silah pozisyonnu esnetme
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
            }

            //hitmarker
            if(photonView.IsMine)
            {
                if(hitmarker_wait > 0)
                {
                    hitmarker_wait -= Time.deltaTime;
                }
                else
                {
                    hitmarkerImage.color = Color.Lerp(hitmarkerImage.color, ClearWhite, Time.deltaTime * 2f);
                }
            }
        }
        #endregion

        #region Özel Metodlar

        [PunRPC]
        void Equip(int p_ind)
        {
            if (currentWeapon != null)
            {
                if(isReloading) StopCoroutine("Reload");
                Destroy(currentWeapon);
            }
            currentIndex = p_ind;
            GameObject t_newEquipment = Instantiate(loadout[p_ind].prefab,weaponParent.position,weaponParent.rotation, weaponParent) as GameObject;
            t_newEquipment.transform.localPosition = Vector3.zero;
            t_newEquipment.transform.localEulerAngles = Vector3.zero;

            if (photonView.IsMine) ChangeLayersRecursively(t_newEquipment, 12);
            else ChangeLayersRecursively(t_newEquipment, 0);

            t_newEquipment.GetComponent<Animator>().Play("PistolEquip", 0, 0);
            RecoilControlindex = 0;

            t_newEquipment.GetComponent<Sway>().isMine = photonView.IsMine; //Sway yalnızca bizde çalışacak uzak oyuncuda çalışmıycak
            //photonview.Ismine || !photonView.Ismine yapılırsa sway iki taraftada çalışır
            currentWeapon = t_newEquipment;
            currentGunData = loadout[p_ind];
        }

        [PunRPC]
        void PickUpWeapon(string name)
        {
            //silahı kütüphaneden bul
            //silahı loadouta ekle
            Gun newWeapon = GunLibrary.FindGun(name);
            if(loadout.Count >= 2)
            {
                //tuttuğumuz silahı yenile
                loadout[currentIndex] = newWeapon;
                Equip(currentIndex);
            }
            else
            {
                loadout.Add(newWeapon);
                    Equip(loadout.Count -1);
            }
        }

        private void ChangeLayersRecursively (GameObject p_target, int p_layer)
        {
            p_target.layer = p_layer;
            foreach (Transform a in p_target.transform) ChangeLayersRecursively(a.gameObject, p_layer);
        }

        public bool Aim(bool p_isAiming)
        {
            if (!currentWeapon) return false;
            if (isReloading) p_isAiming = false;

            isAaiming = p_isAiming;
            Transform t_anchor = currentWeapon.transform.Find("Anchor");
            Transform t_state_ads = currentWeapon.transform.Find("States/ADS/AnchorADS");
            Transform t_state_hip = currentWeapon.transform.Find("States/Hip");

            if (p_isAiming)
            {
                //aim
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }
            else
            {
                //hip
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }

            return p_isAiming;
        }

        [PunRPC]
        void Shoot()
        { 
            Transform t_spawn = transform.Find("Cameras/Normal Camera");


            //bekleme süresi atış hızı
            currentCooldown = loadout[currentIndex].fireRate;
            RecoilControlindex += 1;

            for (int i = 0; i < Mathf.Max(1, currentGunData.pellets); i++)
            {
                //Bloom vector3 setup
                Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
                t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
                t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
                t_bloom -= t_spawn.position;
                t_bloom.Normalize();

                //RayCast
                RaycastHit t_hit = new RaycastHit();
                if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
                {
                    GameObject t_newBulletHole = Instantiate(bulletHolePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                    t_newBulletHole.transform.LookAt(t_hit.point + t_hit.normal * 1f);
                    Destroy(t_newBulletHole, 5f);
                    //oyuncu bizsek
                    if (photonView.IsMine)
                    {
                        //oyuncuyu vuruyorsak
                        if (t_hit.collider.gameObject.layer == 13)
                        {
                            //hasar vurma
                            t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage2", RpcTarget.All, loadout[currentIndex].damage, PhotonNetwork.LocalPlayer.ActorNumber);
                            //hitmarker
                            hitmarkerImage.color = Color.white;
                            sfx.PlayOneShot(hitmarkerSound);
                            hitmarker_wait = 0.5f;
                        }
                    }
                }


                //if (photonView.IsMine) bunu eklersek silahın geri sekmesi yalnızca local playerde çalışır
                //Gun fx
                currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
                currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;
                //recoil.Fire();
                //weaponCamera.transform.Rotate(look.AddRecoil(2f, 2f));
                if(photonView.IsMine)
                {
                    photonView.RPC("RecoilControl", RpcTarget.All);
                }

                //Sound
                sfx.Stop();
                sfx.clip = currentGunData.gunShotSound;
                sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
                sfx.Play();

               if (currentGunData.recovery)
                    currentWeapon.GetComponent<Animator>().Play("Recovery", 0, 0);
            }

        }

        [PunRPC]
        void RecoilControl()
        {
                if (loadout[currentIndex].name == "SMG")
                {
                        if (RecoilControlindex <= 15)
                        {
                        look.AddRecoil(-loadout[currentIndex].recoilX , loadout[currentIndex].recoilY);
                        RecoilControlindex += 1 * Time.deltaTime;
                        }
                        else
                        {
                        look.AddRecoil(loadout[currentIndex].recoilX, loadout[currentIndex].recoilY);
                        RecoilControlindex += 1 * Time.deltaTime;
                        }
                }
                else if (loadout[currentIndex].name == "Assault")
                {
                        if (RecoilControlindex <= 10)
                        {
                        look.AddRecoil(-loadout[currentIndex].recoilX, loadout[currentIndex].recoilY);
                        RecoilControlindex += 1 * Time.deltaTime;
                        }
                        else if (RecoilControlindex <= 20)
                        {
                        look.AddRecoil(loadout[currentIndex].recoilX, loadout[currentIndex].recoilY);
                        RecoilControlindex += 1 * Time.deltaTime;
                        }
                        else
                        {
                        look.AddRecoil(0, loadout[currentIndex].recoilY);
                         RecoilControlindex += 1 * Time.deltaTime;
                        }
                }
                else if (loadout[currentIndex].name == "Pistol")
                {
                    look.AddRecoil(0, loadout[currentIndex].recoilY);
                }
        }

        [PunRPC]
        private void TakeDamage2(int p_damage, int p_actor)
        {
            GetComponent<Player>().TakeDamage(p_damage, p_actor);
        }

        [PunRPC]
        private void ReloadRPC()
        {
            StartCoroutine(Reload(loadout[currentIndex].reloadSpeed));
        }

        IEnumerator Reload(float p_wait)  //Reload ve reload süresi
        {
            if(photonView.IsMine)
            {
                if (loadout[currentIndex].stash != 0)
                {
                    isReloading = true;
                    //if(currentWeapon.GetComponent<Animator>())
                    if (loadout[currentIndex].prefab.name == "Pistol")
                        currentWeapon.GetComponent<Animator>().Play("PistolReload", 0, 0);
                    else
                        currentWeapon.SetActive(false);

                    yield return new WaitForSeconds(p_wait);
                    loadout[currentIndex].Reload();
                    RecoilControlindex = 0;
                    currentWeapon.SetActive(true);
                    isReloading = false;
                }
            }
        }

        IEnumerator RecoilRefreshTime(float p_wait)
        {
            yield return new WaitForSeconds(p_wait);
        }

        public void RefreshAmmo(TMP_Text p_text)
        {
            if(photonView.IsMine)
            {
                int t_clip = loadout[currentIndex].GetClip();
                int t_stash = loadout[currentIndex].GetStash();
                p_text.text = t_clip.ToString("D2") + " / " + t_stash.ToString("D2");
            }
        }

        #endregion
    }
}
