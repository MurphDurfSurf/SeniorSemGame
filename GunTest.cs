using UnityEngine;
using System.Collections;

public class GunTest : MonoBehaviour
{
    public float maxDistance = 50f; // How far our 'shot' will go
    public Color lineColor = Color.red;
    public LineRenderer lineRenderer;   // Optional (for Game view)
    public float flashTime = 0.06f;     // How long the line shows
    public LayerMask hitMask = ~0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Start at this object's position
            Vector3 start = transform.position;
            Vector3 dir = transform.forward;

            Vector3 end;
            if (Physics.Raycast(start, dir, out RaycastHit hit, maxDistance, hitMask))
                end = hit.point;                  // Stops at wall
            else
                end = start + dir * maxDistance; // No wall

            Debug.DrawLine(start, end, lineColor, flashTime);

            if (lineRenderer != null) StartCoroutine(FlashLine(start, end));

            // Draw the line (only visible in Scene view by default)
            Debug.DrawLine(start, end, lineColor, 1f);
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
