using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemAnimator : MonoBehaviour
{
    List<TransformInfo> myAnimatedTransforms = new List<TransformInfo>();
    TransformInfo[] myTransforms;
    [Range(0.01f,10)]
    public float WindStrength = 1;
    float windScaleFac;
    [Range(0, 1)]
    public float minRand = 0.5f;
    [Range(0, 1)]
    public float maxRand = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        myTransforms =  GetComponentsInChildren<TransformInfo>();

        foreach (TransformInfo ti in myTransforms)
        {
            if (ti.nodeLength <= 0 || ti.tag == "Leaf") return;
            if (ti.nodeLength <= 0 || ti.tag == "Leaf") return;
            if (ti.animationID == 0)
            {
                float rand = Random.Range(0.0f, 1.0f);
                if (rand < 0.5f) ti.animationID = 1;
                else ti.animationID = 2;
            }
            myAnimatedTransforms.Add(ti);
        }


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        windScaleFac = 100 / (WindStrength*10);
        foreach (TransformInfo ti in myAnimatedTransforms)
        {
            RigidBodyOverride RO = ti.GetComponent<RigidBodyOverride>();
            if (RO)
            {
                if (RO.isBroken) return; 
            }
           
            float angle = 0;
            if (ti.param != 0)
            {
                 angle = (3 / (ti.param + 1)) *((ti.animationID ==1) ? Mathf.Sin(Time.time): Mathf.Cos(Time.time)) / windScaleFac;
            }
            else
            {
                 angle = ((ti.animationID == 1) ? Mathf.Sin(Time.time) : Mathf.Cos(Time.time)) / windScaleFac;
            }
          
            
            ti.transform.Rotate(Vector3.up * (angle * Random.Range(minRand, maxRand)));
        }
    }
}
