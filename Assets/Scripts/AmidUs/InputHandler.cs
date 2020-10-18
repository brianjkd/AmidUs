﻿using UnityEngine;

namespace AmidUs
{
    // https://github.com/Srfigie/Unity-3d-TopDownMovement/blob/master/Assets/Scripts/InputHandler.cs
    public class InputHandler : MonoBehaviour
    {
        public Vector2 InputVector { get; private set; }

        public Vector3 MousePosition { get; private set; }
        
        // Update is called once per frame
        void Update()
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            InputVector = new Vector2(h, v);

            MousePosition = Input.mousePosition;
        }
    }
}