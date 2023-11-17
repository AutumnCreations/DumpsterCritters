using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
    }

    void LateUpdate()
    {
        float newYRotation = initialRotation.y + (transform.parent.eulerAngles.y * -1);
        Vector3 newRotation = new Vector3(initialRotation.x, newYRotation, initialRotation.z);

        transform.localEulerAngles = newRotation;
    }
} 
