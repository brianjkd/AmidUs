using System.Collections.Generic;
using System.Linq;
using AmidUs.Ui;
using AmidUs.Ui.Panels;
using AmidUs.Utils;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using UnityEngine;

namespace AmidUs.Server
{
    public class GameManager : NetworkedBehaviour
    {
        private void AssignRoles()
        {
            var players = new List<Player>();
            players.AddRange(FindObjectsOfType<Player>());
            
            var rng = new System.Random();
            rng.Shuffle(players);

            // required roles will be filled first
            var roles = new List<Role>();
            roles.Add(Role.Impostor); 
            
            rng.Shuffle(PLAYER_COLORS);
            
            var roleIndex = 0;
            for (var i = 0; i < players.Count; i++)
            {
                if (roleIndex < roles.Count)
                {
                    players[i].SetRoleFromServer(roles[roleIndex]);
                    roleIndex++;
                }
                else
                {
                    players[i].SetRoleFromServer(Role.CrewMate);
                }

                players[i].PlayerColor.Value = PLAYER_COLORS[i];
            }
        }

        public void OnGameStart()
        {
            if (!IsServer)
            {
                return;
            }

            _tasksCompleted.Value = 0;
            _totalTasks.Value = 0;
            
            AssignRoles();
            
            var circularTaskList = new CircularList<Task>(FindObjectsOfType<Task>());
            
            var players = FindObjectsOfType<Player>();
            foreach (var player in players)
            {
                player.InvokeClientRpcOnClient(player.ShowGameUI, player.OwnerClientId);
                
                if (player.GetRole() == Role.Impostor)
                {
                    continue; // impostor does not get tasks
                }

                circularTaskList.Shuffle();  // shuffle so tasks are all over the map and diff for each player
                
                var assignedTaskCount = 0;
                while (assignedTaskCount < GameConstants.TASK_PER_PLAYER)
                {
                    var taskToAssign = circularTaskList.GetNext();
                    taskToAssign.InvokeClientRpcOnClient(taskToAssign.EnableTask, player.OwnerClientId);
                    assignedTaskCount++;
                    _totalTasks.Value++;
                }
            }
        }

        [ServerRPC(RequireOwnership = false)]
        public void CompleteTask()
        {
            _tasksCompleted.Value++;
            CheckIfCrewCompletedAllTasksAndWins();
        }

        private void CheckIfCrewCompletedAllTasksAndWins()
        {
            if (_tasksCompleted.Value == _totalTasks.Value)
            {
                var message = "The crew mates win !!!";
                InvokeClientRpcOnEveryone(ShowGameOver, message);
            }
        }

        public void CheckForGameOverBasedOnWhoIsAlive()
        {
            var connectedPlayers = FindObjectsOfType<Player>();
            var aliveCrew = connectedPlayers.Count(p => !p.IsDead() &&
                                                        p._role != Role.Impostor);
            
            var aliveImpostors = connectedPlayers.Count(p => !p.IsDead() &&
                                                             p._role == Role.Impostor);

            var message = "";
            if (aliveCrew == 0)
            {
                var impostorNames = connectedPlayers.Where(p => p._role == Role.Impostor).Select(p => p.PlayerName.Value);
                message = string.Format("Impostors {0} win!!!", string.Join(", ", impostorNames));
                
                InvokeClientRpcOnEveryone(ShowGameOver, message);
            }

            if (aliveImpostors == 0)
            {
                message = "The crew mates win !!!";
                InvokeClientRpcOnEveryone(ShowGameOver, message);
            }
        }
        
        [ClientRPC]
        private void ShowGameOver(string message)
        {
            var uiManager = FindObjectOfType<UiManager>();
            uiManager.GameOverUi.SetGameOverMessage(message);
            uiManager.Show(PanelType.GameOver);
        }

        [ServerRPC(RequireOwnership = false)]
        public void KillPlayer(ulong callerOwnerClientId)
        {
            var players = FindObjectsOfType<Player>();
            var killer = players.First(p => p.OwnerClientId == callerOwnerClientId);
            
            // TODO set kill cooldown on killer rather than relying on client side variable
            var targets = players.Where(p => p.GetRole() == Role.CrewMate &&
                                             !p.IsDead() &&
                                             p.OwnerClientId != callerOwnerClientId);
            var nearestTarget = TransformUtil.GetNearest(killer.transform.position, targets, GameConstants.KILL_DISTANCE);
            if (nearestTarget != null)
            {
                nearestTarget.SetDead();
                killer.InvokeClientRpcOnClient(killer.OnKillWasSuccessful, callerOwnerClientId); // resets kill cooldown
            }
            
            CheckForGameOverBasedOnWhoIsAlive();
        }
        
        [ServerRPC(RequireOwnership = false)]
        public void ReportDeadBody(ulong callerOwnerClientId)
        {
            var players = FindObjectsOfType<Player>();
            var reporter = players.First(p => p.OwnerClientId == callerOwnerClientId);
            if (reporter.IsDead())
            {
                return;
            }

            var deadBodies = players.Where(p => p.IsDead() && 
                                                !p.IsGhostMode.Value && // dont' allow reporting ghosts
                                                p.OwnerClientId != callerOwnerClientId);
            
            var nearestDeadBody = TransformUtil.GetNearest(reporter.transform.position, deadBodies, GameConstants.REPORT_DISTANCE);
            if (nearestDeadBody == null)
            {
                return;
            }
            
            var votingManager = FindObjectOfType<VotingManager>();
            votingManager.InvokeClientRpcOnEveryone(votingManager.RefreshView, reporter.OwnerClientId, nearestDeadBody.OwnerClientId);
        }

        public string GetTasksCompleted()
        {
            return string.Format("{0} / {1} tasks completed.", _tasksCompleted.Value, _totalTasks.Value);
        }
        
        private NetworkedVar<int> _tasksCompleted = new NetworkedVar<int>(0);
        private NetworkedVar<int> _totalTasks = new NetworkedVar<int>(0);
     
        private static List<Color> PLAYER_COLORS = new List<Color>()
        {
            Color.black,
            Color.blue,
            Color.cyan,
            Color.red,
            Color.yellow,
            Color.green,
            Color.white,
            new Color(128f, 0f, 128f),
            new Color (150, 75, 0),
            new Color (0, 80, 0),
        };
    }
}