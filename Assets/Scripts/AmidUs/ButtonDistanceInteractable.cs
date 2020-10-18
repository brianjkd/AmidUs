using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace AmidUs
{
    public class ButtonDistanceInteractable
    {
        public ButtonDistanceInteractable(Button button, float distance, Func<List<Transform>> refreshTransforms, Transform player)
        {
            _button = button;
            _enableRange = distance;
            _refreshTransforms = refreshTransforms;
            _transforms = new List<Transform>();
            _player = player;
            _curCooldown = Random.Range(0f, _refreshCooldown);  // random range for even refresh distribution with multiple DistanceEnabler
        }
        
        public void Update()
        {
            _curCooldown -= Time.deltaTime;
            if (_curCooldown < 0)
            {
                _curCooldown = _refreshCooldown; // reset
                _transforms = _refreshTransforms.Invoke();
            }

            for (var i = 0; i < _transforms.Count; i++)
            {
                var distance = Vector3.Distance(_player.transform.position, _transforms[i].position);
                if (distance < _enableRange)
                {
                    _button.interactable  = true;
                    return;
                }
            }

            _button.interactable  = false;
        }

        private Button _button;
        private float _enableRange;
        private List<Transform> _transforms;
        private Transform _player;
        private Func<List<Transform>> _refreshTransforms;
        
        private float _refreshCooldown = 3f;
        private float _curCooldown;
    }
}