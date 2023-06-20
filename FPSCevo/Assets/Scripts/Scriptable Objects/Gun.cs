using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FPSCevo
{

    [CreateAssetMenu(fileName ="New Gun", menuName ="Gun")]
    public class Gun : ScriptableObject
    {
        public string name;
        public int damage;
        public int ammo;
        public int clipSize;
        public GameObject prefab;
        public GameObject display;
        public float fireRate;
        public float recoil;
        public float bloom;
        public float kickback;
        public float aimSpeed;
        public float reloadSpeed;
        public float aim_bob_adjust;
        public int pellets; //shotgun ray sayısı
        [Range(0, 1)] public float mainFOV;
        [Range(0, 1)] public float weaponFOV;
        public int burst; //0 semi | 1 auto | 2+ burst fire
        public float RecoilRefreshTime;
        public bool recovery;

        public AudioClip gunShotSound;
        public float pitchRandomization;

        public int stash; //kalan ammo  
        public int clip; //şu anki şarjör

        public float recoilX;
        public float recoilY;

        public void Initialize()
        {
           stash = ammo;
           clip = clipSize;
        }

        public bool FireBullet()
        {
            if(clip > 0)
            {
                clip -= 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reload()
        {
            stash += clip; 
            clip = Mathf.Min(clipSize, stash);
            stash -= clip;
        }

        public int GetStash()
        {
            return stash;
        }

        public int GetClip()
        {
            return clip;
        }
    }

}
