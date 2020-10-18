using AmidUs.Utils;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

namespace AmidUs
{
    public class Task : NetworkedBehaviour
    {
        void Awake()
        {
            _renderer = gameObject.GetComponent<Renderer>(); 
            Off();
        }

        private void On()
        {
            _renderer.enabled = true;
            _hintArrow.SetActive(true);
        }

        private void Off()
        {
            _renderer.enabled = false;
            _hintArrow.SetActive(false);
        }

        public bool IsTaskEnabled()
        {
            return _renderer.enabled;
        }
        
        void Update()
        {
            if (IsTaskEnabled() && _player != null)
            {
                var distance = Vector3.Distance(_player.transform.position, transform.position);
                if (distance > _hintRadius)
                {
                    _hintArrow.SetActive(true);
                    var angletToFacePlayer = TransformUtil.GetRotationToLookAtTarget(transform.position, _player.transform.position)
                        .eulerAngles.y;
                    
                    _hintArrow.transform.eulerAngles = new Vector3(0f, angletToFacePlayer, 0f);  
                    _hintArrow.transform.position = _player.transform.position - (_hintArrow.transform.forward *  _hintRadius) + new Vector3(0f, 1f, 0f);
                }
                else
                {
                    _hintArrow.SetActive(false);
                }
            }
        }

        [ClientRPC]
        public void EnableTask()
        {
            On();
            _player = Player.GetLocalPlayer();
        }
        
        public void DisableTask()
        {
            Off();
        }
        
        public void CompleteTask()
        {
            var taskManager = FindObjectOfType<Server.GameManager>();
            taskManager.InvokeServerRpc(taskManager.CompleteTask);
            // TODO don't disable until server acks back?
            Off();
        }
        
        private Renderer _renderer;
        [SerializeField] private GameObject _hintArrow;
        private Player _player;
        private float _hintRadius = 4f;
    }
}