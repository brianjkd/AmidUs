using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmidUs.Ui;
using AmidUs.Ui.Panels;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

namespace AmidUs.Server
{
    public class VotingManager : NetworkedBehaviour
    {
        void Start()
        {
            _uiManager = FindObjectOfType<UiManager>();
            _votingUi = _uiManager.VotingUi;
        }

        private bool CanVote()
        {
            return _deliberationState == DeliberationState.Voting && !_localPlayer.IsDead();
        }
        
        private bool IsVotingState()
        {
            return _deliberationState == DeliberationState.Voting;
        }

        private void CastVoteAction()
        {
            InvokeServerRpc(PlayerSkipVote, _localPlayer.OwnerClientId);
        }

        [ClientRPC]
        public void RefreshView(ulong reporterOwnerId, ulong deadBodyOwnerId)
        {
            _uiManager.VotingUi.RefreshView(reporterOwnerId, CanVote, IsVotingState, CastVoteAction);
            
            _localPlayer = Player.GetLocalPlayer();

            _playerToPlayersVotingAgainst.Clear();
            _playersWhoSkippedVoting.Clear();
            _playersDoneVoting.Clear();
            
            _remainingTimeToDiscuss = GameConstants.TIME_TO_DISCUSS;
            _remainingTimeToVote = GameConstants.TIME_TO_VOTE;

            var players = FindObjectsOfType<Player>();
            
            foreach (var player in players)
            {
                _playerToPlayersVotingAgainst.Add(player.OwnerClientId, new List<ulong>());
            }

            _expectedVotes = players.Length - players.Count(p => p.IsDead());

            _votingStepsCoroutine = StartCoroutine(VotingSteps());
        }
        
        private IEnumerator VotingSteps()
        {
            _deliberationState = DeliberationState.Discussion;
            while (_remainingTimeToDiscuss > 0)
            {
                _remainingTimeToDiscuss -= Time.deltaTime;
                _votingUi.SetVoteTimer(string.Format("Time to discuss: {0}", Mathf.FloorToInt(_remainingTimeToDiscuss)));
                yield return null;
            }
            
            _deliberationState = DeliberationState.Voting;
            while (_remainingTimeToVote > 0)
            {
                _remainingTimeToVote -= Time.deltaTime;
                _votingUi.SetVoteTimer(string.Format("Time to vote: {0}", Mathf.FloorToInt(_remainingTimeToVote)));
                yield return null;
            }

            ServerEndVoting();
        }
             
        private void ServerEndVoting()
        {
            if (IsServer)
            {
                // stopping duplicate executions from when time is up, but CheckIfVotingIsOver() was called before
                if (_votingStepsCoroutine != null)
                {
                    StopCoroutine(_votingStepsCoroutine);
                }

                var mostVotedPlayer = GetMostVotedPlayer();
                var allPlayers = FindObjectsOfType<Player>();

                if (mostVotedPlayer != NO_VOTE_PLAYER_ID)
                {
                    var playerToKill = allPlayers.First(p => p.OwnerClientId == mostVotedPlayer);
                    playerToKill.SetDead();
                }

                // tell clients to animate voting result
                foreach (var player in allPlayers)
                {
                    if (player.IsDead())
                    {
                        player.InvokeClientRpcOnEveryone(player.TurnOnGhostMode); // turn ghosts on
                        player.IsGhostMode.Value = true;
                    }
                    player.InvokeClientRpcOnClient(player.ShowVoteResultAnimation, player.OwnerClientId,
                        mostVotedPlayer);
                }
            }
        }

        private ulong GetMostVotedPlayer()
        {
            var voteTallies = new List<VoteTally>();
            foreach (var entry in _playerToPlayersVotingAgainst)
            {
                var voteTally = new VoteTally(entry.Key, entry.Value.Count);
                voteTallies.Add(voteTally);
            }
            var voteTallyForNobody = new VoteTally(NO_VOTE_PLAYER_ID, _playersWhoSkippedVoting.Count);
            voteTallies.Add(voteTallyForNobody);
            
            voteTallies.Sort();
            voteTallies.Reverse(); // puts largest vote counts at 0 index
            
            if (voteTallies[0].VotesAgainstCount == voteTallies[1].VotesAgainstCount)
            {
                // tie case
                return NO_VOTE_PLAYER_ID; // in tie, no one dies
            }

            return voteTallies[0].PlayerId;
        }
        
        private bool IsVoteValid(ulong player, ulong votedPlayer)
        {
            var deadPlayers = FindObjectsOfType<Player>().Where(p => p.IsDead()).Select(p => p.OwnerClientId);
            if (deadPlayers.Contains(player) || deadPlayers.Contains(votedPlayer))
            {
                return false; // can't vote if dead or vote for dead
            }
            
            if (_playersDoneVoting.Contains(player))
            {
                return false; // can't double vote
            }

            return true;
        }
        
        private bool IsSkipVoteValid(ulong player)
        {
            var deadPlayers = FindObjectsOfType<Player>().Where(p => p.IsDead()).Select(p => p.OwnerClientId);
            if (deadPlayers.Contains(player))
            {
                return false; // can't vote if dead
            }
            
            if (_playersDoneVoting.Contains(player))
            {
                return false; // can't double vote
            }

            return true;
        }

        private void CheckIfVotingIsOver()
        {
            if (_expectedVotes <= 0)
            {
                ServerEndVoting();
            }
        }

        [ServerRPC(RequireOwnership = false)]
        public void PlayerCastVote(ulong player, ulong votedPlayer)
        {
            if (!IsVoteValid(player, votedPlayer))
            {
                return;
            }
            
            InvokeClientRpcOnEveryone(PlayerCastVoteClientUpdate, player, votedPlayer);

            _expectedVotes--;
            CheckIfVotingIsOver();
        }

        [ServerRPC(RequireOwnership = false)]
        public void PlayerSkipVote(ulong player)
        {
            if (!IsSkipVoteValid(player))
            {
                return;
            }
            
            _playersWhoSkippedVoting.Add(player);
            _playersDoneVoting.Add(player);
            
            InvokeClientRpcOnEveryone(PlayerSkipVoteClientUpdate, player);

            _expectedVotes--;
            CheckIfVotingIsOver();
        }

        [ClientRPC]
        private void PlayerCastVoteClientUpdate(ulong player, ulong votedPlayer)
        {
            _playerToPlayersVotingAgainst[votedPlayer].Add(player);
            _playersDoneVoting.Add(player);
            _votingUi.PlayerCastVoteClientUpdate(player, votedPlayer);
        }
        
        [ClientRPC]
        private void PlayerSkipVoteClientUpdate(ulong player)
        {
            _playersWhoSkippedVoting.Add(player);
            _playersDoneVoting.Add(player);
            _votingUi.PlayerSkipVoteClientUpdate(player);
        }

        public const ulong NO_VOTE_PLAYER_ID = UInt64.MaxValue;
        
        private VotingUi _votingUi;
        private UiManager _uiManager;
        
        private float _remainingTimeToDiscuss;
        private float _remainingTimeToVote;
        private List<ulong> _playersWhoSkippedVoting = new List<ulong>();
        private List<ulong> _playersDoneVoting = new List<ulong>(); // used for blind voting

        private DeliberationState _deliberationState;
        private Dictionary<ulong, List<ulong>> _playerToPlayersVotingAgainst = new Dictionary<ulong, List<ulong>>(); // sync each change with rpcs
        
        private Player _localPlayer;
        private int _expectedVotes;
        private Coroutine _votingStepsCoroutine;
    }
}