﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class LSystemSpawner : MonoBehaviour
{

    BoxCollider spawnArea;
    public int amountX, amountZ;
    public float randomize = 0.0f;
    Bounds spawnBounds;
    private bool isRandom;
    GameObject[] generatedObjects;
    public LSystemsGenerator[] generatorTemplate;



    public void Init()
    {
        if (randomize > 0.0f) isRandom = true;
        spawnArea = GetComponent<BoxCollider>();
        spawnArea.isTrigger = true;
        spawnBounds = spawnArea.bounds;
        generatedObjects = new GameObject[(amountX) * (amountZ)];
        Spawn();
    }

    void generateFractals()
    {
        for (int i = 0; i < generatedObjects.Length; i++)
        {
            int indexer = Random.Range(0, generatorTemplate.Length);
            generatorTemplate[indexer].Init();
            generatorTemplate[indexer].initColliders();
            generatorTemplate[indexer].setUpPhysics();
            generatedObjects[i] = generatorTemplate[indexer].generatedObject;
            generatedObjects[i].transform.parent = this.transform;
        }
    }

    void cacheChildren()
    {
        Transform[] generatedObjectsT = GetComponentsInChildren<Transform>();
        generatedObjects = new GameObject[generatedObjectsT.Length];
        for (int i = 1; i < generatedObjectsT.Length; i++)
        {
            generatedObjects[i] = generatedObjectsT[i].gameObject;
        }
    }
    public void Clear()
    {
        cacheChildren();
        for (int i = 1; i < generatedObjects.Length; i++)
        {  
            DestroyImmediate(generatedObjects[i]);
        }
        generatedObjects = null;
    }




    public void fitToGroundPlane()
    {
        foreach (GameObject g in generatedObjects)
        {
            RaycastHit hit;
            Collider col = g.GetComponent<Collider>();
            Ray ray = new Ray(g.transform.position, Vector3.down);
            if (Physics.Raycast(ray, out hit, 10000))
            {
                g.transform.position = hit.point;
            }
                //col.Raycast(ray, out hit, 10);
           
        }

    }
    // Update is called once per frame
    public void Spawn()
    {
        float sizeX = spawnBounds.size.x;
        float sizeZ = spawnBounds.size.z;
        generateFractals();

        float stepX = sizeX / (amountX-1);
        float stepZ = sizeZ / (amountZ-1);
 

        Vector3 bottomLeft = new Vector3(-sizeX / 2.0f, 0, -sizeZ/2.0f );
        for (int i = 0, z = 0; z <amountZ; z++)
        {
            for (int x = 0; x <amountX; x++,i++)
            {
                Vector3 spawnPos = bottomLeft;
                if (isRandom)
                {
                    Vector3 randomOffset = Random.onUnitSphere * randomize;
                    Vector3 off = new Vector3(randomOffset.x, 0, randomOffset.z);

                    spawnPos += off +  new Vector3(stepX * x, 0, stepZ * z);

                }
                else
                {
                    spawnPos += new Vector3(stepX * x, 0, stepZ * z);
                }
          
      
     
                generatedObjects[i].transform.localPosition = spawnPos;
            
            }
        }



    }
}
