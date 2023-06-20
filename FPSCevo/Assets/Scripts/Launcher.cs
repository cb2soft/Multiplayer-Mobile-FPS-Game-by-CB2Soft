using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

namespace FPSCevo
{

    [System.Serializable]
    public class ProfileData
    {
        public string username;
        public int level;
        public int exp;

        public ProfileData()
        {
            this.username = "DEFAULT";
            this.level = 0;
            this.exp = 0;
        }

        public ProfileData(string u, int l, int x)
        {
            this.username = u;
            this.level = l;
            this.exp = x;
        }
    }

    public class Launcher : MonoBehaviourPunCallbacks
    {

        public TMP_InputField usernameField;
        public static ProfileData myProfile = new ProfileData();

        public GameObject tabMain;
        public GameObject tabRooms;
        public GameObject buttonRoom;
        private List<RoomInfo> roomList;

        public TMP_InputField roomNameField;
        public Slider maxPlayersSlider;
        public TMP_Text maxPlayersValue;
        public GameObject tabCreate;

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;  //otomatik sahne değiştirme tüm kullanıcılar için
            myProfile = Data.LoadProfile();
            usernameField.text = myProfile.username;
            Connect();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Ana sunucuya bağlanıldı.");
            PhotonNetwork.JoinLobby();
            //Join();
            //Debug.Log(PhotonNetwork.CloudRegion);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Odaya bağlanıldı.");
            StartGame();
            base.OnJoinedLobby();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Create();
            base.OnJoinRandomFailed(returnCode, message);
        }

        public void Connect()
        {
            Debug.Log("Bağlanmaya çalışılıyor.");
            PhotonNetwork.GameVersion = "0.0.0";  //Sadece aynı versiyonudaki kişilerin aynı sunucuya bağlanabilmesi
            PhotonNetwork.ConnectUsingSettings();  //Bağlantı
        }

        public void Join()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        public void Create()
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = (byte)maxPlayersSlider.value;

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("map", 0);
            options.CustomRoomProperties = properties;

            PhotonNetwork.CreateRoom(roomNameField.text, options); //Oda oluşturma
        }

        public void ChangeMap()
        {

        }

        public void ChangeMaxPlayersSlider (float t_value)
        {
            maxPlayersValue.text = Mathf.RoundToInt(t_value).ToString();
        }

        public void TabCloseAll()
        {
            tabMain.SetActive(false);
            tabRooms.SetActive(false);
            tabCreate.SetActive(false);
        }

        public void TabOpenMain()
        {
            TabCloseAll();
            tabMain.SetActive(true);
        }

        public void TabOpenRooms()
        {
            TabCloseAll();
            tabRooms.SetActive(true);
        }

        public void TabOpenCreate()
        {
            TabCloseAll();
            tabCreate.SetActive(true);
        }

        private void ClearRoomList()
        {
            Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");
            foreach (Transform a in content) Destroy(a.gameObject);
        }

        private void VerifyUsername()
        {
            if(string.IsNullOrEmpty(usernameField.text))
            {
                myProfile.username = "Random Plyer" + Random.Range(1, 1000);
            }
            else
            {
                myProfile.username = usernameField.text;
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> p_roomList)
        {
            roomList = p_roomList;
            ClearRoomList();

            Debug.Log("Odalar yüklendi @" + Time.time);
            Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");

            foreach(RoomInfo a in roomList)
            {
                GameObject newRoomButton = Instantiate(buttonRoom, content) as GameObject; //instantiate it as a child of content

                newRoomButton.transform.Find("Name").GetComponent<TMP_Text>().text = a.Name;
                newRoomButton.transform.Find("Player").GetComponent<TMP_Text>().text = a.PlayerCount + " / " + a.MaxPlayers;

                newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });
            }

            base.OnRoomListUpdate(roomList);
        }

        public void JoinRoom(Transform p_button)
        {
            Debug.Log("Odaya bağlanılıyor @ " + Time.time);
            string t_roomName = p_button.Find("Name").GetComponent<TMP_Text>().text;
            VerifyUsername();
            PhotonNetwork.JoinRoom(t_roomName);
        }

        public void StartGame()
        {
            VerifyUsername();
            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Data.SaveProfile(myProfile);  //SAVE ETMEK İSTEDİĞİN HER YERE BUNU GÖM
                PhotonNetwork.LoadLevel(1);  //Sahneyi yükleme
            }
        }

    }
}
