using UnityEngine;
using UnityEngine.InputSystem;

namespace AOTADev
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input References")]
        [SerializeField] private PlayerInput playerInput;

        // Input Action references
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _jumpAction;

        [Header("Camera")]
        [SerializeField] private Camera _camera;
        public Camera MainCamera { get => _camera; set => _camera = value; }

        [SerializeField] private float sensitivityX = 0.1f;
        [SerializeField] private float sensitivityY = 0.1f;
        [SerializeField] private float minPitch = -89f;
        [SerializeField] private float maxPitch = 89f;
        [SerializeField] private bool lockCursor = true;

        private float _pitch;

        [Header("Movement")]
        [SerializeField] private float _gravityAccel = -20f;
        [SerializeField] private float acceleration = 40f;
        [SerializeField] private float maxMoveSpeed = 6f;
        [SerializeField] private float brakingFactor = 16f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 6f;
        [SerializeField] private float groundCheckDistance = 1.1f; 

        [Header("Grounding")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private int ignoreLayer = 7;

        private Rigidbody _rb;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_camera == null) _camera = GetComponentInChildren<Camera>();

            var actions = playerInput.actions;
            _moveAction = actions["Move"];
            _lookAction = actions["Look"];
            _attackAction = actions["Attack"];
            _jumpAction = actions["Jump"];

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            _moveInput = _moveAction.ReadValue<Vector2>();
            Vector2 lookDelta = _lookAction.ReadValue<Vector2>();

            if (_jumpAction != null && _jumpAction.WasPressedThisFrame())
                TryJump();

            if (_attackAction != null && _attackAction.WasPressedThisFrame())
                TryAttack();

            float yaw = lookDelta.x * sensitivityX;
            float pitchDelta = -lookDelta.y * sensitivityY;

            _pitch = Mathf.Clamp(_pitch + pitchDelta, minPitch, maxPitch);

            transform.Rotate(Vector3.up, yaw, Space.Self);
            if (MainCamera != null)
                MainCamera.transform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }

        private void FixedUpdate() => UpdateLocomotion();

        private void TryJump()
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void TryAttack()
        {
            Debug.Log("Attack triggered!");
        }

        private void UpdateLocomotion()
        {
            Vector3 netAccel = Vector3.up * _gravityAccel;

            // Calculate world direction based on camera horizontal plane
            Vector3 camForward = Vector3.ProjectOnPlane(MainCamera.transform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(MainCamera.transform.right, Vector3.up).normalized;
            Vector3 inputWorld = (camForward * _moveInput.y) + (camRight * _moveInput.x);

            bool hasInput = _moveInput.sqrMagnitude > 1e-6f;

            if (hasInput)
                netAccel += ComputeMoveAccel(inputWorld);
            else
                netAccel += ComputeBrakeAccel();

            _rb.AddForce(netAccel, ForceMode.Acceleration);
        }

        private Vector3 ComputeMoveAccel(Vector3 worldMoveDir)
        {
            Vector3 desiredDir = worldMoveDir.normalized;
            float accel = acceleration * (IsGrounded() ? 1f : 2f);

            Vector3 vH = Horizontal(_rb.linearVelocity);
            float dt = Time.fixedDeltaTime;
            Vector3 proposed = vH + desiredDir * accel * dt;

            if (proposed.magnitude > maxMoveSpeed)
            {
                float keep = Mathf.Max(vH.magnitude, maxMoveSpeed);
                proposed = proposed.normalized * keep;
            }

            return (proposed - vH) / dt;
        }

        private Vector3 ComputeBrakeAccel()
        {
            if (!IsGrounded()) return Vector3.zero;
            Vector3 vH = Horizontal(_rb.linearVelocity);
            if (vH.sqrMagnitude <= 1e-6f) return Vector3.zero;

            float dt = Time.fixedDeltaTime;
            float maxBraking = Mathf.Min(brakingFactor, vH.magnitude / dt);
            return -maxBraking * vH.normalized;
        }

        private static Vector3 Horizontal(Vector3 v) => Vector3.ProjectOnPlane(v, Vector3.up);

        //Buggt
        private bool IsGrounded()
        {
            int ignoreMask = ~(1 << ignoreLayer);
            int mask = groundMask.value & ignoreMask;
            return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, mask, QueryTriggerInteraction.Ignore);
        }
    }
}