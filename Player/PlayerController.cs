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

    [Header("Camera Tilt")]
    public float tiltAngle = 4f;
    public float tiltSpeed = 5f;
    public float airTiltMultiplier = 1.5f;
    private float currentTilt = 0f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] jumpSounds;

    [Header("Landing Bounce")]
    public float landingDipAmount = 0.5f;     // How far down the camera dips
    public float landingBounceAmount = 0.2f;  // How much it bounces back up
    public float landingDipSpeed = 12f;       // Speed of the dip effect
    public float landingBounceSpeed = 8f;     // Speed of the bounce recovery
    private float landingBounceY = 0f;        // Current bounce offset
    private bool isLanding = false;
    private bool wasInAir = false;

    [Header("Jump / Gravity")]
    public float jumpHeight = 2.2f;
    public float gravity = -30f;
    private Vector3 lastPosition;
    private float stuckThreshold = 0.05f;  // How much movement is considered "stuck"
    private bool wasJumping = false;
    private float lastJumpVelocity;        // Track jump velocity changes

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
        
        // Calculate tilt based on movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float targetTilt = horizontalInput * tiltAngle;
        if (!cc.isGrounded) {
            targetTilt *= airTiltMultiplier; // More pronounced tilt in air
        }
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        
        // Apply both pitch and tilt while preserving yaw
        cameraHolder.localRotation = Quaternion.Euler(xRot, 0f, -currentTilt);

        // --- Movement (WASD) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = (transform.right * h + transform.forward * v).normalized;
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : 1f);
        Vector3 horizontal = inputDir * speed;

        // --- Ground snap using built-in flag ---
        if (cc.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f; // tiny downward force to keep grounded

            // Detect landing
            if (wasInAir && !isLanding)
            {
                isLanding = true;
                StartCoroutine(HandleLandingBounce());
            }
        }
        
        // Track air state
        wasInAir = !cc.isGrounded;

        // Store current position for next frame's comparison
        Vector3 preMovementPosition = transform.position;

        // --- Jump (Space) ---
        if (Input.GetButtonDown("Jump") && cc.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            wasJumping = true;
            SoundFXManager.instance.PlayRandomSound(jumpSounds, transform.position, 0.5f); //JUMP SFX
            lastPosition = transform.position;
            lastJumpVelocity = velocity.y;
        }

        // Apply gravity first
        velocity.y += gravity * Time.deltaTime;

        // Move the character controller
        Vector3 movement = horizontal + new Vector3(0f, velocity.y, 0f);
        cc.Move(movement * Time.deltaTime);

        // Check for head collision after movement
        if (velocity.y > 0 && wasJumping)
        {
            // Compare positions after movement has been applied
            float actualYMovement = Mathf.Abs(transform.position.y - preMovementPosition.y);
            float expectedYMovement = velocity.y * Time.deltaTime;
            
            // If we moved significantly less than expected, we hit something
            if (actualYMovement < expectedYMovement * 0.5f)
            {
                Debug.Log($"Hit ceiling. Expected movement: {expectedYMovement}, Actual: {actualYMovement}");
                velocity.y = 0f;
                wasJumping = false;
            }
        }
        else if (velocity.y <= 0 && wasJumping)
        {
            wasJumping = false;
        }

        // --- Crouch ---
        bool wantCrouch = Input.GetKey(crouchKey);

        float targetHeight = wantCrouch ? crouchHeight : standHeight;
        Vector3 targetCenter = wantCrouch
            ? new Vector3(0f, targetHeight * 0.5f - cc.skinWidth, 0f) 
            : standCenter;
        float targetCamY = wantCrouch ? crouchCamY : standCamY;

        // Smooth transitions
        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * crouchLerp);
        cc.center = Vector3.Lerp(cc.center, targetCenter, Time.deltaTime * crouchLerp);
        Vector3 camLocal = cameraHolder.localPosition;
        float targetY = targetCamY + landingBounceY; // Add landing bounce offset
        camLocal.y = Mathf.Lerp(camLocal.y, targetY, Time.deltaTime * crouchLerp);
        cameraHolder.localPosition = camLocal;

        // Handle movement speed
        if (cc.isGrounded && wantCrouch)
        {
            // Normal crouch movement
            speed *= crouchSpeedMult;
            horizontal = inputDir * speed;
        }
        
        if (Input.GetKey(crouchKey))
        {
            Debug.Log("Using crouchKey: " + crouchKey);
        }
    }

    private IEnumerator HandleLandingBounce()
    {
        float elapsed = 0f;
        
        // Initial downward dip
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * landingDipSpeed;
            landingBounceY = Mathf.Lerp(0, -landingDipAmount, elapsed);
            yield return null;
        }
        
        elapsed = 0f;
        float startY = landingBounceY;
        
        // Bounce back up slightly
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * landingBounceSpeed;
            landingBounceY = Mathf.Lerp(startY, landingBounceAmount, elapsed);
            yield return null;
        }
        
        elapsed = 0f;
        startY = landingBounceY;
        
        // Settle back to normal
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * landingBounceSpeed;
            landingBounceY = Mathf.Lerp(startY, 0f, elapsed);
            yield return null;
        }
        
        landingBounceY = 0f;
        isLanding = false;
    }
}




