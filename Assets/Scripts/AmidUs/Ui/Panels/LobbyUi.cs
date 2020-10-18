using AmidUs.Server;
using MLAPI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AmidUs.Ui.Panels
{
    public class LobbyUi : MonoBehaviour, UiPanel
    {
        public void Initialize()
        {
            _cancelButton.onClick.AddListener(GoBackToMainMenu);
            _startGameButton.onClick.AddListener(StartGame);
            _playerName.onValueChanged.AddListener(OnPlayerNameChanged);
        }

        void Update()
        {
            _curCooldown -= Time.deltaTime;
            if (_curCooldown < 0)
            {
                _cooldown = 0;
                var playerCount = FindObjectsOfType<Player>().Length;
                _playerCount.text = string.Format("Player Count {0}", playerCount);
            }
        }

        private void OnPlayerNameChanged(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            if (newName.Length > 0)
            {
                Player.GetLocalPlayer().PlayerName.Value = newName;
            }
        }

        public void Show()
        {
            _panel.SetActive(true);
            var netWorkManager = FindObjectOfType<NetworkingManager>();
            if (netWorkManager.IsHost)
            {
                _startGameButton.gameObject.SetActive(true);
            }
            else
            {
                _startGameButton.gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }

        private void GoBackToMainMenu()
        {
            FindObjectOfType<UiManager>().GoBackToMainMenu();
        }

        private void StartGame()
        {
            var hostGame = FindObjectOfType<HostGame>();
            hostGame.StartGame();
        }

        public void SetRoomCode(string roomCode)
        {
            _roomCode = roomCode;
            _roomCodeInputField.text = _roomCode;
        }

        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_InputField _playerName;
        [SerializeField] private TMP_Text _playerCount;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private TMP_InputField _roomCodeInputField;

        private string _roomCode;
        private float _curCooldown;
        private float _cooldown = 1f;
    }
}