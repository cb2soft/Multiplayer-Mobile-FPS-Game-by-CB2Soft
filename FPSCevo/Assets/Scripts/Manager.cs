using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using WebSocketSharp;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using ExitGames.Client.Photon.StructWrapping;


namespace FPSCevo
{
    public class PlayerInfo
    {
        public ProfileData profile; //3 tane data burdan geliyor
        public int actor;
        public short kills; // bunlara eklenince toplam 6
        public short deaths;

        public PlayerInfo(ProfileData p, int a, short k, short d)
        {
            this.profile = p;
            this.actor = a;
            this.kills = k;
            this.deaths = d;
        }
    }

    public class Manager : MonoBehaviour, IOnEventCallback
    {
        public string player_prefab_string;
        public GameObject player_prefab;
        public Transform[] spawn_points;
        //public Weapon weapon;

        public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
        public int myInd;

        private TMP_Text ui_myKills;
        private TMP_Text ui_myDeaths;
        private Transform ui_leaderboard;

        public enum EventCodes : byte
        {
            NewPlayer,
            UpdatePlayers,
            ChangeStat
        }


        public void Start()
        {
            ValidateConnection();
            InitializeUI();
            NewPlayer_S(Launcher.myProfile);
            DontDestroyOnLoad(gameObject);
            Spawn();
            //photonView.RPC("Spawn", RpcTarget.All);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                if (ui_leaderboard.gameObject.activeSelf) ui_leaderboard.gameObject.SetActive(false);
                else Leaderboard(ui_leaderboard);
            }
            if(Input.GetKeyUp(KeyCode.Tab))
            {
                ui_leaderboard.gameObject.SetActive(false);
            }
        }
        public void Awake()
        {

        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnEvent (EventData photonEvent)
        {
            if (photonEvent.Code >= 200) return;

            EventCodes e = (EventCodes)photonEvent.Code;
            object [] o = (object[])photonEvent.CustomData;

            switch(e)
            {
                case EventCodes.NewPlayer:
                    NewPlayer_R(o);
                    break;
                case EventCodes.UpdatePlayers:
                    UpdatePlayers_R(o);
                    break;
                case EventCodes.ChangeStat:
                    ChangeStat_R(o);
                    break;
            }
        }

        [PunRPC]
        public void Spawn()
        {

            /*GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            GameObject[] players = GameObject.FindObjectsOfType<Player>().Select(t=>t.gameObject).ToArray();

            float dumDist = 0;
            GameObject dumSpawn = null;
            foreach (var sp in spawnPoints)
                foreach (var pl in players)
                {
                    var dist = Vector3.Distance(sp.transform.position, pl.transform.position);
                    if (dist > dumDist)
                    {
                        dumDist = dist;
                        dumSpawn = sp;
                    }
                }

            //Transform t_spawn = [Random.Range(0, spawn_points.Length)];
            PhotonNetwork.Instantiate(player_prefab, dumSpawn.transform.position, dumSpawn.transform.rotation);*/

            /*Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];

            //if
            PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation); */

            Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];

            if(PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Instantiate(player_prefab_string, t_spawn.position, t_spawn.rotation);
            }
            else
            {
               GameObject newPlayer = Instantiate(player_prefab, t_spawn.position, t_spawn.rotation) as GameObject;
            }

        }

        private void InitializeUI()
        {
            ui_myKills = GameObject.Find("HUD/Stats/Kills/Text").GetComponent<TMP_Text>();
            ui_myDeaths = GameObject.Find("HUD/Stats/Deaths/Text").GetComponent<TMP_Text>();
            ui_leaderboard = GameObject.Find("HUD").transform.Find("Leaderboard").transform;

            RefreshMyStats();
        }

        private void RefreshMyStats()
        {
            if(playerInfo.Count > myInd)
            {
                ui_myKills.text = $"Kills : {playerInfo[myInd].kills}";
                ui_myDeaths.text = $"Deaths : {playerInfo[myInd].deaths}";
            }
            else
            {
                ui_myKills.text = "Kills : 0";
                ui_myDeaths.text = "Deaths : 0";
            }

            if (ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);
        }

        private void Leaderboard(Transform p_ldb)
        {
            //temizleme
            for (int i = 2; i < p_ldb.childCount; i++)
            {
                Destroy(p_ldb.GetChild(i).gameObject);
            }

            //Mod ve map ayarlama
            p_ldb.Find("Header/GameModText").GetComponent<TMP_Text>().text = "FREE FOR ALL";
            p_ldb.Find("Header/MapNameText").GetComponent<TMP_Text>().text = "Test Map";

            GameObject playercard = p_ldb.GetChild(1).gameObject;
            playercard.SetActive(false);

            //Listeleme
            List<PlayerInfo> sorted = SortPlayers(playerInfo);

            //Görünen yazılar
            bool t_alternateColors = false;
            foreach(PlayerInfo a in sorted)
            {
                GameObject newCard = Instantiate(playercard, p_ldb) as GameObject;

               /* if (t_alternateColors) newCard.GetComponent<Image>().color = new Color32(0, 0, 0, 180);
                t_alternateColors = !t_alternateColors; */

                newCard.transform.Find("txt_Level").GetComponent<TMP_Text>().text = a.profile.level.ToString("00");
                newCard.transform.Find("txt_Username").GetComponent<TMP_Text>().text = a.profile.username;
                newCard.transform.Find("txt_Score").GetComponent<TMP_Text>().text = (a.kills * 100).ToString();
                newCard.transform.Find("txt_Kills").GetComponent<TMP_Text>().text = a.kills.ToString();
                newCard.transform.Find("txt_Deaths").GetComponent<TMP_Text>().text = a.deaths.ToString();

                newCard.SetActive(true);
            }

            p_ldb.gameObject.SetActive(true);
        }

        private List<PlayerInfo> SortPlayers(List<PlayerInfo> p_info)
        {
            List<PlayerInfo> sorted = new List<PlayerInfo>();

            while(sorted.Count < p_info.Count)
            {
                //default ayarlama
                short highest = -1;
                PlayerInfo selection = p_info[0];

                //sonraki en yüksek oyuncu
                foreach(PlayerInfo a in p_info)
                {
                    if (sorted.Contains(a)) continue;
                    if(a.kills>highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                //oyuncuyu ekle
                sorted.Add(selection);
            }

            return sorted;
        }

        private void ValidateConnection()
        {
            if (PhotonNetwork.IsConnected) return;
            SceneManager.LoadScene(0);
        }

        public void NewPlayer_S(ProfileData p) //Send data
        {
            object[] package = new object[6];

            package[0] = p.username;
            package[1] = p.level;
            package[2] = p.exp;
            package[3] = PhotonNetwork.LocalPlayer.ActorNumber;
            package[4] = (short)0;
            package[5] = (short)0;

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.NewPlayer,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                new SendOptions { Reliability = true }
                );
        }

        public void NewPlayer_R (object[] data) //Receive data
        {
            PlayerInfo p = new PlayerInfo(
                new ProfileData(
                    (string)data[0],
                    (int)data[1],
                    (int)data[2]
                    ),
                (int)data[3],
                (short)data[4],
                (short)data[5]
                );

            playerInfo.Add(p);

            UpdatePlayers_S(playerInfo);
        }

        public void UpdatePlayers_S(List<PlayerInfo> info)
        {
            object[] package = new object[info.Count];

            for(int i=0; i < info.Count; i++)
            {
                object[] piece = new object[6];

                piece[0] = info[i].profile.username;
                piece[1] = info[i].profile.level;
                piece[2] = info[i].profile.exp;
                piece[3] = info[i].actor;
                piece[4] = info[i].kills;
                piece[5] = info[i].deaths;

                package[i] = piece;
            }

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.UpdatePlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
        }

        public void UpdatePlayers_R(object[] data)
        {
            playerInfo = new List<PlayerInfo>();

            for (int i = 0; i < data.Length; i++)
            {
                object[] extract = (object[]) data[i];

                PlayerInfo p = new PlayerInfo(
                    new ProfileData(
                        (string)extract[0],
                        (int)extract[1],
                        (int)extract[2]
                        ),
                    (int)extract[3],
                    (short)extract[4],
                    (short)extract[5]
                    );

                playerInfo.Add(p);

                if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor) myInd = i;
            }
        }

        public void ChangeStat_S(int actor, byte stat, byte amt)
        {
            object[] package = new object[] { actor, stat, amt };

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.ChangeStat,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
                );
        }

        public void ChangeStat_R(object[] data)
        {
            int actor = (int)data[0];
            byte stat = (byte)data[1];
            byte amt = (byte)data[2];

            for (int i = 0; i < playerInfo.Count; i++)
            {
                if(playerInfo[i].actor == actor)
                {
                    switch(stat)
                    {
                        case 0: //kill
                            playerInfo[i].kills += amt;
                            Debug.Log($"Player { playerInfo[i].profile.username} : kills = {playerInfo[i].kills}");
                            break;

                        case 1: //death
                            playerInfo[i].deaths += amt;
                            Debug.Log($"Player { playerInfo[i].profile.username} : kills = {playerInfo[i].deaths}");
                            break;
                    }     

                    if (i == myInd) RefreshMyStats();
                    if (ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);
                    return;
                }
            }
        }
    }

}
