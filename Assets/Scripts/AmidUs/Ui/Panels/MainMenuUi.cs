using System;
using AmidUs.Server;
using MLAPI.Transports.UNET;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AmidUs.Ui.Panels
{
    public class MainMenuUi : MonoBehaviour, UiPanel
    {
        public void Initialize()
        {
            _quitGameButton.onClick.AddListener(QuitGame);
            _hostGameButton.onClick.AddListener(CreateRoom);
            _joinGameButton.onClick.AddListener(JoinExistingRoom);
        }

        public void Show()
        {
            _panel.SetActive(true);
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }

        private void CreateRoom()
        {
            var hostGame = FindObjectOfType<HostGame>();
            hostGame.MakeServer();
            var uiManager = FindObjectOfType<UiManager>();
            uiManager.Show(PanelType.Lobby);
            uiManager.LobbyUi.SetRoomCode(GetHostCode());
        }

        private string GetHostCode()
        {
            var unetTransport = FindObjectOfType<UnetTransport>();
            if (unetTransport != null)
            {
                // return unetTransport.ConnectAddress;
                return "127.0.0.1";
            }

            var steamTransport = FindObjectOfType<SteamP2PTransport.SteamP2PTransport>();
            if (steamTransport != null)
            {
                return Convert.ToString(steamTransport.ConnectToSteamID);
            }

            return "ERROR";
        }
        
        private void JoinExistingRoom()
        {
            var roomCode = _roomCodeInputField.text;
            if (string.IsNullOrEmpty(roomCode))
            {
                return;
            }
            var hostGame = FindObjectOfType<HostGame>();
            hostGame.MakeClient(_roomCodeInputField.text);
        }

        private void QuitGame()
        {
            Application.Quit();
        }

        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _hostGameButton;
        [SerializeField] private Button _joinGameButton;
        [SerializeField] private TMP_InputField _roomCodeInputField;
        [SerializeField] private Button _quitGameButton;
    }
}