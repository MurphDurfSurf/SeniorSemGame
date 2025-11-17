using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRecoil : MonoBehaviour
{

    public enum RecoilMode { AirOnly, GroundOnly, Always }

    [Header("Tuning")]
    public float defaultHorizontalForce = 6f;   // planar impulse (m/s)
    public float defaultVerticalForce   = 0f;   // upward impulse (m/s)
    public float horizontalDecayPerSec  = 6f;   // fade speed
    public float verticalDecayPerSec    = 6f;   // fade speed
    public float maxHorizontalSpeed     = 12f;  // clamp planar recoil
    public float maxVerticalSpeed       = 12f;  // clamp vertical recoil
    public bool clampPlanar             = true;
    
    [Header("Behavior")]
    public RecoilMode mode = RecoilMode.AirOnly; // default: only while airborne
    public bool respectHorizontalOnly = false;   // if true, ignore any Y in Kick dir

    CharacterController cc;
    Vector3 planarVel;  // XZ recoil (world space)
    float verticalVel;  // Y recoil (m/s)

    void Awake() => cc = GetComponent<CharacterController>();

    bool CanApplyNow()
    {
        return mode switch
        {
            RecoilMode.AirOnly   => !cc.isGrounded,
            RecoilMode.GroundOnly=>  cc.isGrounded,
            RecoilMode.Always    =>  true,
            _ => true
        };
    }


    // Kick in a direction with optional strengths. Direction can include Y unless respectHorizontalOnly is true.
    public void Kick(Vector3 direction, float? horizontalStrength = null, float? verticalStrength = null)
    {
        if (!CanApplyNow()) return;

        var dir = direction.normalized;
        float h = horizontalStrength ?? defaultHorizontalForce;
        float v = verticalStrength   ?? defaultVerticalForce;

        // Split into planar + vertical
        Vector3 planarDir = new Vector3(dir.x, 0f, dir.z);
        if (planarDir.sqrMagnitude > 0.0001f) planarDir.Normalize();

        if (respectHorizontalOnly)
        {
            // force vertical to use only v parameter
            planarVel += planarDir * h;
        }
        else
        {
            // allow dir.y to contribute to vertical kick, scaled by h
            planarVel += planarDir * h;
            v += Mathf.Max(0f, dir.y) * h; // only add upward from dir.y
        }

        verticalVel += v;

        if (clampPlanar) planarVel = Vector3.ClampMagnitude(planarVel, maxHorizontalSpeed);
        verticalVel = Mathf.Clamp(verticalVel, -maxVerticalSpeed, maxVerticalSpeed);
    }

     
    // Convenience: classic "blast jump" (back + up). 
    // e.g., Blast(-forward, 8f, 10f, RecoilMode.Always)
    
    public void Blast(Vector3 backDir, float horizontalForce, float upwardForce, RecoilMode useMode = RecoilMode.Always)
    {
        var prev = mode;
        mode = useMode;
        Kick(backDir, horizontalForce, upwardForce);
        mode = prev;
    }

    void LateUpdate()
    {
        // Build movement this frame from recoil velocities.
        Vector3 move = planarVel + Vector3.up * verticalVel;

        if (move.sqrMagnitude > 0f)
        {
            cc.Move(move * Time.deltaTime);
        }

        // Decay
        if (planarVel.sqrMagnitude > 0f)
            planarVel = Vector3.MoveTowards(planarVel, Vector3.zero, horizontalDecayPerSec * Time.deltaTime);

        if (verticalVel != 0f)
        {
            // If grounded and vertical velocity is downward/small, kill it so we don't glue to floor.
            if (cc.isGrounded && verticalVel <= 0f) verticalVel = 0f;
            else verticalVel = Mathf.MoveTowards(verticalVel, 0f, verticalDecayPerSec * Time.deltaTime);
        }
    }
}
