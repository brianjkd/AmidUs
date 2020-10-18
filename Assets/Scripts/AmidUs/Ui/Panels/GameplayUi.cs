using System.Collections.Generic;
using System.Linq;
using AmidUs.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AmidUs.Ui.Panels
{
    public class GameplayUi : MonoBehaviour, UiPanel
    {
        public void Hide()
        {
            _panel.SetActive(false);
        }

        public void Initialize()
        {
        }

        public void Show()
        {
            _panel.SetActive(true);
        }
        
        public void DelayedInitialize(Player player)
        {
            _initialized = true;
            _player = player;
            _gameManager = FindObjectOfType<Server.GameManager>();
            if (player.GetRole() != Role.Impostor)
            {
                _kill.gameObject.SetActive(false);
            }

            ResetKillCooldown();

            _report.onClick.AddListener(player.ReportDeadBody);
            _use.onClick.AddListener(player.UseNearest);
            _kill.onClick.AddListener(player.KillNearest);

            Show();
            
            _taskButtonButtonDistanceInteractable = new ButtonDistanceInteractable(_use,
                GameConstants.USE_DISTANCE,
                GetTaskTransforms,
                player.transform);
            
            _reportButtonButtonDistanceInteractable = new ButtonDistanceInteractable(_report,
                GameConstants.REPORT_DISTANCE,
                GetDeadPlayers,
                player.transform);
            
            _killButtonButtonDistanceInteractable = new ButtonDistanceInteractable(_kill,
                GameConstants.KILL_DISTANCE,
                GetAliveCrewMates,
                player.transform);
        }

        private List<Transform> GetTaskTransforms()
        {
            var tasks = FindObjectsOfType<Task>();
            var activeTaskTransforms = tasks.Where(t => t.IsTaskEnabled()).Select(t => t.transform).ToList();
            return activeTaskTransforms;
        }
        
        private List<Transform> GetDeadPlayers()
        {
            if (_player.IsDead()) // a dead player client cannot report
            {
                return new List<Transform>();
            }

            var players = FindObjectsOfType<Player>();
            var deadPlayers = players.Where(p => p.IsDead() && !p.IsGhostMode.Value).Select(t => t.transform).ToList();
            return deadPlayers;
        }
        
        private List<Transform> GetAliveCrewMates()
        {
            if (_player._role != Role.Impostor) // only impostors can kill
            {
                return new List<Transform>();
            }
            var players = FindObjectsOfType<Player>();
            var aliveCrewMates = players.Where(p => !p.IsDead() && p._role == Role.CrewMate).Select(t => t.transform).ToList();
            return aliveCrewMates;
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            _taskbar.text = _gameManager.GetTasksCompleted();

            RefreshKillButton();
            _taskButtonButtonDistanceInteractable.Update();
            _reportButtonButtonDistanceInteractable.Update();
            _killButtonButtonDistanceInteractable.Update();
        }

        public void ResetKillCooldown()
        {
            _curKillCooldown = GameConstants.KILL_COOLDOWN;
        }

        private float _curKillCooldown;
        private void RefreshKillButton()
        {
            if (!_kill.gameObject.activeSelf)
            {
                return;
            }

            _curKillCooldown = Mathf.Max(0,_curKillCooldown - Time.deltaTime);
            var weight = _curKillCooldown / GameConstants.KILL_COOLDOWN;
            _killCooldown.fillAmount = weight;
            _killCooldownTime.text = Mathf.RoundToInt(_curKillCooldown).ToString();

            if (_curKillCooldown <= 0)
            {
                _kill.enabled = true;
                _killCooldown.gameObject.SetActive(false);
                _killCooldownTime.gameObject.SetActive(false);
            }
            else
            {
                _kill.enabled = false;
                _killCooldown.gameObject.SetActive(true);
                _killCooldownTime.gameObject.SetActive(true);
            }
        }
        
        [SerializeField] private Button _report;
        [SerializeField] private Button _kill;
        [SerializeField] private Image _killCooldown;
        [SerializeField] private TMP_Text _killCooldownTime;
        [SerializeField] private Button _use;
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _taskbar;

        private bool _initialized;
        private GameManager _gameManager;
        private Player _player;
        
        private ButtonDistanceInteractable _taskButtonButtonDistanceInteractable;
        private ButtonDistanceInteractable _reportButtonButtonDistanceInteractable;
        private ButtonDistanceInteractable _killButtonButtonDistanceInteractable;
    }
}