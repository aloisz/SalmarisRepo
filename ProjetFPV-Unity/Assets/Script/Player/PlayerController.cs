using System;
using System.Collections;
using AI;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : GenericSingletonClass<PlayerController>
    {
        [Header("Overall Behavior")]
        public bool canMove = true;
        public bool canDoubleJump = true;
        public Vector3 overallMomentum;
        public bool DEBUG;
        
        public Action onDeath;
        
        //---------------------------------------
        
        [Header("Components")]
        [SerializeField] internal PlayerScriptable playerScriptable;
        [SerializeField] private Transform cameraAttachPosition;
        [SerializeField] private Collider capsuleCollider, capsuleColliderSlide;

        [HideInInspector] public Rigidbody _rb;
        
        //---------------------------------------

        [Header("Values")]
        internal Vector3 direction;
        internal Vector3 directionNotReset;
        
        private float _moveSpeed;
        private float _velocity;
        private float _rotationX;
        
        private float _dashTimer;
        private float _timerBeforeReDash;
        private float _idleTimer;

        private float _actualSlopeAngle;

        private int _amountOfJumps;
        
        //---------------------------------------

        [Header("Momentum")] 
        private float _dashTimerSpeedAdd;
        private float _speedMultiplierFromDash = 1f;
        private float _slideAccelerateTimer;

        private bool hasAlreadyAppliedJumpFacility;
        private float _jumpFacilityForce = 0f;
        
        private float _slideBoost;

        private float _timerSlideDragOnGround;
        private float _timerSlideDrag;
        
        //---------------------------------------
        
        [Header("States")]
        public bool isOnGround;
        public bool isMoving;
        public bool isSliding;
        public bool isJumping;
        public bool isDashing;
        public bool isOnSlope;
        public bool isSlopeClimbing;

        internal PlayerActionStates currentActionState;
        
        private bool _canJump;
        private bool _canDash;
        private bool _canApplyGravity = true;
        
        private bool _isAccelerating;
        private bool _isDecelerating;
        
        internal enum PlayerActionStates
        {
            Idle,
            Moving,
            Sliding,
            Jumping,
            Dashing,
        }

        //---------------------------------------

        [Header("Detection")] 
        [SerializeField] private LayerMask groundLayer;

        private RaycastHit _raycastGroundRight;
        private RaycastHit _raycastGroundLeft;
        private RaycastHit _raycastGroundForward;
        private RaycastHit _raycastGroundBack;
        
        private RaycastHit _raycastSlope;
        private RaycastHit _raycastSlopeFront;
        
        private RaycastHit _raycastEdgeDown;
        private RaycastHit _raycastEdgeFromTop;
        
        //---------------------------------------

        public override void Awake()
        {
            base.Awake();
            
            //get the rigidbody component.
            _rb = GetComponent<Rigidbody>();
            
            Cursor.lockState = CursorLockMode.Locked;
        }
        private void Update()
        {
            PlayerStateMachine();

            ManageDashDuration();
            ManageSpeedMultiplierFromDash();
            ManageSlideBoost();
            
            SetMoveSpeed();

            RechargeStamina();
            
            DetectIdling();
            
            DetectSlope();
            DetectGround();
            if (playerScriptable.enableAutoJumpEdge) DetectEdges();

            ManageSlideColliders();

            RotateCameraFromInput();

            //Debug
            if (Input.GetKeyDown(KeyCode.Keypad1)) Time.timeScale = 0.1f;
            if (Input.GetKeyDown(KeyCode.Keypad2)) Time.timeScale = 0.5f;
            if (Input.GetKeyDown(KeyCode.Keypad3)) Time.timeScale = 1f;
        }
        
        private void FixedUpdate()
        {
            Move();
            ManageGravity();
            
            SetDrag();
            
            PlayerInputs.Instance.onJump?.Invoke();
            VerifyDashExecution();
        }

        //-------------------- Movements ----------------------

        #region Movements
        
        /// <summary>
        /// Move the player in the current direction value, based on scriptable parameters.
        /// </summary>
        private void Move()
        {
            //Set the current input values to the direction.
            var dirFromInputs = PlayerInputs.Instance.moveValue;
            direction = new Vector3(dirFromInputs.x, 0, dirFromInputs.y).normalized;
            if (isSliding && !isSlopeClimbing)
                direction = new Vector3(direction.x, direction.y, Mathf.Abs(direction.z));
            
            //If the direction isn't null, set the direction not reset to direction.
            if (direction.magnitude > playerScriptable.moveThreshold) directionNotReset = direction;
            
            _rb.AddForce(GetOverallMomentumVector(), ForceMode.Impulse);

            if (_rb.velocity.magnitude > playerScriptable.rigidbodyClampSlide && isSliding) 
                _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, playerScriptable.rigidbodyClampSlide);

            if (_rb.velocity.magnitude > playerScriptable.maxRigidbodyVelocity)
                _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, playerScriptable.maxRigidbodyVelocity);
        }
        
        /// <summary>
        /// Return the overall momentum physical vector from all the forces applied to player.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetOverallMomentumVector()
        {
            var dividerOnSlope = (!isSlopeClimbing && isSliding ? playerScriptable.slopeSpeedDivider : 1f);
            var accelerating = _rb.velocity.magnitude < playerScriptable.speedMaxToAccelerate && !isSliding;
            var vectorMove = DirectionFromCamera(direction).normalized * (_moveSpeed * _speedMultiplierFromDash * (accelerating ? playerScriptable.accelerationMultiplier : 1f));

            var slopeDirection = new Vector3(_raycastSlope.normal.x, 0, _raycastSlope.normal.z).normalized;

            var vectorSlideDown = Vector3.down * (playerScriptable.slidingInSlopeDownForce);
            var vectorSlideForward = (slopeDirection * (_actualSlopeAngle / playerScriptable.slopeAngleDivider));
            var vectorSlide = vectorSlideDown + ((vectorSlideForward * playerScriptable.slopeForwardBoost));

            var finalVector = ((isMoving ? vectorMove : Vector3.zero) +
                              (isOnSlope && isSliding && !isSlopeClimbing
                                  ? vectorSlide
                                  : Vector3.zero)) / dividerOnSlope;

            var slideBoostValue = (isSliding ? _slideBoost : 1f);
            var tempFinalVectorX = finalVector.x * slideBoostValue;
            var tempFinalVectorZ = finalVector.z * slideBoostValue;

            finalVector = new Vector3(tempFinalVectorX, finalVector.y, tempFinalVectorZ);

            overallMomentum = finalVector / (isSlopeClimbing && isSliding ? playerScriptable.decelerationMultiplierSlideInSlopeUp : 1f);

            return overallMomentum;
        }
        
        /// <summary>
        /// Define the player's speed based on grounded state.
        /// </summary>
        private void SetMoveSpeed()
        {
            _moveSpeed = (isOnGround ? playerScriptable.moveSpeed : 
                playerScriptable.moveSpeed / playerScriptable.moveSpeedInAirDivider) * PlayerKillStreak.Instance.speedBoost;
        }
        
        /// <summary>
        /// Manage the dash duration and the delay between each dashes.
        /// </summary>
        private void ManageDashDuration()
        {
            _dashTimer.DecreaseTimerIfPositive();
            _timerBeforeReDash.DecreaseTimerIfPositive();
            
            if (_dashTimer <= 0f)
            {
                _canApplyGravity = true;
                _rb.useGravity = true;
                isDashing = false;
            }
        }
        
        /// <summary>
        /// Setup the speed bonus gained by using a dash, during a certain duration.
        /// </summary>
        private void ManageSpeedMultiplierFromDash()
        {
            //Decrease the duration while the value is positive.
            _dashTimerSpeedAdd.DecreaseTimerIfPositive();
            
            //Set the speed multiplier from dash to the added % value if the timer isn't finished.
            // Lerp the speed to the multiplier in 1s.
            // Lerp the speed to the basic value in Xs.
            _speedMultiplierFromDash = _dashTimerSpeedAdd > 0 ? 
                Mathf.Lerp(_speedMultiplierFromDash, playerScriptable.dashSpeedMultiplier, Time.deltaTime) : 
                Mathf.Lerp(_speedMultiplierFromDash, 1f, Time.deltaTime / playerScriptable.dashSpeedMultiplierResetDuration);
        }

        /// <summary>
        /// Setup the speed boost when the player start sliding, also decelerate him if he is still sliding on the ground.
        /// It's using an animation curve to manage the boost and the de-boost.
        /// </summary>
        private void ManageSlideBoost()
        {
            if (!isSliding)
            {
                _slideAccelerateTimer = 0f;
            }
            else
            {
                _slideAccelerateTimer += Time.deltaTime;
                _slideBoost = playerScriptable.slideBoostCurve.Evaluate(_slideAccelerateTimer);
            }
        }
        
        /// <summary>
        /// Recharge the player's stamina when he is on the ground or less if he's in the air.
        /// </summary>
        private void RechargeStamina()
        {
            if(!isDashing && !isJumping)
                PlayerStamina.Instance.GenerateStaminaStep(playerScriptable.staminaPerSecond * PlayerKillStreak.Instance.staminaBoost);
            
            else if(isJumping)
                //Generate two times less stamina when in this airs.
                PlayerStamina.Instance.GenerateStaminaStep(playerScriptable.staminaPerSecond * PlayerKillStreak.Instance.staminaBoost / 2f);
        }

        #endregion
        
        //-------------------- Camera ----------------------
        
        #region Camera
        
        /// <summary>
        /// Get the forward vector based on the camera forward direction.
        /// </summary>
        /// <param name="dir">The input direction to convert to.</param>
        /// <returns></returns>
        public Vector3 DirectionFromCamera(Vector3 dir)
        {
            Vector3 camForward = cameraAttachPosition.forward;
            Vector3 camRight = cameraAttachPosition.right;

            camForward.y = 0;
            camRight.y = 0;

            Vector3 forwardRelative = dir.z * camForward;
            Vector3 rightRelative = dir.x * camRight;

            return forwardRelative + rightRelative;
        }
        
        #endregion
        
        //-------------------- Physic ----------------------
        
        #region Physic

        /// <summary>
        /// Managing the gravity by applying a force on the Y velocity axis, and adapt the XZ from the camera direction.
        /// </summary>
        private void ManageGravity()
        {
            if (_canApplyGravity)
            {
                var velocity = _rb.velocity;

                var v = velocity;

                v.y -= Time.deltaTime * playerScriptable.gravityMultiplier;

                v.x = velocity.x + (DirectionFromCamera(direction).x * playerScriptable.moveAirMultiplier);
                v.z = velocity.z + (DirectionFromCamera(direction).z * playerScriptable.moveAirMultiplier);

                _rb.velocity = v;
            }
        }

        /// <summary>
        /// Set the physical material of the player, from it's different states.
        /// </summary>
        /// <param name="s">The state to set the material with.</param>
        private void SetPhysicalMaterialCollider(PlayerActionStates s)
        {
            var chosenMat = s == PlayerActionStates.Idle
                ? playerScriptable.frictionMaterial
                : playerScriptable.movingMaterial;
            
            //if the capsule hasn't the material yet, apply it.
            if (capsuleCollider.sharedMaterial != chosenMat)
            {
                capsuleCollider.material = chosenMat;
            }
            
            //if the slide capsule hasn't the material yet, apply it.
            if (capsuleColliderSlide.sharedMaterial != chosenMat)
            {
                capsuleColliderSlide.material = chosenMat;
            }
        }

        [SerializeField] private float[] drags;
        /// <summary>
        /// Set the rigidbody drag of the player, from it's different states.
        /// </summary>
        private void SetDrag()
        {
            if (isOnGround && isSliding && !isOnSlope) 
                _timerSlideDragOnGround += Time.deltaTime / playerScriptable.timeToReachFrictionWhenSlidingOnGround;
            else _timerSlideDragOnGround = 0f;
            
            if (isOnGround && isSliding && isOnSlope && !isSlopeClimbing) 
                _timerSlideDrag += Time.deltaTime / playerScriptable.timeToReachFrictionWhenSliding;
            else _timerSlideDrag = 0f;
                
            if (isOnGround && isSliding && isOnSlope && !isSlopeClimbing)
            {
                // 0f
                _rb.drag = Mathf.Lerp(0f, drags[0], _timerSlideDrag);
            }
            else if (isOnGround && isSliding && !isOnSlope && !isSlopeClimbing)
            {
                // 0.65f
                _rb.drag = drags[1];
                if (isOnGround && isSliding && !isOnSlope)
                {
                    // 5f
                    _rb.drag = Mathf.Lerp(_rb.drag, drags[2], _timerSlideDragOnGround);
                }
            }
            else if (isOnGround && isSliding && isOnSlope && isSlopeClimbing && _rb.velocity.magnitude < 25f)
            {
                // 0f
                _rb.drag = drags[3];
                _rb.velocity /= 1.2f;
            }
            else if (isOnSlope && !isSliding)
            {
                _rb.drag = playerScriptable.groundDrag;
            }
            else if (isOnGround && isSliding && !isOnSlope && _rb.velocity.magnitude < 20f)
            {
                // 6f
                _rb.drag = drags[4];
            }
            else if (isOnGround && !isSliding)
            {
                _rb.drag = playerScriptable.groundDrag;
            }
            else
            {
                _rb.drag = _rb.velocity.magnitude >= 50f ? 0.8f : playerScriptable.airDrag;
            }
        }

        internal Vector2 GetDirectionXZ(Vector3 vel) => new Vector2(vel.x, vel.z);
        
        #endregion
        
        //-------------------- Detections ----------------------
        
        #region Detections
        
        /// <summary>
        /// Detect the ground by drawing an overlapBoxNonAlloc, and manage the landing verification.
        /// </summary>
        void DetectGround()
        {
            var offset = playerScriptable.groundDetectionForwardOffset;
            var pos = cameraAttachPosition.position + new Vector3(0,playerScriptable.groundDetectionUpOffset,0);
            
            var posCheckRight = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(0), offset);
            var isOnGroundTempRight = Physics.Raycast(posCheckRight, Vector3.down * playerScriptable.groundDetectionLenght, out _raycastGroundRight, 
                playerScriptable.groundDetectionLenght, groundLayer);
            
            var posCheckLeft = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(1), offset);
            var isOnGroundTempLeft = Physics.Raycast(posCheckLeft, Vector3.down * playerScriptable.groundDetectionLenght, out _raycastGroundLeft, 
                playerScriptable.groundDetectionLenght, groundLayer);
            
            var posCheckForward = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(2), offset);
            var isOnGroundTempForward = Physics.Raycast(posCheckForward, Vector3.down * playerScriptable.groundDetectionLenght, out _raycastGroundForward, 
                playerScriptable.groundDetectionLenght, groundLayer);
            
            var posCheckBack = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(3), offset);
            var isOnGroundTempBack = Physics.Raycast(posCheckBack, Vector3.down * playerScriptable.groundDetectionLenght, out _raycastGroundBack, 
                playerScriptable.groundDetectionLenght, groundLayer);

            if (isOnGroundTempRight || isOnGroundTempLeft || isOnGroundTempForward || isOnGroundTempBack)
            {
                if (!isOnGround)
                {
                    isOnGround = true;
                    StartCoroutine(nameof(OnLand));
                }
            }
            else
            {
                isOnGround = false;
            }
        }

        /// <summary>
        /// Return a position from an offset and a lenght of offset. Used for ground checking.
        /// </summary>
        /// <param name="pos">The base position where the offset will be applied.</param>
        /// <param name="dir">The direction where appling the offset.</param>
        /// <param name="offset">The amount of offset to apply.</param>
        /// <returns></returns>
        private Vector3 ReturnCheckOffsetFromDir(Vector3 pos, Vector3 dir, float offset) => pos + dir * offset;
        
        /// <summary>
        /// On land, execute and reset variables.
        /// </summary>
        IEnumerator OnLand()
        {
            _canApplyGravity = false;

            _jumpFacilityForce = 0f;

            if (_rb.velocity.magnitude >= 50f) _rb.velocity *= 1.25f;

            yield return new WaitForSeconds(0.025f);
            
            isJumping = false;
            _amountOfJumps = 0;
        }
        
        /// <summary>
        /// Detect the slope under the player, check if the player is climbing the slope or falling onto it.
        /// </summary>
        private void DetectSlope()
        {
            Physics.Raycast(transform.position + new Vector3(0,0.75f,0), Vector3.down, out _raycastSlope, playerScriptable.raycastLenghtSlopeDetection,
                groundLayer);
            
            Physics.Raycast(transform.position + new Vector3(0,0.75f,0) + (transform.forward * 2f), Vector3.down, 
                out _raycastSlopeFront, playerScriptable.raycastLenghtSlopeDetection, groundLayer);

            _actualSlopeAngle = Vector3.Angle(_raycastSlope.normal, Vector3.up);

            if (_actualSlopeAngle > playerScriptable.minSlopeDegrees) 
            {
                isOnSlope = true;
                isSlopeClimbing = _raycastSlopeFront.point.y > _raycastSlope.point.y;
            }
            else
            {
                isOnSlope = false;
                isSlopeClimbing = false;
            }
        }

        /// <summary>
        /// Function that detect the next edge toward the player,
        /// the height and how much the player need to jump to past it.
        /// </summary>
        private void DetectEdges()
        {
            var transform1 = transform;
            var position = transform1.position;
            var forward = transform1.forward;
            
            //Top Detect Edge Raycast
            if (_raycastEdgeDown.collider)
            {
                if (Physics.Raycast(position + new Vector3(0, playerScriptable.edgeDetectionTopOffsetY, 0), 
                        forward, 
                        Vector3.Distance(transform.position, _raycastEdgeDown.point) + playerScriptable.edgeDetectionOffsetLenght, groundLayer))
                {
                    return;
                }
            }
            else
            {
                if (Physics.Raycast(position + new Vector3(0, playerScriptable.edgeDetectionTopOffsetY, 0),
                        forward,
                        playerScriptable.edgeDetectionLenght + playerScriptable.edgeDetectionOffsetLenght, groundLayer))
                {
                    return;
                }
            }

            //Down Detect Edge Raycast (GREEN)
            Physics.Raycast(position + new Vector3(0, playerScriptable.edgeDetectionBottomOffsetY, 0),
                forward, out _raycastEdgeDown,
                playerScriptable.edgeDetectionLenght, groundLayer);

            //Down From Top Detect Edge Raycast (BLUE)
            if (_raycastEdgeDown.collider)
            {
                Physics.Raycast(position + new Vector3(0, playerScriptable.edgeDetectionTopOffsetY, 0)
                                         + forward * (Vector3.Distance(transform.position, _raycastEdgeDown.point)
                                                      + playerScriptable.edgeDetectionOffsetLenght),
                    Vector3.down, out _raycastEdgeFromTop,
                    playerScriptable.edgeDetectionHeight, groundLayer);
            }
            else
            {
                Physics.Raycast(position + new Vector3(0, playerScriptable.edgeDetectionTopOffsetY, 0)
                                         + forward * (playerScriptable.edgeDetectionLenght +
                                                      playerScriptable.edgeDetectionOffsetLenght),
                    Vector3.down, out _raycastEdgeFromTop,
                    playerScriptable.edgeDetectionHeight, groundLayer);
            }


            if (_raycastEdgeFromTop.collider && _raycastEdgeDown.collider)
            {
                _jumpFacilityForce = Mathf.Lerp(playerScriptable.minMaxJumpFacility.x, playerScriptable.minMaxJumpFacility.y, 
                    Vector3.Distance(_raycastEdgeFromTop.point, _raycastEdgeDown.point) / playerScriptable.maxHeightToJumpFacility);

                if(/*isOnSlope || */!isMoving || direction.z < 0.5f
                   || (!isOnGround && _rb.velocity.y < 10f) || 
                   GetDirectionXZ(_rb.velocity).magnitude < (playerScriptable.moveSpeed * 10f) - 5f) return;
                
                if (!isJumping && _jumpFacilityForce > playerScriptable.maxHeightToJumpFacility) return;
                
                var jumpFacility = Vector3.up * (_jumpFacilityForce * playerScriptable.jumpEdgeImpulseForce);
                
                _rb.velocity = Vector3.zero;
                _rb.AddForce(jumpFacility + GetOverallMomentumVector());
            }
        }
        
        /// <summary>
        /// Manage if slide collider should be active or not,
        /// depending on the slide state of the player.
        /// </summary>
        private void ManageSlideColliders()
        {
            if (isSliding)
            {
                if (!capsuleColliderSlide.enabled)
                {
                    capsuleColliderSlide.enabled = true;
                    capsuleCollider.enabled = false;
                }
            }
            else
            {
                if (!capsuleCollider.enabled)
                {
                    capsuleCollider.enabled = true;
                    capsuleColliderSlide.enabled = false;
                }
            }
        }

        public AI_Pawn DetectNearestEnemy()
        {
            float distance = 99999f;
            AI_Pawn nearest = null;
            foreach (AI_Pawn ai in FindObjectsOfType<AI_Pawn>())
            {
                if (Vector3.Distance(transform.position, ai.transform.position) < distance)
                {
                    distance = Vector3.Distance(transform.position, ai.transform.position);
                    nearest = ai;
                } 
            }
            return nearest;
        }

        #endregion
        
        //-------------------- Inputs ----------------------
        
        #region Inputs

        /// <summary>
        /// Make the player jump, at different heights if he's in coyote jump or not. 
        /// </summary>
        private void Jump()
        {
            if (canDoubleJump)
            {
                if (isOnGround)
                {
                    _amountOfJumps++;
                }
                else
                {
                    _amountOfJumps = 2;
                }
                isJumping = true;
            }
            else if(!isDashing)
            {
                _amountOfJumps = 1;
                isJumping = true;
            }

            var forwardMomentumVector = GetDirectionXZ(_rb.velocity) / (playerScriptable.maxRigidbodyVelocity / playerScriptable.jumpMomentumDivider);

            if (_amountOfJumps < 2) _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            
            _rb.AddForce((_amountOfJumps < 2 ? 
                playerScriptable.jumpForce : 
                playerScriptable.jumpForce * Mathf.Lerp(0.75f, playerScriptable.secondaryJumpMultiplierFromYVel, 
                    Mathf.Abs(_rb.velocity.y / playerScriptable.maxRigidbodyVelocity))) * (Vector3.up + new Vector3(forwardMomentumVector.x, 0, forwardMomentumVector.y)), ForceMode.Impulse);
        }
        
        /// <summary>
        /// Make the camera rotate from where the player look.
        /// </summary>
        private void RotateCameraFromInput()
        {
            var rot = PlayerInputs.Instance.rotateValue;
            
            _rotationX += -rot.y * playerScriptable.sensibility;
            _rotationX = Mathf.Clamp(_rotationX, -playerScriptable.lookLimitY, playerScriptable.lookLimitY); 
            cameraAttachPosition.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, rot.x * playerScriptable.sensibility, 0);
        }

        /// <summary>
        /// Make the player dash.
        /// </summary>
        private void Dash()
        {
            var dashDirectionConvert = Helper.ConvertTo4Dir(new Vector2(direction.x, direction.z));
            var dirFromCam = new Vector3(Mathf.RoundToInt(dashDirectionConvert.x), 0, Mathf.RoundToInt(dashDirectionConvert.y));
            var dashDirection = DirectionFromCamera(dirFromCam);
            var dashDirectionNoY = new Vector3(dashDirection.x, 0, dashDirection.z);

            transform.position += new Vector3(0, 0.5f, 0);
            
            _rb.AddForce((dashDirectionNoY.magnitude < 0.1f ? transform.forward : dashDirectionNoY) * 
                         playerScriptable.dashForce, ForceMode.Impulse);

            _canApplyGravity = false;
            _rb.useGravity = false;
            
            _dashTimer = playerScriptable.dashDuration;
            _timerBeforeReDash = playerScriptable.dashDuration + playerScriptable.dashLagDuration;
            
            _dashTimerSpeedAdd = playerScriptable.dashSpeedMultiplierDuration;

            isDashing = true;

            _rb.velocity = Vector3.zero;
        }

        /// <summary>
        /// Verify if the player can dash.
        /// </summary>
        private void VerifyDashExecution()
        {
            if (_canDash)
            {
                PlayerStamina.Instance.ConsumeStaminaStep(1);
                Dash();
            }
        }

        /// <summary>
        /// Verify if the player can jump.
        /// </summary>
        public void VerifyJumpExecution()
        {
            if(_canJump) Jump();
            PlayerInputs.Instance.onJump -= VerifyJumpExecution;
        }

        #endregion
        
        //-------------------- State Machine ----------------------

        #region StateMachine
        void PlayerStateMachine()
        {
            _canDash = PlayerInputs.Instance.isReceivingDashInputs && !isDashing && 
                       PlayerStamina.Instance.HasEnoughStamina(1) && _timerBeforeReDash < 0.1f;
            _canJump = canDoubleJump ? _amountOfJumps < 2 : (isOnGround && !isJumping);
            
            isSliding = PlayerInputs.Instance.isReceivingSlideInputs && isOnGround;
            isMoving = direction.magnitude > playerScriptable.moveThreshold;
            
            if(isSliding && !isJumping && !isDashing) currentActionState = PlayerActionStates.Sliding;
            
            else if (isMoving && !isSliding && !isJumping && !isDashing && !PlayerInputs.Instance.isReceivingSlideInputs) currentActionState = PlayerActionStates.Moving;
            
            else if (!isSliding && isJumping && !isDashing) currentActionState = PlayerActionStates.Jumping;
                
            else if(isDashing) currentActionState = PlayerActionStates.Dashing;

            else if (_idleTimer <= 0f) currentActionState = PlayerActionStates.Idle;

            else currentActionState = PlayerActionStates.Moving;

            switch (currentActionState)
            {
                case PlayerActionStates.Idle: OnIdle();
                    break;
                case PlayerActionStates.Moving: //_decelerationSlideOnGround = 1f;
                    break;
                case PlayerActionStates.Sliding:
                    break;
                case PlayerActionStates.Jumping:
                    break;
                case PlayerActionStates.Dashing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            SetPhysicalMaterialCollider(currentActionState);
        }

        private void DetectIdling()
        {
            if (isMoving && isOnGround) _idleTimer = playerScriptable.timeBeforeDetectedIdle;
            else _idleTimer.DecreaseTimerIfPositive();
        }

        private void OnIdle()
        {
            //Reset the current dash duration. Make this cancel the dash.
            _dashTimer = 0f;
            
            //Reset the timer of the speed dash bonus.
            _dashTimerSpeedAdd = 0f;
            
            //Reset the speed bonus from dash.
            _speedMultiplierFromDash = 1f;
            
            //Reset the deceleration for the slide on ground.
            //_decelerationSlideOnGround = 1f;
        }

        private void Death() => onDeath.Invoke();

        #endregion

        //-------------------- Debug ----------------------

        #region Debug
        private void OnDrawGizmos()
        {
            var position = transform.position;

            #region Slopes
            
            //Slope Detection
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(position + new Vector3(0, 0.75f, 0),
                Vector3.down * playerScriptable.raycastLenghtSlopeDetection);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + new Vector3(0,0.75f,0) + (transform.forward * 2f),
                Vector3.down * playerScriptable.raycastLenghtSlopeDetection);

            //Slope Normal
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(_raycastSlope.point, _raycastSlope.normal * 2.5f);
            
            #endregion
            
            #region GroundCheck

            if (cameraAttachPosition)
            {
                var offset = playerScriptable.groundDetectionForwardOffset;
                var pos = cameraAttachPosition.position + new Vector3(0, playerScriptable.groundDetectionUpOffset, 0);

                for (int i = 0; i < 4; i++)
                {
                    var posCheck = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(i), offset);

                    Gizmos.color = new Color(1, .5f, 0);
                    Gizmos.DrawRay(posCheck, Vector3.down * playerScriptable.groundDetectionLenght);
                }
            }

            #endregion

            if (!playerScriptable.enableAutoJumpEdge) return;

            #region EdgeDetectionTop
            
            Gizmos.color = Color.red;
            if(_raycastEdgeDown.collider)
            {
                Gizmos.DrawRay(transform.position + new Vector3(0, playerScriptable.edgeDetectionTopOffsetY, 0), 
                transform.forward * (Vector3.Distance(transform.position, _raycastEdgeDown.point) + playerScriptable.edgeDetectionOffsetLenght));
            }
            else
            {
                Gizmos.DrawRay(transform.position + new Vector3(0, playerScriptable.edgeDetectionTopOffsetY, 0), 
                    transform.forward * (playerScriptable.edgeDetectionLenght + playerScriptable.edgeDetectionOffsetLenght));
            }
            
            #endregion
            
            #region EdgeDetectionDown
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position + new Vector3(0, playerScriptable.edgeDetectionBottomOffsetY, 0), 
                transform.forward * playerScriptable.edgeDetectionLenght);

            #endregion
            
            #region EdgeDetectionDownTop
            
            Gizmos.color = Color.blue;
            if (_raycastEdgeDown.collider)
            {
                Gizmos.DrawRay((transform.position + new Vector3(0, playerScriptable.edgeDetectionTopOffsetY, 0))
                               + transform.forward * (Vector3.Distance(transform.position, _raycastEdgeDown.point) + playerScriptable.edgeDetectionOffsetLenght), 
                    Vector3.down * playerScriptable.edgeDetectionHeight);
            }
            else
            {
                Gizmos.DrawRay((transform.position + new Vector3(0, playerScriptable.edgeDetectionTopOffsetY, 0))
                               + transform.forward * (playerScriptable.edgeDetectionLenght + playerScriptable.edgeDetectionOffsetLenght), 
                    Vector3.down * playerScriptable.edgeDetectionHeight);
            }
            
            #endregion
        }

        private void OnGUI()
        {
            if (!DEBUG) return;

            // Set up GUI style for the text
            GUIStyle style = new GUIStyle
            {
                fontSize = 24,
                normal =
                {
                    textColor = Color.white
                }
            };

            // Set the position and size of the text
            // 60 each part
            // 30 each elements
            Rect rect = new Rect(10, 10, 200, 50);
            Rect rect1 = new Rect(10, 40, 200, 50);
            
            Rect rect2 = new Rect(10, 100, 200, 50);
            Rect rect3 = new Rect(10, 130, 200, 50);
            
            Rect rect4 = new Rect(10, 190, 200, 50);
            
            Rect rect5 = new Rect(10, 250, 200, 50);
            
            Rect rect6 = new Rect(10, 310, 200, 50);
            
            Rect rect7 = new Rect(10, 370, 200, 50);
            
            Rect rect8 = new Rect(10, 430, 200, 50);
            Rect rect9 = new Rect(10, 460, 200, 50);
            
            Rect rect10 = new Rect(10, 520, 200, 50);
            
            Rect rect11 = new Rect(10, 580, 200, 50);
            
            Rect rect12 = new Rect(10, 630, 200, 50);

            // Display the text on the screen
            GUI.Label(rect, $"Direction : {direction}", style);
            GUI.Label(rect1, $"Direction No reset : {directionNotReset}", style);
            
            GUI.Label(rect2, $"Rigidbody Velocity : {_rb.velocity}", style);
            GUI.Label(rect3, $"Rigidbody Magnitude : {_rb.velocity.magnitude}", style);
            
            GUI.Label(rect4, $"Current State : {Convert.ToString(currentActionState)}", style);
            
            GUI.Label(rect5, $"Overall Momentum Vector : {GetOverallMomentumVector()}", style);
            
            GUI.Label(rect6, $"Grounded ? : {isOnGround}", BoolStyle(isOnGround));
            
            GUI.Label(rect7, $"Is On Slope ? : {isOnSlope}", BoolStyle(isOnSlope));

            var text = _raycastSlope.collider ? new Vector3(_raycastSlope.normal.x, 0, _raycastSlope.normal.z).normalized : Vector3.zero;
            GUI.Label(rect8, $"Current Slope Direction : {text}", style);
            GUI.Label(rect9, $"Current Slope Angle : {_actualSlopeAngle}", style);
            
            GUI.Label(rect10, $"Rigidbody Drag : {_rb.drag}", style);
            
            GUI.Label(rect11, $"Is Slope Climbing : {isSlopeClimbing}", BoolStyle(isSlopeClimbing));
        }

        GUIStyle BoolStyle(bool value)
        {
            var style = new GUIStyle
            {
                fontSize = 24,
                normal =
                {
                    textColor = value ? Color.green : Color.red
                }
            };
            return style;
        }
        
        #endregion
        
        //-------------------- Unused ----------------------
        
        #region Unused
        //?
        #endregion
    }
}

