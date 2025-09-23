using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleController : MonoBehaviour {
    [Header("Move / Look")]
    public float moveSpeed = 6f;
    public float mouseSensitivity = 3f;
    public float sprintSpeed = 2f;
    public Transform cameraHolder;

    [Header("Jump / Gravity")]
    public float jumpHeight = 2.2f;
    public float gravity = -30f;

    [Header("Crouch")]
    public KeyCode crouchKey = KeyCode.C;
    public float crouchHeight = 1.2f;        // hitbox height when crouched
    public float crouchCamY = 1.0f;          // cameraHolder local Y when crouched
    public float standCamY = 1.6f;           // cameraHolder local Y when standing
    public float crouchSpeedMult = 0.6f;     // slower while crouched
    public float crouchLerp = 12f;  

    CharacterController cc;
    Vector3 velocity;   // only Y used for gravity/jump
    float xRot = 0f;    // camera pitch
    float standHeight;
    Vector3 standCenter;

    void Awake() {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        crouchKey = KeyCode.C;
        standHeight = cc.height;
        standCenter = cc.center;
        cameraHolder.localPosition = new Vector3(
            cameraHolder.localPosition.x, standCamY, cameraHolder.localPosition.z);
    }

    bool HasHeadroom() {
        var b = cc.bounds;
        float radius = cc.radius * 0.95f;
        Vector3 bottom = new(b.center.x, b.min.y + radius + 0.05f, b.center.z);
        Vector3 topStand = new(b.center.x, b.min.y + standHeight - radius - 0.05f, b.center.z);
        return !Physics.CheckCapsule(bottom, topStand, radius, ~0, QueryTriggerInteraction.Ignore);
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
        Vector3 inputDir = (transform.right * h + transform.forward * v).normalized;
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : 1f);
        Vector3 horizontal = inputDir * speed;

        // --- Ground snap using built-in flag ---
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f; // tiny downward force to keep grounded

        if (cc.isGrounded)
        { 
            Debug.Log("GROUNDED");
        }

        // --- Jump (Space) ---
        if (cc.isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (Input.GetButtonDown("Jump"))
        { 
            Debug.Log("JUMMPING ATTEMPT");
        }

        // --- Crouch ---
        bool wantCrouch = Input.GetKey(crouchKey);
        float targetHeight = wantCrouch ? crouchHeight : standHeight;
        Vector3 targetCenter = wantCrouch 
            ? new Vector3(0f, targetHeight * 0.5f - cc.skinWidth, 0f) 
            : standCenter;
        float targetCamY = wantCrouch ? crouchCamY : standCamY;

        // smooth transitions
        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * crouchLerp);
        cc.center = Vector3.Lerp(cc.center, targetCenter, Time.deltaTime * crouchLerp);
        Vector3 camLocal = cameraHolder.localPosition;
        camLocal.y = Mathf.Lerp(camLocal.y, targetCamY, Time.deltaTime * crouchLerp);
        cameraHolder.localPosition = camLocal;

        // crouch slows speed
        if (cc.isGrounded && Input.GetKey(crouchKey))
        {
            speed *= wantCrouch ? crouchSpeedMult : 1f;
            horizontal = inputDir * speed;
        }
        
        if (Input.GetKey(crouchKey))
        {
            Debug.Log("Using crouchKey: " + crouchKey);
        }


        // --- Gravity ---
        velocity.y += gravity * Time.deltaTime;

        Vector3 final = (horizontal + new Vector3(0f, velocity.y, 0f)) * Time.deltaTime;
        cc.Move(final);
    }
}




