using UnityEngine;
using System.Collections;

public class GunTest : MonoBehaviour
{
    public float maxDistance = 50f; // How far our 'shot' will go
    public Color lineColor = Color.red;
    public LineRenderer lineRenderer;   // Optional (for Game view)
    public float flashTime = 1f;     // How long the line shows
    public LayerMask hitMask = ~0;

    private Collider selfCollider;

    void Awake()
    {
        // grab the collider on the same object (if it has one)
        selfCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Start at this object's position
            Vector3 start = transform.position;
            Vector3 dir = transform.forward;

            Vector3 end = start + dir * maxDistance;
            if (Physics.Raycast(start, dir, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != selfCollider)   // ignore if it’s “me”
                {
                    end = hit.point;
                }
            }

            Debug.DrawLine(start, end, lineColor, flashTime);
            if (lineRenderer != null) StartCoroutine(FlashLine(start, end));

            var recoil = GetComponentInParent<GunRecoil>();
            if (recoil)
            {
                // Example 1: simple air knockback
                recoil.Kick(-transform.forward, 6f, 0f);

                // Example 2: rocket jump (works on ground)
                // recoil.Blast(-transform.forward, 8f, 10f, RecoilMotor.RecoilMode.Always);
            }


        }
    }
    
    IEnumerator FlashLine(Vector3 a, Vector3 b)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, a);
        lineRenderer.SetPosition(1, b);
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(flashTime);
        lineRenderer.enabled = false;
    }

}
