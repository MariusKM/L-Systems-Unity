using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyOverride : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 tempVel = rb.velocity;
        Vector3 tempAngVel = rb.angularVelocity;
        rb.angularVelocity = new Vector3(0, tempAngVel.y, 0);
        rb.velocity = new Vector3(tempVel.x, tempVel.y, 0);
    }
}
