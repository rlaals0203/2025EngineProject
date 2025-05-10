using System;
using Blade.Entities;
using UnityEngine;

namespace Blade.Players
{
    public class CharacterMovement : MonoBehaviour, IEntityComponent
    {
        [SerializeField] private float moveSize = 1f;
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float gravity = -9.8f;
        [SerializeField] private float rotationSpeed = 20f;
        [SerializeField] private CharacterController controller;
        // [SerializeField] private Transform parent;
        public bool IsGround => controller.isGrounded;
        public bool CanManualMovement { get; set; } = true;
        public bool IsArrived { get; set; } = true;

        private Vector3 _velocity;
        public Vector3 Velocity => _velocity;
        private float _verticalVelocity;
        private Vector3 _movementDirection;

        private Vector3 targetDestination;
        private Vector3 prevPosition;

        private Entity _entity;
        public void Initialize(Entity entity)
        {
            _entity = entity;
            targetDestination = transform.position;
        }
        
        public void SetMovementDirection(Vector2 movementInput)
        {
            if (IsArrived && movementInput == Vector2.zero)
            {
                Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y);
                prevPosition = targetDestination;
                _movementDirection = direction;
                targetDestination += direction; 
                IsArrived = false;
            }
        }

        private void FixedUpdate()
        {
            Debug.Log($"Movement Direction: {_movementDirection} Target Destination: {targetDestination}");

            CalculateMovement();
            ApplyGravity();
            Move();
        }

        private void CalculateMovement()
        {
            if (CanManualMovement && IsArrived == false)
                _velocity = _movementDirection * (moveSpeed * Time.fixedDeltaTime);

            if (_velocity.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(_velocity);
                Transform parent = _entity.transform;
                parent.rotation = Quaternion.Lerp(parent.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
            }
            
            if ((targetDestination - prevPosition)) {
                StopImmediately();
                IsArrived = true;
            }
        }

        private void ApplyGravity()
        {
            if(IsGround && _verticalVelocity < 0)
                _verticalVelocity = -0.03f;
            else 
                _verticalVelocity += gravity * Time.fixedDeltaTime;
            
            _velocity.y = _verticalVelocity;
        }

        private void Move()
        {
            controller.Move(_velocity);
        }

        public void StopImmediately()
        {
            _movementDirection = Vector3.zero;
        }
    }
}