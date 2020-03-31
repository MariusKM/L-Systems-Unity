using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyOverride : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;
    Joint joint;
    [HideInInspector]
    public bool isBroken;
    private float lerpVal;
    private float fadeSpeed = 0.001f;
    private Color originalCol;
    private LineRenderer LR;
    bool isLeaf = false;


    void Start()
    {
        if (tag == "Leaf") isLeaf = true;
        rb = GetComponent<Rigidbody>();
        LR = GetComponent<LineRenderer>();
        originalCol = LR.material.color;
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // if (isBroken) return;
        Vector3 tempVel = rb.velocity;
        Vector3 tempAngVel = rb.angularVelocity;
        rb.angularVelocity = new Vector3(0, tempAngVel.y, 0);
 
        rb.velocity = new Vector3(tempVel.x, tempVel.y, 0);
        if (isLeaf)
        {
           // transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, 0);
        }
        if (isBroken) fadeOut();

    }

    void fadeOut()
    {
        if (lerpVal > 1.0f) Destroy(gameObject);
        lerpVal += fadeSpeed;
        float lerpAlpha = Mathf.Lerp(originalCol.a, 0, lerpVal);
        Color lerpCol = originalCol;
        lerpCol.a = lerpAlpha;
        LR.material.color = lerpCol;
    }

    void OnJointBreak(float breakForce)
    {
        Debug.Log("A joint has just been broken!, force: " + breakForce);
        if (transform.parent)
        {
            if (!transform.parent.GetComponent<FractalWindZone>())
            {
                isBroken = true;
                transform.parent = null;
            }
        }
       
        rb.angularDrag = 0.1f;
        rb.drag = 0.1f;
        rb.useGravity = true;
        RigidBodyOverride[] rbs = GetComponentsInChildren<RigidBodyOverride>();
        if (rb != null)
        {
            foreach (RigidBodyOverride rO in rbs)
            {
                rO.isBroken = true;
            }
        }
    }

}
