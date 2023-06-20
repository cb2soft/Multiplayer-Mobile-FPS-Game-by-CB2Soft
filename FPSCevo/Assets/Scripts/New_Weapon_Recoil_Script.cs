using UnityEngine;
using Photon.Pun;

namespace FPSCevo
{
    public class New_Weapon_Recoil_Script : MonoBehaviourPunCallbacks
    {
        [Header("Recoil_Transform")]
        public Transform RecoilPositionTranform;
        public Transform RecoilRotationTranform;
        [Space(10)]
        [Header("Recoil_Settings")]
        public float PositionDampTime;
        public float RotationDampTime;
        [Space(10)]
        public float Recoil1;
        public float Recoil2;
        public float Recoil3;
        public float Recoil4;
        [Space(10)]
        public Vector3 RecoilRotation;
        public Vector3 RecoilKickBack;

        public Vector3 RecoilRotation_Aim;
        public Vector3 RecoilKickBack_Aim;
        [Space(10)]
        public Vector3 CurrentRecoil1;
        public Vector3 CurrentRecoil2;
        public Vector3 CurrentRecoil3;
        public Vector3 CurrentRecoil4;
        [Space(10)]
        public Vector3 RotationOutput;

        public bool aim;

        void FixedUpdate()
        {
            CurrentRecoil1 = Vector3.Lerp(CurrentRecoil1, Vector3.zero, Recoil1 * Time.deltaTime);
            CurrentRecoil2 = Vector3.Lerp(CurrentRecoil2, CurrentRecoil1, Recoil2 * Time.deltaTime);
            CurrentRecoil3 = Vector3.Lerp(CurrentRecoil3, Vector3.zero, Recoil3 * Time.deltaTime);
            CurrentRecoil4 = Vector3.Lerp(CurrentRecoil4, CurrentRecoil3, Recoil4 * Time.deltaTime);

            RecoilPositionTranform.localPosition = Vector3.Slerp(RecoilPositionTranform.localPosition, CurrentRecoil3, PositionDampTime * Time.fixedDeltaTime);
            RotationOutput = Vector3.Slerp(RotationOutput, CurrentRecoil1, RotationDampTime * Time.fixedDeltaTime);
            RecoilRotationTranform.localRotation = Quaternion.Euler(RotationOutput);
        }
        public void Fire()
        {
                CurrentRecoil1 += new Vector3(RecoilRotation_Aim.x, Random.Range(-RecoilRotation_Aim.y, RecoilRotation_Aim.y), Random.Range(-RecoilRotation_Aim.z, RecoilRotation_Aim.z));
                CurrentRecoil3 += new Vector3(Random.Range(-RecoilKickBack_Aim.x, RecoilKickBack_Aim.x), Random.Range(-RecoilKickBack_Aim.y, RecoilKickBack_Aim.y), RecoilKickBack_Aim.z);
        }
    }
}
