using UnityEngine;

public class CameraSelector : MonoBehaviour
{
    public enum Selection
    {
        FirstPerson,
        ThirdPerson
    }

    // Track our current selection
    public Selection selection;

    private FirstPersonCamera firstPersonCamera;
    private ThirdPersonCamera thirdPersonCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get references to our cameras, by looking for the scripts in the scene
        // 'FindFirstObjectByType' will search the active scene for the type given.
        firstPersonCamera = FindFirstObjectByType<FirstPersonCamera>();
        thirdPersonCamera = FindFirstObjectByType<ThirdPersonCamera>();

        SelectCamera(selection);
    }

    /// <summary>
    /// Change the current camera based on the input setting.
    /// </summary>
    /// <param name="newSelection"></param>
    public void SelectCamera(Selection newSelection)
    {
        selection = newSelection;

        // Enable/Disable the game objects of the cameras
        switch (selection)
        {
            case Selection.FirstPerson:
                firstPersonCamera.gameObject.SetActive(true);
                thirdPersonCamera.gameObject.SetActive(false);
                break;
            case Selection.ThirdPerson:
                thirdPersonCamera.gameObject.SetActive(true);
                firstPersonCamera.gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Return the root transform of the currently used camera.
    /// </summary>
    /// <returns></returns>
    public Transform GetCameraTransform()
    {
        // 'return' replaces 'break' here, because we don't need to go on past the switch statement
        switch (selection)
        {
            case Selection.FirstPerson:
                return firstPersonCamera.transform;
            case Selection.ThirdPerson:
                return thirdPersonCamera.transform;
            // 'default' - if the switch statement does not match with anything else, do this
            default:
                return null;    //null means nothing
        }
    }

    /// <summary>
    /// Return the currently selected camera type
    /// </summary>
    /// <returns></returns>
    public Selection GetSelection()
    {
        return selection;
    }
}
