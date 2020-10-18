using System.Collections;
using System.Linq;
using AmidUs.Server;
using AmidUs.Ui;
using AmidUs.Utils;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using UnityEngine;

namespace AmidUs
{
    // movement code taken from https://github.com/Srfigie/Unity-3d-TopDownMovement/blob/master/Assets/Scripts/TopDownCharacterMover.cs
    public class Player : NetworkedBehaviour
    {
        public static Player GetLocalPlayer()
        {
            return FindObjectsOfType<Player>().First(p => p.IsLocalPlayer);
        }

        private Player _localPlayer; 

        public bool IsDead()
        {
            return _isDead.Value;
        }

        public void SetDead()
        {
            _isDead.Value = true;
        }

        public Role GetRole()
        {
            return _role;
        }

        public void SetRoleFromServer(Role role)
        {
            InvokeClientRpcOnEveryone(SetRole, role);
        }

        [ClientRPC]
        public void ShowVoteResultAnimation(ulong killedPlayer)
        {
            transform.position = FindObjectOfType<SpawnPoint>().GetRandomPosition(); // reset position
            StartCoroutine(Animate(killedPlayer));
        }
        
        [ClientRPC]
        public void TurnOnGhostMode()
        {
            if (IsLocalPlayer) // see Ghosts
            {
                // add ghost layer to layers the camera can see
                var cameraFollow = FindObjectOfType<CameraFollow>();
                cameraFollow.ShowGhosts();
            }
            
            if (IsLocalPlayer)  // TODO this hack prevents errors when player is not local because Animator never setup...
            {
                _animator.SetBool("isDead", false); // so ghost can have walk animations again
            } 
            
            SetGhostColor();
            LayerUtil.SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("Ghost"));
        }

        private void SetGhostColor()
        {
            var curColor = PlayerColor.Value;
            curColor.a = 0.05f;
            PlayerColor.Value = curColor;
            MaterialUtil.SetBlend(MaterialBlendMode.Fade, _material);
        }

        private void ClearGhostColor()
        {
            var curColor = PlayerColor.Value;
            curColor.a = 1f;
            PlayerColor.Value = curColor;
            MaterialUtil.SetBlend(MaterialBlendMode.Opaque, _material);
        }

        private IEnumerator Animate(ulong killedPlayer)
        {
            // show voting results;
            var uiManager = FindObjectOfType<UiManager>();

            uiManager.VotingUi.ShowVotingResults();
            
            yield return new WaitForSeconds(3f);

            var message = "";
            if (killedPlayer == VotingManager.NO_VOTE_PLAYER_ID)
            {
                message = "No one was voted off.";
            }
            else
            {
                var player = FindObjectsOfType<Player>().First(p => p.OwnerClientId == killedPlayer);
                if (player._role == Role.Impostor)
                {
                    message = string.Format("{0} was an Impostor :)", player.PlayerName.Value);
                }
                else
                {
                    message = string.Format("{0} was NOT an Impostor :(", player.PlayerName.Value);
                }
            }
            
            uiManager.VoteResultsUi.SetMessage(message);
            uiManager.Show(PanelType.VoteResults);
            yield return new WaitForSeconds(5f);
            uiManager.Show(PanelType.Gameplay);
            if (IsServer)
            {
                var taskManager = FindObjectOfType<Server.GameManager>();
                taskManager.CheckForGameOverBasedOnWhoIsAlive();
            }
        }

        [ClientRPC]
        public void ShowGameUI()
        {
            var uiManager = FindObjectOfType<UiManager>();
            
            uiManager.GameplayUi.DelayedInitialize(this);
            uiManager.Show(PanelType.Gameplay);
        }
        
        [ClientRPC]
        public void SetRole(Role role)
        {
            _role = role;
        }
        
        void Start()
        {
            _material = GetComponentInChildren<Renderer>().material;
            PlayerName.Value = string.Format("Player_{0}", OwnerClientId);
            ClearGhostColor();
            _localPlayer = GetLocalPlayer();

            if (!IsLocalPlayer)
            {
                _flashlight.SetActive(false);
            }
            else // Local player
            {
                _flashlight.SetActive(true);

                _input = GetComponent<InputHandler>();
                var cameraFollow = FindObjectOfType<CameraFollow>();
                _camera = cameraFollow.GetComponent<Camera>();

                transform.position = FindObjectOfType<SpawnPoint>().GetRandomPosition();
                cameraFollow.SetTarget(transform);

                _animator = GetComponent<Animator>();
            }
        }
        
        void Update()
        {
            _material.color = PlayerColor.Value;
            RenderName();

            if (!IsLocalPlayer)
            {
                return;
            }

            if (_isDead.Value && !IsGhostMode.Value)
            {
                _animator.SetBool("isDead", true);
                return;
            }

            var targetVector = new Vector3(_input.InputVector.x, 0, _input.InputVector.y);

            if (targetVector.x != 0f || targetVector.z != 0f)
            {
                _animator.SetBool("isWalking", true);
            }
            else
            {
                _animator.SetBool("isWalking", false);
            }

            var movementVector = MoveTowardTarget(targetVector);

            if (!RotateTowardMouse)
            {
                RotateTowardMovementVector(movementVector);
            }
            if (RotateTowardMouse)
            {
                RotateFromMouseVector();
            }
        }

        private Player GetClientPlayer()
        {
            if (_clientPlayer == null)
            {
                _clientPlayer = GetLocalPlayer();
            }

            return _clientPlayer;
        }

        private bool CanSeeKillers()
        {
            return GetClientPlayer()._role == Role.Impostor;
        }

        private void RenderName()
        {
            if (IsGhostMode.Value && _localPlayer.IsGhostMode.Value || !_isDead.Value)
            {
                var color = Color.white;
                if (_role == Role.Impostor && CanSeeKillers())
                {
                    color = Color.red;
                }
                
                _nameplate.SetName(PlayerName.Value , color);
            }
        }

        private void RotateFromMouseVector()
        {
            Ray ray = _camera.ScreenPointToRay(_input.MousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f))
            {
                var target = hitInfo.point;
                target.y = transform.position.y;
                transform.LookAt(target);
            }
        }

        private Vector3 MoveTowardTarget(Vector3 targetVector)
        {
            var speed = MovementSpeed * Time.deltaTime;

            targetVector = Quaternion.Euler(0, _camera.gameObject.transform.rotation.eulerAngles.y, 0) * targetVector;
            var targetPosition = transform.position + targetVector * speed;
            transform.position = targetPosition;
            return targetVector;
        }

        private void RotateTowardMovementVector(Vector3 movementDirection)
        {
            if (movementDirection.magnitude == 0)
            {
                return;
            }
            var rotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, RotationSpeed);
        }

        public void KillNearest()
        {
            var taskManager = FindObjectOfType<Server.GameManager>();
            taskManager.InvokeServerRpc(taskManager.KillPlayer, OwnerClientId);
        }
        
        [ClientRPC]
        public void OnKillWasSuccessful()
        {
            var uiManager = FindObjectOfType<UiManager>();
            uiManager.GameplayUi.ResetKillCooldown();
        }

        public void ReportDeadBody()
        {
            if (_isDead.Value)
            {
                return;
            }

            var taskManager = FindObjectOfType<Server.GameManager>();
            taskManager.InvokeServerRpc(taskManager.ReportDeadBody, OwnerClientId);
        }

        public void UseNearest()
        {
            var availableTasks = FindObjectsOfType<Task>().Where(t => t.IsTaskEnabled());
            var nearestTask = TransformUtil.GetNearest(transform.position, availableTasks, GameConstants.USE_DISTANCE);

            if (nearestTask != null)
            {
                var uiManager = FindObjectOfType<UiManager>();
                uiManager.TaskUi.SetOnTaskCompleteAction(nearestTask.CompleteTask);
                uiManager.Show(PanelType.Task);
            }
        }

        public void EmergencyMeeting()
        { 
            // TODO
        }

        public void UseNearestVent()
        {
            // TODO
        }
        
        public NetworkedVar<string> PlayerName = new NetworkedVar<string>(new NetworkedVarSettings {WritePermission = NetworkedVarPermission.OwnerOnly}, "player");
        private NetworkedVar<bool> _isDead = new NetworkedVar<bool>(new NetworkedVarSettings {WritePermission = NetworkedVarPermission.ServerOnly}, false);
        public NetworkedVar<bool> IsGhostMode = new NetworkedVar<bool>(new NetworkedVarSettings {WritePermission = NetworkedVarPermission.ServerOnly}, false);
        public NetworkedVar<Color> PlayerColor = new NetworkedVar<Color>(new NetworkedVarSettings {WritePermission = NetworkedVarPermission.ServerOnly}, Color.blue);
        public Role _role = Role.Unassigned;

        private Material _material;
        [SerializeField] private GameObject _flashlight;
        private Player _clientPlayer;
        
        private InputHandler _input;

        [SerializeField] private bool RotateTowardMouse;

        [SerializeField] private float MovementSpeed;
        [SerializeField] private float RotationSpeed;

        private Camera _camera;
        private Animator _animator;

        [SerializeField] private Nameplate _nameplate;
    }
}