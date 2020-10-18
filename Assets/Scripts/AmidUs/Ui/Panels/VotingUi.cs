using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AmidUs.Ui.Panels
{
    public class VotingUi : MonoBehaviour, UiPanel
    {
        void Start()
        {
            _skipVoteButton.onClick.AddListener(_skipCastVote.ShowVoteButtons);
        }

        public void SetVoteTimer(string message)
        {
            _voteTimer.text = message;
        }

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

        public void PlayerCastVoteClientUpdate(ulong player, ulong votedPlayer)
        {
            var voterPlayer = FindObjectsOfType<Player>().FirstOrDefault(p => p.OwnerClientId == player);
            if (votedPlayer != null)
            {
                _playerToBallotUI[votedPlayer].ReceiveVote(voterPlayer);
            }

            ShowIVoted(player);
        }
        
        
        public void PlayerSkipVoteClientUpdate(ulong player)
        {
            var playerWhoSkipped = FindObjectsOfType<Player>().FirstOrDefault(p => p.OwnerClientId == player);
            if (playerWhoSkipped != null)
            {
                _skippedVotingResults.ReceiveVote(playerWhoSkipped);
            }
            
            ShowIVoted(player);
        }

        // called by clients to render the view
        public void RefreshView(ulong reporterOwnerId,
            Func<bool> CanVote,
            Func<bool> IsVotingState,
            Action CastVoteAction)
        {
            _skipCastVote.Init(CanVote, CastVoteAction);
            _skippedVotingResults.ClearVoterIcons();
            
            var uiManager = FindObjectOfType<UiManager>();
            uiManager.Show(PanelType.Voting);

            var players = FindObjectsOfType<Player>();
            
            // clear exist
            foreach (var playerBallotUi in _playerToBallotUI.Values)
            {
                Destroy(playerBallotUi.gameObject);
            }
            _playerToBallotUI.Clear();

            foreach (var player in players)
            {
                var go = Instantiate(_playerBallotPrefab);
                go.transform.parent = _playersToVotePanel.transform;
                var playerBallotUi = go.GetComponent<PlayerBallotUi>();
                playerBallotUi.Init(player, IsVotingState);
                _playerToBallotUI.Add(player.OwnerClientId, playerBallotUi);
            }
            
            _playerToBallotUI[reporterOwnerId].ShowLoudSpeaker();
        }

        public void ShowVotingResults()
        {
            foreach (var playerBallot in _playerToBallotUI.Values)
            {
                playerBallot.ShowVotingResults();
            }
            
            _skippedVotingResults.ShowVotingResults();
        }
        
        private void ShowIVoted(ulong player)
        {
            _playerToBallotUI[player].ShowIVotedIcon();
        }
        
        [SerializeField] private GameObject _panel;
        [SerializeField] private GameObject _playersToVotePanel;
        [SerializeField] private TMP_Text _voteTimer;
        [SerializeField] private Button _skipVoteButton;

        [SerializeField] private GameObject _playerBallotPrefab;
        [SerializeField] private VotingResults _skippedVotingResults;
        [SerializeField] private CastVote _skipCastVote;

        private Dictionary<ulong, PlayerBallotUi> _playerToBallotUI = new Dictionary<ulong, PlayerBallotUi>();
    }
}