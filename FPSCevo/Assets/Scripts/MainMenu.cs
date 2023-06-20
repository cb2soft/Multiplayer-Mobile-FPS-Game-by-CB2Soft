using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
//using UnityEngine.SceneManagement;

namespace FPSCevo
{
    public class MainMenu : MonoBehaviour
    {
        public Launcher Launcher;

        private void Start()
        {
            Pause.paused = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public void JoinMatch()
        {
            //SceneManager.LoadScene(1);
            Launcher.Join();
        }

        public void CreateMatch()
        {
            Launcher.Create();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}

