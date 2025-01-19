using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPSControllerLPFP
{
    /// 메인 캐릭터 관리
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(AudioSource))]

    public class FpsControllerLPFP : MonoBehaviour
    {
        private Status status;
        
// 경고창 off
#pragma warning disable 649
        // 캐릭터에게 적용할 변수들
		[Header("Player Settings")]
        [Tooltip("캐릭터 정보"), SerializeField]
        private Transform arms;

        [Tooltip("캐릭터 위치 정보"), SerializeField]
        private Vector3 armPosition;

		[Header("Audio Settings")]
        [Tooltip("걷는 소리"), SerializeField]
        private AudioClip walkingSound;

        [Tooltip("뛰는 소리"), SerializeField]
        private AudioClip runningSound;

		[Header("Movement Settings")]
        [Tooltip("걷기 속도"), SerializeField]
        private float walkingSpeed = 5f;

        [Tooltip("뛰기 속도"), SerializeField]
        private float runningSpeed = 9f;

        [Tooltip("최대 속도에 도달하는 시간"), SerializeField]
        private float movementSmoothness = 0.125f;

        [Tooltip("점프력"), SerializeField]
        private float jumpForce = 35f;

		[Header("Look Settings")]
        [Tooltip("회전 속도"), SerializeField]
        private float mouseSensitivity = 7f;

        [Tooltip("최대 회전 속도에 도달하는 시간"), SerializeField]
        private float rotationSmoothness = 0.05f;

        [Tooltip("최소 수직 각도"), SerializeField]
        private float minVerticalAngle = -90f;

        [Tooltip("최대 수직 각도"), SerializeField]
        private float maxVerticalAngle = 90f;

        [Tooltip("입력 버튼"), SerializeField]
        private FpsInput input;
// 경고창 On
#pragma warning restore 649

        private Rigidbody _rigidbody; // 캐릭터의 물리적인 속성
        private CapsuleCollider _collider; // 캐릭터의 충돌체
        private AudioSource _audioSource; // 캐릭터의 소리
        private SmoothRotation _rotationX; // 캐릭터의 수평 회전
        private SmoothRotation _rotationY; // 캐릭터의 수직 회전
        private SmoothVelocity _velocityX; // 캐릭터의 수평 속도
        private SmoothVelocity _velocityZ; // 캐릭터의 수직 속도
        private bool _isGrounded; // 캐릭터가 땅에 있는지 확인

        private readonly RaycastHit[] _groundCastResults = new RaycastHit[8]; // 땅에 대한 충돌체
        private readonly RaycastHit[] _wallCastResults = new RaycastHit[8]; // 벽에 대한 충돌체

        private void Start()
        {
            status = GetComponent<Status>();

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _collider = GetComponent<CapsuleCollider>();
            _audioSource = GetComponent<AudioSource>();
			arms = AssignCharactersCamera();
            _audioSource.clip = walkingSound;
            _audioSource.loop = true;
            _rotationX = new SmoothRotation(RotationXRaw);
            _rotationY = new SmoothRotation(RotationYRaw);
            _velocityX = new SmoothVelocity();
            _velocityZ = new SmoothVelocity();
            Cursor.lockState = CursorLockMode.Locked;
            ValidateRotationRestriction();
        }

        // 캐릭터의 카메라 위치를 설정
        private Transform AssignCharactersCamera()
        {
            var t = transform;
			arms.SetPositionAndRotation(t.position, t.rotation);
			return arms;
        }
        
        /// Clamps <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> to valid values and
        /// ensures that <see cref="minVerticalAngle"/> is less than <see cref="maxVerticalAngle"/>.
        private void ValidateRotationRestriction()
        {
            minVerticalAngle = ClampRotationRestriction(minVerticalAngle, -90, 90);
            maxVerticalAngle = ClampRotationRestriction(maxVerticalAngle, -90, 90);
            if (maxVerticalAngle >= minVerticalAngle) return;
            Debug.LogWarning("maxVerticalAngle should be greater than minVerticalAngle.");
            var min = minVerticalAngle;
            minVerticalAngle = maxVerticalAngle;
            maxVerticalAngle = min;
        }

        private static float ClampRotationRestriction(float rotationRestriction, float min, float max)
        {
            if (rotationRestriction >= min && rotationRestriction <= max) return rotationRestriction;
            var message = string.Format("Rotation restrictions should be between {0} and {1} degrees.", min, max);
            Debug.LogWarning(message);
            return Mathf.Clamp(rotationRestriction, min, max);
        }
			
        /// Checks if the character is on the ground.
        private void OnCollisionStay()
        {
            var bounds = _collider.bounds;
            var extents = bounds.extents;
            var radius = extents.x - 0.01f;
            Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
                _groundCastResults, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
            if (!_groundCastResults.Any(hit => hit.collider != null && hit.collider != _collider)) return;
            for (var i = 0; i < _groundCastResults.Length; i++)
            {
                _groundCastResults[i] = new RaycastHit();
            }

            _isGrounded = true;
        }
			
        /// Processes the character movement and the camera rotation every fixed framerate frame.
        private void FixedUpdate()
        {
            // FixedUpdate is used instead of Update because this code is dealing with physics and smoothing.
            RotateCameraAndCharacter();
            MoveCharacter();
            _isGrounded = false;
        }
			
        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        private void Update()
        {
			arms.position = transform.position + transform.TransformVector(armPosition);
            Jump();
            PlayFootstepSounds();
        }

        private void RotateCameraAndCharacter()
        {
            var rotationX = _rotationX.Update(RotationXRaw, rotationSmoothness);
            var rotationY = _rotationY.Update(RotationYRaw, rotationSmoothness);
            var clampedY = RestrictVerticalRotation(rotationY);
            _rotationY.Current = clampedY;
			var worldUp = arms.InverseTransformDirection(Vector3.up);
			var rotation = arms.rotation *
                           Quaternion.AngleAxis(rotationX, worldUp) *
                           Quaternion.AngleAxis(clampedY, Vector3.left);
            transform.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
			arms.rotation = rotation;
        }
			
        /// Returns the target rotation of the camera around the y axis with no smoothing.
        private float RotationXRaw
        {
            get { return input.RotateX * mouseSensitivity; }
        }
			
        /// Returns the target rotation of the camera around the x axis with no smoothing.
        private float RotationYRaw
        {
            get { return input.RotateY * mouseSensitivity; }
        }
			
        /// Clamps the rotation of the camera around the x axis
        /// between the <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> values.
        private float RestrictVerticalRotation(float mouseY)
        {
			var currentAngle = NormalizeAngle(arms.eulerAngles.x);
            var minY = minVerticalAngle + currentAngle;
            var maxY = maxVerticalAngle + currentAngle;
            return Mathf.Clamp(mouseY, minY + 0.01f, maxY - 0.01f);
        }
			
        /// Normalize an angle between -180 and 180 degrees.
        /// <param name="angleDegrees">angle to normalize</param>
        /// <returns>normalized angle</returns>
        private static float NormalizeAngle(float angleDegrees)
        {
            while (angleDegrees > 180f)
            {
                angleDegrees -= 360f;
            }

            while (angleDegrees <= -180f)
            {
                angleDegrees += 360f;
            }

            return angleDegrees;
        }

        private void MoveCharacter()
        {
            var direction = new Vector3(input.Move, 0f, input.Strafe).normalized;
            var worldDirection = transform.TransformDirection(direction);
            var velocity = worldDirection * (input.Run ? runningSpeed : walkingSpeed);
            //Checks for collisions so that the character does not stuck when jumping against walls.
            var intersectsWall = CheckCollisionsWithWalls(velocity);
            if (intersectsWall)
            {
                _velocityX.Current = _velocityZ.Current = 0f;
                return;
            }

            var smoothX = _velocityX.Update(velocity.x, movementSmoothness);
            var smoothZ = _velocityZ.Update(velocity.z, movementSmoothness);
            var rigidbodyVelocity = _rigidbody.velocity;
            var force = new Vector3(smoothX - rigidbodyVelocity.x, 0f, smoothZ - rigidbodyVelocity.z);
            _rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        private bool CheckCollisionsWithWalls(Vector3 velocity)
        {
            if (_isGrounded) return false;
            var bounds = _collider.bounds;
            var radius = _collider.radius;
            var halfHeight = _collider.height * 0.5f - radius * 1.0f;
            var point1 = bounds.center;
            point1.y += halfHeight;
            var point2 = bounds.center;
            point2.y -= halfHeight;
            Physics.CapsuleCastNonAlloc(point1, point2, radius, velocity.normalized, _wallCastResults,
                radius * 0.04f, ~0, QueryTriggerInteraction.Ignore);
            var collides = _wallCastResults.Any(hit => hit.collider != null && hit.collider != _collider);
            if (!collides) return false;
            for (var i = 0; i < _wallCastResults.Length; i++)
            {
                _wallCastResults[i] = new RaycastHit();
            }

            return true;
        }

        private void Jump()
        {
            if (!_isGrounded || !input.Jump) return;
            _isGrounded = false;
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void PlayFootstepSounds()
        {
            if (_isGrounded && _rigidbody.velocity.sqrMagnitude > 0.1f)
            {
                _audioSource.clip = input.Run ? runningSound : walkingSound;
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                }
            }
            else
            {
                if (_audioSource.isPlaying)
                {
                    _audioSource.Pause();
                }
            }
        }
			
        /// A helper for assistance with smoothing the camera rotation.
        private class SmoothRotation
        {
            private float _current;
            private float _currentVelocity;

            public SmoothRotation(float startAngle)
            {
                _current = startAngle;
            }
				
            /// Returns the smoothed rotation.
            public float Update(float target, float smoothTime)
            {
                return _current = Mathf.SmoothDampAngle(_current, target, ref _currentVelocity, smoothTime);
            }

            public float Current
            {
                set { _current = value; }
            }
        }
			
        /// A helper for assistance with smoothing the movement.
        private class SmoothVelocity
        {
            private float _current;
            private float _currentVelocity;

            /// Returns the smoothed velocity.
            public float Update(float target, float smoothTime)
            {
                return _current = Mathf.SmoothDamp(_current, target, ref _currentVelocity, smoothTime);
            }

            public float Current
            {
                set { _current = value; }
            }
        }
			
        /// 입력 버튼을 저장하는 클래스
        [Serializable]
        private class FpsInput
        {
            [Tooltip("수평 회전을 위한 가상 축 이름"), SerializeField]
            private string rotateX = "Mouse X";

            [Tooltip("수직 회전을 위한 가상 축 이름"), SerializeField]
            private string rotateY = "Mouse Y";

            [Tooltip("앞뒤 이동을 위한 가상 축 이름"), SerializeField]
            private string move = "Horizontal";

            [Tooltip("좌우 이동을 위한 가상 축 이름"), SerializeField]
            private string strafe = "Vertical";

            [Tooltip("달리기 버튼에 매핑된 가상 버튼 이름"), SerializeField]
            private string run = "Fire3";   // Left Shift

            [Tooltip("점프 버튼에 매핑된 가상 버튼 이름"), SerializeField]
            private string jump = "Jump";

            // 입력 값 반환
            public float RotateX
            {
                get { return Input.GetAxisRaw(rotateX); }
            }      
            public float RotateY
            {
                get { return Input.GetAxisRaw(rotateY); }
            }       
            public float Move
            {
                get { return Input.GetAxisRaw(move); }
            }        
            public float Strafe
            {
                get { return Input.GetAxisRaw(strafe); }
            }        
            public bool Run
            {
                get { return Input.GetButton(run); }
            }     
            public bool Jump
            {
                get { return Input.GetButtonDown(jump); }
            }
        }

        public void TakeDamage(int damage)
        {
            bool isDie = status.DecreaseHP(damage);

            if (isDie == true)
            {
                SceneManager.LoadScene("ReStart");
            }
        }
    }
}