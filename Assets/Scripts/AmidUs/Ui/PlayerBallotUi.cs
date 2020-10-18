using System;
using AmidUs.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AmidUs.Ui
{
    public class PlayerBallotUi : MonoBehaviour
    {
        public void ShowLoudSpeaker()
        {
            _loudspeakerIcon.enabled = true;
        }

        public void Init(Player player, Func<bool> isVotingState)
        {
            _isVotingState = isVotingState;
            _votingResults.ClearVoterIcons();

            _loudspeakerIcon.enabled = false;
            
            _iVoted.SetActive(false);
            _deadIcon.SetActive(false);

            _localPlayer = Player.GetLocalPlayer();
            _player = player;
            
            _name.text = string.Format(player.PlayerName.Value);
            _playerIcon.color = player.PlayerColor.Value;

            if (player.IsDead())
            {
                ShowDeadIcon();
            }
            
            _castVote.Init(CanVote, CastVoteAction);
            _showVoteOptions.onClick.AddListener(_castVote.ShowVoteButtons);
        }
        
        private bool CanVote()
        {
            return !_localPlayer.IsDead() && !_player.IsDead() && _isVotingState.Invoke();
        }

        private void CastVoteAction()
        {
            var votingManager = FindObjectOfType<VotingManager>();
            votingManager.InvokeServerRpc(votingManager.PlayerCastVote, _localPlayer.OwnerClientId, _player.OwnerClientId);
        }

        public void ShowIVotedIcon()
        {
            _iVoted.SetActive(true);
        }
        
        private void ShowDeadIcon()
        {
            _deadIcon.SetActive(true);
        }

        public void ReceiveVote(Player voter)
        {
            _votingResults.ReceiveVote(voter);
        }

        public void ShowVotingResults()
        {
            _votingResults.ShowVotingResults();
        }
        
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Button _showVoteOptions;
        [SerializeField] private Image _playerIcon;
        [SerializeField] private Image _loudspeakerIcon;
        [SerializeField] private GameObject _deadIcon;
        [SerializeField] private GameObject _iVoted;
        
        [SerializeField] private VotingResults _votingResults;
        [SerializeField] private CastVote _castVote;
        
        private Player _localPlayer;
        private Player _player;
        
        private Func<bool> _isVotingState;
    }
}