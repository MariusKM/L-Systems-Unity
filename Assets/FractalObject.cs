using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalObject : MonoBehaviour
{
    bool hasWind = true;
    public float windforce = 1;
    public Vector3 windDir = Vector3.zero;
    float ySeed;
    private Rigidbody[] myRBs;
    public Vector3 windForce;
    // Start is called before the first frame update
    void Start()
    {
        myRBs = GetComponentsInChildren<Rigidbody>();

        ySeed = Random.seed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        float noise = Mathf.PerlinNoise(Time.time, ySeed);
        windForce = (noise*windforce) * windDir;
        foreach(Rigidbody r in myRBs)
        {
                 r.AddForce(windForce, ForceMode.Acceleration);
        }
    
    }
}
