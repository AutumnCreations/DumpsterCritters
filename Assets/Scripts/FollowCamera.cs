using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    Vector3 offset;
    PlayerController player;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        offset = transform.position;
    }
    void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }
}
