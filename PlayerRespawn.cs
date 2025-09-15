using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    CharacterController cc;
    Rigidbody rb;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    public void RespawnAt(Transform point)
    {
        if (cc) cc.enabled = false;

        transform.SetPositionAndRotation(point.position, point.rotation);

        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (cc) cc.enabled = true;
    }
}
