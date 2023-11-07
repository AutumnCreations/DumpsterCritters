using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;
    private Quaternion initialRotation;

    void Start()
    {
        mainCamera = Camera.main;
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // Calculate the rotation needed to undo the camera's rotation
        Quaternion counterRotation = Quaternion.Euler(30, 45, 0);

        // Apply this rotation to the game object
        transform.rotation = counterRotation * initialRotation;
    }
}
