using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    // How fast the camera should move
    public float sensitivity;

    // The player transform
    public Transform playerTransform;

    // How low are we allowed to look?
    public float verticalRotationMin;

    // How high are we allowed to look?
    public float verticalRotationMax;

    // What is our current camera direction?
    private float currentHorizontalRotation;
    private float currentVerticalRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Store our initial rotations
        currentHorizontalRotation = transform.localEulerAngles.y;
        currentVerticalRotation = transform.localEulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        // Adjust our current horizontal rotation
        currentHorizontalRotation += Input.GetAxis("Mouse X") * sensitivity;

        // Adjust our vertical rotation
        // here, -= because of screen-space (down is bigger) to Unity space (up is bigger)
        currentVerticalRotation -= Input.GetAxis("Mouse Y") * sensitivity;

        // Constrain (aka clamp) our vertical rotation
        currentVerticalRotation = Mathf.Clamp(currentVerticalRotation, verticalRotationMin, verticalRotationMax);

        // Apply the rotation
        transform.localEulerAngles = new Vector3(currentVerticalRotation, currentHorizontalRotation);

        // Snap to the player's position
        transform.position = playerTransform.position;
    }
}
