using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalWindZone : MonoBehaviour
{

    bool hasWind = true;
    public float windforce = 1;
    public float lerpSpeed = 0.1f;
    public Vector3 windDir = Vector3.right;
    float ySeed;
    private Rigidbody[] myRBs;
    public Vector3 windForce;
    bool hasTarget = false;
    float targetVel, oldVel;
    float lerpVal;
    // Start is called before the first frame update
    void Start()
    {

        cacheColliders();
        ySeed = Random.seed;
    }

    void cacheColliders()
    {
        myRBs = GetComponentsInChildren<Rigidbody>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (lerpVal > 1.0f) hasTarget = false;
        if (!hasTarget)
        {
            lerpVal = 0;
            oldVel = targetVel;
            targetVel = Mathf.PerlinNoise(Time.time, ySeed) * Random.Range(-1.0f, 1.01f);
            hasTarget = true;
        }

        lerpVal += lerpSpeed;

        float noise = Mathf.Lerp(oldVel, targetVel, lerpVal);
        windForce = (noise * windforce) * windDir;
        foreach (Rigidbody r in myRBs)
        {
            if (r != null) r.AddForce(windForce, ForceMode.Acceleration);

        }


    }
}
