using System;
using UnityEngine;

namespace Scriptable
{
    
    [CreateAssetMenu(fileName = "Player", menuName = "Scriptable Objects/Player")]
    public class LinkData : ScriptableObject
    {
        public int maxHealth = 5;
        public int startHealth = 1;
        
        public PlayerControllerData controllerData;
        public PlayerInputData inputData;
    }
    
    [Serializable]
    public class PlayerControllerData
    {
        [Header("Mouvement")]
        public float walkSpeed = 10f;
        public float rotationSpeed = 20f;
        
        [Header("Jump")]
        public float jumpForce = 10f;
        public float jumpDuration = 0.2f;
        public float minSpeedToJump = 2f; 
        public float edgeDetectDistance = 1.2f;
        public float groundCheckDistance = 0.3f;
        public float groundCheckRadius = 0.35f;
        public float maxStepDown = 0.4f;
        public float maxFallHeight = 3f; 
        public LayerMask groundLayer;
    }
    
    [Serializable]
    public class PlayerInputData
    {
        public InputData interact;
        public InputData crouch;
        public InputData aim;
        public InputData dodge;
        public InputData shoot;
        public InputData melody;
        public InputData openMelodyBook;
        public InputData uiMove;
        public InputData uiQuit;
    }
}