using UnityEngine;

namespace AmidUs
{
    public class CameraFollow : MonoBehaviour 
    {
        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        public void SetTarget(Transform playerTransform)
        {
            _playerTransform = playerTransform;
            _initialized = true;
            curZoom = startZoom;
        }

        public void ShowGhosts()
        {
            ShowLayer(GHOST_LAYER);
        }

        public void HideGhosts()
        {
            HideLayer(GHOST_LAYER);
        }

        private void ShowLayer(string layer) 
        {
            _camera.cullingMask |= 1 << LayerMask.NameToLayer(layer);
        }
        
        private void HideLayer(string layer) 
        {
            _camera.cullingMask &=  ~(1 << LayerMask.NameToLayer(layer));
        }

        void Update()
        {
            if (!_initialized || _playerTransform == null)
            {
                return;
            }

            var newPos = _playerTransform.position + new Vector3(_cameraOffset.x, curZoom, _cameraOffset.z);

            transform.position = Vector3.Slerp(transform.position, newPos, SmoothFactor);
        }
        
        private Transform _playerTransform;
        private bool _initialized;
        
        public Vector3 _cameraOffset;

        [Range(0.01f, 1.0f)] public float SmoothFactor = 0.5f;
        public float startZoom = 10f;

        private float minZoom = 5f;
        private float maxZoom = 20f;
        private float curZoom = 15f;
        private float zoomSpeedMod = 30f;

        private Camera _camera;
        
        private const string GHOST_LAYER = "Ghost";
    }
}