using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleController : MonoBehaviour {
    [Header("Move / Look")]
    public float moveSpeed = 6f;
    public float mouseSensitivity = 3f;
    public float sprintSpeed = 3f;
    public Transform cameraHolder;

    [Header("Jump / Gravity")]
    public float jumpHeight = 2.2f;
    public float gravity = -30f;

    CharacterController cc;
    Vector3 velocity;   // only Y used for gravity/jump
    float xRot = 0f;    // camera pitch

    void Awake() {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        // --- Mouse look ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);                  // yaw on player
        xRot = Mathf.Clamp(xRot - mouseY, -80f, 80f);      // pitch on camera
        cameraHolder.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        // --- Movement (WASD) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * h + transform.forward * v).normalized;
        cc.Move(move * moveSpeed * Time.deltaTime);

        // -- Sprinting --
        if (Input.GetKey(KeyCode.LeftShift))
            cc.Move(move * (moveSpeed + sprintSpeed) * Time.deltaTime);

        // --- Ground snap using built-in flag ---
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f; // tiny downward force to keep grounded

        // --- Jump (Space) ---
        if (cc.isGrounded && Input.GetButtonDown("Jump")) {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- Gravity ---
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}



