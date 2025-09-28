using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

// Make sure our object has all the components it needs to work
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(CameraSelector))]

public class StateMachineMovement : MonoBehaviour
{
    // Define what states are possible for our player
    public enum State
    {
        Walk,
        Rise,
        Fall,
        Jet,
        Slide
    }

    // 'SerializeField' is an attribute which lets a private variable be edited in Unity
    [SerializeField] private State currentState;

    [SerializeField] private float walkSpeed;

    [SerializeField] private float jumpPower;

    [SerializeField] private float gravity;

    [SerializeField] private LayerMask groundLayer;

    // How many jumps are allowed total?
    [SerializeField] private int jumpsAllowed = 2;

    [SerializeField] private float slideDuration = 1f;   // Duration of a slde in seconds
    [SerializeField] private float slideSpeedBoost = 5f; // Additional speed apllied when sliding
    [SerializeField] private float slideCooldown = 1f; // cooldown between sliding

    private float slideTimer; 
    private float slideCooldownTimer;

    [SerializeField] private Transform playerModel;  // the child model for the player for tilting
    [SerializeField] private float slideTiltAngle = 45f; // tilt angle
    [SerializeField] private float tiltSpeed = 10f; // lerp speed

    private Rigidbody rb;

    private CameraSelector cameraSelector;

    private CapsuleCollider capsuleCollider;

    private int jumpsRemaining;

    // What's the max amount of fill
    [SerializeField] private float jetPackFuelMax;

    // How fast feul is consumed
    [SerializeField] private float jetPackFuelConsumption;

    // What's pur current feul?
    private float jetPackFuelRemaining;

    // How fast should we jet accelerate?
    [SerializeField] private float jetPackAccel;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the rigidbody from the player
        rb = GetComponent<Rigidbody>();

        // Prevent the rigidbody from rotating
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Turn off natural gravity (we're doing that manually)
        rb.useGravity = false;

        cameraSelector = GetComponent<CameraSelector>();

        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (slideCooldownTimer > 0)
            slideCooldownTimer -= Time.deltaTime;

        // Call different behaviour depending on our current state
        switch (currentState)
        {
            case State.Walk:
                WalkState();
                break;
            case State.Rise:
                RiseState();
                break;
            case State.Fall:
                FallState();
                break;
            case State.Jet:
                JetState();
                break;
            case State.Slide:
                SlideState();
                break;
        }

        if (playerModel != null && currentState != State.Slide)
        {
            playerModel.localRotation = Quaternion.Lerp(playerModel.localRotation, Quaternion.identity, Time.deltaTime * tiltSpeed);
        }
    }

    private void WalkState()
    {
        //Refresh our Remaing Jumps
        jumpsRemaining = jumpsAllowed;

        // Increase our jet feul
        jetPackFuelRemaining = Mathf.Clamp(jetPackFuelRemaining + Time.deltaTime, 0, jetPackFuelMax);

        // Gert a movement direction based on our inputs and camera
        Vector3 inputMovement = GetMovementFromInput();

        // Adjust using our walk speed
        inputMovement *= walkSpeed;

        // Apply gravity, but because we're on the ground, don't "fall" faster than 0
        inputMovement.y = Mathf.Clamp(rb.linearVelocity.y - gravity * Time.deltaTime, 0f, float.PositiveInfinity);

        // Apply that movement to the rigidbody
        rb.linearVelocity = inputMovement;

        // If we're no longer on the ground (e.g we walk off a cliff)
        if (!IsGrounded())
        {
            // We should be considered falling
            currentState = State.Fall;
        }
        else  // If we ARE on the ground
        {
            // If we press jump
            if (Input.GetButtonDown("Jump"))
            {
                RiseAtSpeed(jumpPower);
            }
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                StartSlide(); // Change to Sliding
            }
        }
    }

    private void RiseState()
    {
        Vector3 inputMovement = GetMovementFromInput();
        inputMovement *= walkSpeed;

        // No clamp here because we're in the air, it should be possible to fall
        inputMovement.y = rb.linearVelocity.y - gravity * Time.deltaTime;

        rb.linearVelocity = inputMovement;

        // If our linear velocity y is less than 0, we are falling, so we should change state
        if (rb.linearVelocity.y < 0)
        {
            // If the jump button is being held...
            if (Input.GetButton("Jump") && jetPackFuelRemaining > 0)
            {
                currentState = State.Jet;
            }
            else
            {
                currentState = State.Fall;
            }
                
        }

        TryToJump();
    }

    private void FallState()
    {
        Vector3 inputMovement = GetMovementFromInput();
        inputMovement *= walkSpeed;

        // No clamp here because we're in the air, it should be possible to fall
        inputMovement.y = rb.linearVelocity.y - gravity * Time.deltaTime;

        rb.linearVelocity = inputMovement;

        TryToJump();

        // If we failed to jump, we're still in the fall state... so we can use that to check if we should jet
        if (currentState != State.Fall)
        {
            // If we DID jump, we should stop early
            return; // aka, 'stop here'
        }

        if (Input.GetButton("Jump"))
        {
            currentState = State.Jet;
        }
        else if (IsGrounded())
        {
            currentState = State.Walk;
        }

    }

    private void JetState()
    {
        // Reduce our current feul
        jetPackFuelRemaining -= jetPackFuelConsumption * Time.deltaTime;

        // Just like other get our input movement
        Vector3 inputMovement = GetMovementFromInput();
        inputMovement *= walkSpeed;

        // Add our jetPeck accel to our upward speed
        inputMovement.y = rb.linearVelocity.y + jetPackAccel * Time.deltaTime;

        rb.linearVelocity = inputMovement;

        // Exit the state if we erlease Jump or run out of feul
        if (Input.GetButtonUp("Jump") || jetPackFuelRemaining <= 0)
        {
            currentState = State.Rise;
        }
    }

    private void SlideState()
    {
        slideTimer += Time.deltaTime;

        // adds speed boost to walkSpeed
        Vector3 slideDir = rb.linearVelocity.normalized * (walkSpeed + slideSpeedBoost);
        slideDir.y = rb.linearVelocity.y;
        rb.linearVelocity = slideDir;


        // tilt the child mdel smoothly
        if (playerModel != null)
        {
            Quaternion targetRotation = Quaternion.Euler(slideTiltAngle, playerModel.localEulerAngles.y, playerModel.localEulerAngles.z);
            playerModel.localRotation = Quaternion.Lerp(playerModel.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
        }

        // endslide doohicky
        if (slideTimer >= slideDuration || !IsGrounded())
        {
            currentState = State.Walk;
            slideTimer = 0f;
            slideCooldownTimer = slideCooldown;

           
        }
    }

    /// <summary>
    /// Trigger a jump and the Rise state if the player is grounded, or has jumps remaining
    /// </summary>
    private void TryToJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if(IsGrounded() || jumpsRemaining > 0)
            {
                RiseAtSpeed(jumpPower);
            }
        }
    }

    /// <summary>
    /// Make the player ascend at the given speed, and put them in the Rise state
    /// </summary>
    /// <param name="speed"></param>
    private void RiseAtSpeed(float speed)
    {
        // Chaneg onlyour vertical velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, speed, rb.linearVelocity.z);

        // Change our state to 'Rise'
        currentState = State.Rise;

        // Take away 1 possible jump
        jumpsRemaining--;    // same as 'jumpsremaining -= 1
    }


    //
    private void StartSlide()
    {
        // wait for cooldown completeion
        if (slideCooldownTimer <= 0)
        {
            currentState = State.Slide;
            slideTimer = 0f;
            slideCooldownTimer = slideCooldown;
        }
    }

    /// <summary>
    /// Get the current directional inputs, and convert that to movement, based on camera direction
    /// </summary>
    /// <returns></returns>
    private Vector3 GetMovementFromInput()
    {
        // Construct a new, local Vector2 to hold our inputs
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Convert that to 3D space (y to z)
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);

        // Get the current Camera Treansform 
        Transform cameraTransform = cameraSelector.GetCameraTransform();

        // Transform our movement direction based on the camera
        moveDirection = cameraTransform.TransformDirection(moveDirection);

        // Return that result
        return moveDirection;
    }

    private bool IsGrounded()
    {

        return Physics.Raycast(transform.position, Vector3.down, capsuleCollider.height / 2f + 0.01f, groundLayer);
    }


}
