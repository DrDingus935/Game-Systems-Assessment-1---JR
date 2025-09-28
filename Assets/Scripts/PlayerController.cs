using UnityEngine;

// an [Attribute] helps define how this script should work
[RequireComponent(typeof(Rigidbody))]
// RequireComponent tells this script to check the objects for important components
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed;

    public float jumpPower;

    public float gravity;

    public Vector2 currentInput;

    public Vector3 movement;

    private CameraSelector cameraSelector;

    // private = no one else can see this
    private Rigidbody rb;

    private CapsuleCollider capsuleCollider;

    void Start()
    {
        // Get the component reference and save it
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        cameraSelector = GetComponent<CameraSelector>();
    }


    void Update()
    {
        // Get our inputs into the variable we made for them
        currentInput.x = Input.GetAxis("Horizontal");
        currentInput.y = Input.GetAxis("Vertical");

        // Update our movement vector based on our inputs
        movement = new Vector3(currentInput.x, 0, currentInput.y);

        // Apply our speed to our movement direction
        movement *= walkSpeed;  // same as 'movement = movement * walkSpeed'

        // If we have a camera to follow...
        if (cameraSelector.GetSelection() == CameraSelector.Selection.FirstPerson) // 'null' means nothing/empty
        {
            // Match left/right rotation to the camera
            transform.localEulerAngles = new Vector3(0, cameraSelector.GetCameraTransform().localEulerAngles.y);

            // Translate or change the movement from global forward, to local forward
            movement = transform.TransformDirection(movement);
        }
        else
        {
            movement = cameraSelector.GetCameraTransform().TransformDirection(movement);
        }

        movement.y = rb.linearVelocity.y - gravity * Time.deltaTime;

        // Give the movement to the rigidbody
        rb.linearVelocity = movement;

        // Check if we're on the ground
        if (IsGrounded())
        {
            // Check if the jump key is pressed
            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }
        }
    }

    // Trigger upwards momentum based on our jump power
    private void Jump()
    {
        // Copy the vector3 currently tracking our velocity 
        // get a local variable (can only be used in this method)
        Vector3 currentVelocity = rb.linearVelocity;

        // Change the y-value of that vector3
        currentVelocity.y = jumpPower;

        // Apply the copied vector back to our velocity
        rb.linearVelocity = currentVelocity;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, capsuleCollider.height / 2f + 0.01f);
    }
}
