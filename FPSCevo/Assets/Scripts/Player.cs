using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine.ProBuilder;

namespace FPSCevo
{
    public class Player : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Değişkenler
        [SerializeField] float speed;
        [SerializeField] float sprintModifier;
        [SerializeField] float crouchModifier;
        [SerializeField] Camera normalCam;
        [SerializeField] Camera weaponCamera;
        [SerializeField] GameObject cameraParent;
        [SerializeField] float sprintFOV = 1.5f;
        [SerializeField] float baseFOV;
        [SerializeField] float jumpForce;
        [SerializeField] float jetForce;
        [SerializeField] float jetWait; //Ne kadar süre yerde olursak yakıt gelecek
        [SerializeField] float jetRecovery;
        [SerializeField] float max_fuel;
        [SerializeField] LayerMask ground;
        [SerializeField] Transform groundDetector;
        [SerializeField] Transform weaponParent;
        [SerializeField] int maxHealth;
        public int currentHealth;
        [SerializeField] float lenghtOfSlide;
        [SerializeField] float slideModifier;
        [SerializeField] float slideAmount;
        [SerializeField] float crouchAmount;
        [SerializeField] GameObject standingCollider;
        [SerializeField] GameObject crouchingCollider;
        [SerializeField] GameObject mesh;

        public ProfileData playerProfile;
        public TextMeshPro playerUsername;

        public float highestVel;

        private Transform ui_HealthBar;
        private Transform ui_fuelBar;
        private TMP_Text ui_ammo;
        private TMP_Text ui_username;

        private float movementCounter;
        private float idleCounter;

        private Vector3 targetWeaponBobPosition;
        private Vector3 origin;
        private Vector3 weaponParentCurrentPos;

        public Rigidbody rb;
        private Vector3 weaponParentOrigin;
        private Manager manager;
        private Weapon weapon;
        private float current_fuel;
        private float current_recovery; //ne kadar süredir yerdeyiz

        /* [SerializeField]
         private bool _isGround = false;
         [SerializeField]
         private bool _isGround2 = false; */

        public bool isGrounded; //{ get { return _isGround; } set { _isGround = value; if (value) GetComponent<FallDamage>().FallDmg(30); } }
        public bool isGrounded2; //{ get { return _isGround2; } set { _isGround2 = value; if (value) GetComponent<FallDamage>().FallDmg(30); } }
        public bool isAiming;
        public bool isCrouching;
        private bool canJet;

        private bool sliding;
        private bool crouched;
        private float slide_time;
        private Vector3 slideDirection;

        private float aimAngle;

        private Vector3 normalCamTarget;
        private Vector3 weaponCamTarget;
        #endregion

        #region MonoBehaviour Callbacks

        void changeLayer(Transform _transform, int _layerIndex)
        {
            _transform.gameObject.layer = _layerIndex;
            for (int i = 0; i < _transform.childCount; i++)
            {
                _transform.GetChild(i).gameObject.layer = _layerIndex;
                if (_transform.GetChild(i).childCount > 0)
                    changeLayer(_transform.GetChild(i), _layerIndex);
            }
        }

        private void ChangeLayer2(Transform _changelayerTransform, int _changeLayerIndex)
        {
            _changelayerTransform.gameObject.layer = _changeLayerIndex;
            foreach (Transform t in _changelayerTransform) ChangeLayer2(t, _changeLayerIndex);
        }

        #region Photon Callbacks

        public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
        {
            if (p_stream.IsWriting) //yazan bizsek gönderiyor
            {
                p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f)); //float gönderilebilir ama server performansı düşer
                //p_stream.SendNext(baseFOV);
            }
            else //yazan biz değilsek veri alıyor
            {
                aimAngle = (int)p_stream.ReceiveNext() / 100f;
                //baseFOV = (float)p_stream.ReceiveNext();
            }
        }

        #endregion

        void Start()
        {
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();
            currentHealth = maxHealth;
            current_fuel = max_fuel;

            /*if(photonView.IsMine)
            {
                cameraParent.SetActive(true);
            }
            else
            {
                CameraParent.SetActive(false);
            }*/
            cameraParent.SetActive(photonView.IsMine); //Üstteki kodla aynı cameraparent objesini oyuncu bizsek açıyor

            if (!photonView.IsMine)
            {
                //changeLayer(gameObject.transform,13);
                standingCollider.gameObject.layer = 13;
                crouchingCollider.gameObject.layer = 13;
                //ChangeLayer2(mesh.transform, 13);
                // gameObject.layer = 13; //oyuncu biz değilsek objenin layerını değiştiriyor.
            }

            baseFOV = normalCam.fieldOfView;
            origin = normalCam.transform.localPosition;
            if (Camera.main) Camera.main.enabled = false;  //bir main camera varsa onu kapatıyor.
            //Camera.main.gameObject.SetActive(false);
            rb = GetComponent<Rigidbody>();
            weaponParentOrigin = weaponParent.localPosition;
            weaponParentCurrentPos = weaponParentOrigin;

            if (photonView.IsMine)
            {
                ui_HealthBar = GameObject.Find("HUD/Health/Bar").transform;
                ui_fuelBar = GameObject.Find("HUD/Fuel/Bar").transform;
                ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<TMP_Text>();
                ui_username = GameObject.Find("HUD/Username/Text").GetComponent<TMP_Text>();

                RefreshHealthBar();
                ui_username.text = Launcher.myProfile.username;

                photonView.RPC("SyncProfile", RpcTarget.All, Launcher.myProfile.username, Launcher.myProfile.level, Launcher.myProfile.exp);
            }
        }

        [PunRPC]
        private void SyncProfile(string p_username, int p_level, int p_exp)
        {
            playerProfile = new ProfileData(p_username, p_level, p_exp);
            playerUsername.text = playerProfile.username;
        }

        void Update()
        {
            /*//Hareket yönlerini alma
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");
            
            //Koşma ve Zıplama kontrolü
            //isGrounded = Physics.Raycast(transform.position - transform.up, Vector3.down, 0.2f);
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKeyDown(KeyCode.Space);

            //Koşma ve zıplama kontrolü
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping;

            //Zıplama
            if (isJumping /*&&rb.velocity.y == 0)
            {
                rb.velocity = Vector3.up * jumpForce;
            } */
            Debug.DrawRay(transform.position - transform.up, Vector3.down, Color.red);
        }

        void FixedUpdate()
        {
            if (!photonView.IsMine)
            {
                RefreshMultiplayerState();
                return;
            }

            //Hareket yönlerini alma
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");

            //Koşma ve Zıplama kontrolü
            isGrounded2 = Physics.Raycast(groundDetector.position, Vector3.down, 0.2f, ground); //crouch eklendikten sonra 0.1f yeterli değil 0.15f yükseltildi
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool slide = Input.GetKey(KeyCode.C);
            bool crouch = Input.GetKeyDown(KeyCode.LeftControl);
            bool pause = Input.GetKeyDown(KeyCode.Escape);
            bool aim = Input.GetMouseButton(1);
            bool jet = Input.GetKey(KeyCode.Space);

            //Koşma ve zıplama kontrolü
            bool isJumping = jump && (isGrounded || isGrounded2);
            bool isSprinting = sprint && t_vmove > 0 && !isJumping;
            bool isSliding = isSprinting && slide && !sliding;
            isCrouching = crouch && !isSprinting && !isJumping && (isGrounded || isGrounded2);
            isAiming = aim && !isSliding && !isSprinting;

            //Pause
            if (pause)
            {
                GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
            }
            if (Pause.paused)
            {
                t_hmove = 0f;
                t_vmove = 0f;
                sprint = false;
                jump = false;
                crouch = false;
                pause = false;
                isGrounded = false;
                isGrounded2 = false;
                isJumping = false;
                isSprinting = false;
                isCrouching = false;
                isAiming = false;
            }

            //Oturma
            if (isCrouching)
            {
                photonView.RPC("SetCrouch", RpcTarget.All, !crouched);
            }

            // if (Input.GetKeyDown(KeyCode.Space) && crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);

            //Zıplama
            if (isJumping/*&&rb.velocity.y == 0*/)
            {
                if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false); //zıplamaya basarsak otomatik oturmadan kalkıyor
                rb.velocity = Vector3.up * jumpForce;
                current_recovery = 0f;
                highestVel = 0;  //zıplama hasarıyla alakalı
            }
            if(!isGrounded || !isGrounded2)
            {
                highestVel = rb.velocity.y;  //zıplama hasarıyla alakalı
            }


            if (Input.GetKeyDown(KeyCode.K)) TakeDamage(25, -1);

            //HeadBob
            //if(sliding) { } //bunu yaparsak silah sabit kalıyor. //bunu yaparsak bir alt satır else if olmalı
            if (!isGrounded || !isGrounded2)
            {
                //havada süzülürkene
                HeadBob(idleCounter, 0.025f, 0.025f);
                idleCounter += 0;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if (sliding)
            {
                //kayarken
                HeadBob(movementCounter, 0.055f, 0.055f);
                //movementCounter += Time.deltaTime * 66; //sürünürken silahın sallanmasını istiyorsak
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
                //Movetowards a ile b noktası arasında her frame başına aynı hızda gidiyor kısaca daha hızlı geçiş
                //Lerp a ile b arasında yüzdelik alarak gidiyor smooth geçiş yapıyor ama yavaş
            }
            else if (t_hmove == 0 && t_vmove == 0)
            {
                //dururken
                HeadBob(idleCounter, 0.025f, 0.025f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if (!isSprinting && !crouched)
            {
                //yürürken
                HeadBob(movementCounter, 0.04f, 0.04f);
                movementCounter += Time.deltaTime * 3;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else if (crouched)
            {
                //otururken
                HeadBob(movementCounter, 0.02f, 0.02f);
                movementCounter += Time.deltaTime * 1.75f;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else
            {
                //koşarken
                HeadBob(movementCounter, 0.055f, 0.055f);
                movementCounter += Time.deltaTime * 6;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }

            //Hareket
            Vector3 t_direction = Vector3.zero;
            float t_adjustedSpeed = speed;
            if (!sliding)
            {
                t_direction = new Vector3(t_hmove, 0, t_vmove);
                t_direction.Normalize();
                t_direction = transform.TransformDirection(t_direction);

                if (isSprinting)
                {
                    if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                    t_adjustedSpeed *= sprintModifier;
                }
                else if (crouched)
                {
                    t_adjustedSpeed *= crouchModifier; //1 den düşüklerle çarpmalısın 0.5f vb.
                }
            }
            else
            {
                t_direction = slideDirection;
                t_adjustedSpeed = slideModifier;
                slide_time -= Time.deltaTime;
                if (slide_time <= 0)
                {
                    sliding = false;
                    weaponParentCurrentPos += Vector3.up * (slideAmount - crouchAmount);
                }
            }

            Vector3 t_targetVelocity = t_direction * t_adjustedSpeed * Time.deltaTime;
            t_targetVelocity.y = rb.velocity.y;
            rb.velocity = t_targetVelocity;

            //Koşarken yerde sürünme
            if (isSliding)
            {
                sliding = true;
                slideDirection = t_direction;
                slide_time = lenghtOfSlide;
                //Kamerayı ayarlama
                weaponParentCurrentPos += Vector3.down * (slideAmount - crouchAmount);
                if (!crouched) photonView.RPC("SetCrouch", RpcTarget.All, true);
                //photonView.RPC("SetCrouch", RpcTarget.All, !crouched);
            }

            //Jet Motor
            if (jump && !isGrounded || !isGrounded2)
                canJet = true;
            if (isGrounded || isGrounded2)
                canJet = false;

            if (canJet && jet && current_fuel > 0)
            {
                rb.AddForce(Vector3.up * jetForce * Time.deltaTime, ForceMode.Acceleration);
                current_fuel = Mathf.Max(0, current_fuel - Time.fixedDeltaTime);
            }
            if (isGrounded || isGrounded2)
            {
                if (current_recovery < jetWait)
                    current_recovery = Mathf.Min(jetWait, current_recovery + Time.fixedDeltaTime);
                else
                    current_fuel = Mathf.Min(max_fuel, current_fuel + Time.fixedDeltaTime * jetRecovery);
            }

            ui_fuelBar.localScale = new Vector3(current_fuel / max_fuel, 1, 1);

            //Aim alma
            isAiming = weapon.Aim(isAiming);

            //Koşarken kamera hareketi CAMERA STUFF
            if (sliding)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOV * 1.2f, Time.deltaTime * 8f);
                weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, baseFOV * sprintFOV * 1.2f, Time.deltaTime * 8f);
                //SprintFov * 1f değerini düşürürsek kayarken kamera yaklaşıyor. Yükseltirsek kamera uzaklaşıyor.
                normalCamTarget = Vector3.MoveTowards(normalCam.transform.localPosition, origin + Vector3.down * slideAmount, Time.deltaTime * 6f);
                weaponCamTarget = Vector3.MoveTowards(weaponCamera.transform.localPosition, origin + Vector3.down * slideAmount, Time.deltaTime * 6f);
            }
            else
            {
                if (isSprinting)
                {
                    normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOV, Time.deltaTime * 8f);
                    weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, baseFOV * sprintFOV, Time.deltaTime * 8f);
                }

                else if (isAiming)
                {
                    normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * weapon.currentGunData.mainFOV, Time.deltaTime * 8f);
                    weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, baseFOV * weapon.currentGunData.weaponFOV, Time.deltaTime * 8f);
                }
                else
                {
                    normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
                    weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, baseFOV, Time.deltaTime * 8f);
                }

                if (crouched)
                {
                    normalCamTarget = Vector3.MoveTowards(normalCam.transform.localPosition, origin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
                    weaponCamTarget = Vector3.MoveTowards(weaponCamera.transform.localPosition, origin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
                }
                else
                {
                    normalCamTarget = Vector3.MoveTowards(normalCam.transform.localPosition, origin, Time.deltaTime * 6f);
                    weaponCamTarget = Vector3.MoveTowards(weaponCamera.transform.localPosition, origin, Time.deltaTime * 6f);
                }
            }
            //UI Refreshes
            RefreshHealthBar();
            weapon.RefreshAmmo(ui_ammo);
        }

        private void LateUpdate()
        {
            normalCam.transform.localPosition = normalCamTarget;
            weaponCamera.transform.localPosition = weaponCamTarget;
        }

        #endregion

        #region DebugDrawRay
        /*  private void Update()
          {
              Debug.DrawRay(transform.position-transform.up, Vector3.down, Color.red);
          } 
          private void OnDrawGizmos()
          {
          } */
        #endregion

        #region Özel Metodlar

        void RefreshMultiplayerState()
        {
            float cacheEulY = weaponParent.localEulerAngles.y;
            //quaternion.identity = vector3.zero aynı şeyler

            Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
            weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

            Vector3 finalRotation = weaponParent.localEulerAngles;
            finalRotation.y = cacheEulY;

            weaponParent.localEulerAngles = finalRotation;
        }

        [PunRPC]
        void SetCrouch(bool p_state)
        {
            if (crouched == p_state) return;
            crouched = p_state;

            if (crouched)
            {
                standingCollider.SetActive(false);
                crouchingCollider.SetActive(true);
                weaponParentCurrentPos += Vector3.down * crouchAmount;
            }
            else
            {
                standingCollider.SetActive(true);
                crouchingCollider.SetActive(false);
                weaponParentCurrentPos -= Vector3.down * crouchAmount;
            }
        }

        void HeadBob(float p_z, float p_x_intensity, float p_y_intensity)
        {
            float t_aim_adjust = 1f;
            if (isAiming) t_aim_adjust = weapon.loadout[weapon.currentIndex].aim_bob_adjust;
            //if (weapon.isAminig) t_aim_adjust = weapon.loadout[weapon.currentIndex].aim_bob_adjust;
            targetWeaponBobPosition = weaponParentCurrentPos + new Vector3(Mathf.Cos(p_z) * p_x_intensity * t_aim_adjust, Mathf.Sin(p_z * 2) * p_y_intensity * t_aim_adjust, 0);
        }

        void RefreshHealthBar()
        {
            float t_health_ratio = (float)currentHealth / (float)maxHealth;
            ui_HealthBar.localScale = Vector3.Lerp(ui_HealthBar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f); //geçişli can silme
            //ui_HealthBar.localScale = new Vector3(t_health_ratio, 1, 1);  //Direk can silme
        }

        [PunRPC]
        public void TakeDamage(int p_damage, int p_actor)
        {
            if (photonView.IsMine)
            {
                currentHealth -= p_damage;
                Debug.Log(currentHealth + " / " + p_damage);
                RefreshHealthBar();

                if (currentHealth <= 0)
                {
                    Debug.Log("YOU DIED");
                    manager.Spawn();
                    manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

                    if (p_actor >= 0)
                        manager.ChangeStat_S(p_actor, 0, 1);

                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
        #endregion



    }
}

