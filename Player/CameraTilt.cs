using UnityEngine;

public class CameraTilt : MonoBehaviour
{
    public float tiltAngle = 4f; // Maximum tilt angle
    public float tiltSpeed = 5f;  // Speed of tilting

    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        float tilt = Input.GetAxis("Horizontal") * tiltAngle;
        Quaternion targetRotation = Quaternion.Euler(originalRotation.eulerAngles.x, originalRotation.eulerAngles.y, -tilt);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
    }
}
