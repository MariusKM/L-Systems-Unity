using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyOverride : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;
    Joint joint;
    bool isBroken;
    public float gravityScale = 0.00001f;
    public static float globalGravity = -9.81f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
       // if (isBroken) return;
        Vector3 tempVel = rb.velocity;
        Vector3 tempAngVel = rb.angularVelocity;
        rb.angularVelocity = new Vector3(0, tempAngVel.y, 0);
        rb.velocity = new Vector3(tempVel.x, tempVel.y, 0);
  
    }


    void OnJointBreak(float breakForce)
    {
        Debug.Log("A joint has just been broken!, force: " + breakForce);
        isBroken = true;
        rb.angularDrag = 0.1f;
        rb.drag = 0.1f;
        rb.useGravity = true;
    }

}
