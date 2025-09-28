using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public float sensitivity;

    public float verticalRotationMin;
    public float verticalRotationMax;

    public Transform playerTransform;

    // How far away the camera should be from the player, with no obstructions
    public float cameraZoomIdeal;

    // Define what physics layer to use for wall-checks
    public LayerMask blockingLayer;

    // This is the first child object, responsible for up/down rotation
    private Transform boomTransform;

    // This is the camera object, responsible for zooming in/out (along Z-axis)
    private Transform cameraTransform;

    private float currentHorizontalRotation;
    private float currentVerticalRotation;

    // How zoomed in are we actually right now, including obstructions?
    private float cameraZoomCurrent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the first child object of this object
        boomTransform = transform.GetChild(0);

        // Get the first child of THAT object (should be our camera)
        cameraTransform = boomTransform.GetChild(0);

        // Set our starting rotation
        currentHorizontalRotation = transform.localEulerAngles.y;

        // For our vertical rotation, that's being managed by the boom
        currentVerticalRotation = boomTransform.localEulerAngles.x;

        cameraZoomCurrent = cameraZoomIdeal;
    }

    // Update is called once per frame
    void Update()
    {
        // Change the desired rotation using inputs
        currentHorizontalRotation += Input.GetAxis("Mouse X") * sensitivity;
        currentVerticalRotation -= Input.GetAxis("Mouse Y") * sensitivity;

        // Clamp the rotation where needed
        currentVerticalRotation = Mathf.Clamp(currentVerticalRotation, verticalRotationMin, verticalRotationMax);

        //Apply the rotation to our various transforms
        transform.localEulerAngles = new Vector3(0, currentHorizontalRotation); // .z will be 0

        boomTransform.localEulerAngles = new Vector3(currentVerticalRotation, 0);

        // Snap to the player
        transform.position = playerTransform.position;

        // Get the direction from the player to the camera - direction from A to B is 'B - A'
        Vector3 directionToCamera = cameraTransform.position - transform.position;

        // 'out' gets extra information out of a function, more than just 'return' does
        // If we hit something with a raycast towards the camera...
        if (Physics.Raycast(transform.position, directionToCamera, out RaycastHit hit, cameraZoomIdeal, blockingLayer))
        {
            // Adjust our zoom based on where we hit
            cameraZoomCurrent = hit.distance;
        }
        else
        {
            // Else, go to our ideal zoom
            cameraZoomCurrent = cameraZoomIdeal;
        }

        // Update our camera transform's z-position, to change the zoom
        cameraTransform.localPosition = new Vector3(0, 0, -cameraZoomCurrent);
    }
}
