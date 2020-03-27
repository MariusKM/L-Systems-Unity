using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class LSystemSpawner : MonoBehaviour
{

    BoxCollider spawnArea;
    public int amountX, amountZ;
    Bounds spawnBounds;

    GameObject[] generatedObjects;
    public LSystemsGenerator[] generatorTemplate;



    public void Init()
    {
        spawnArea = GetComponent<BoxCollider>();
        spawnArea.isTrigger = true;
        spawnBounds = spawnArea.bounds;
        generatedObjects = new GameObject[amountX * amountZ];

        Spawn();
    }

    void generateFractals()
    {
       
        for (int i = 0; i < generatedObjects.Length; i++)
        {
            int indexer = Random.Range(0, generatorTemplate.Length);
            Debug.Log(indexer);
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
    // Update is called once per frame
    public void Spawn()
    {
        float sizeX = spawnBounds.size.x;
        float sizeZ = spawnBounds.size.z;
        generateFractals();

        float stepX = sizeX / (amountX-1);
        float stepZ = sizeZ / (amountZ-1);

        Vector3 bottomLeft = new Vector3(-sizeX / 2, 0, -sizeZ/2 );

        for (int z = 0; z < amountZ; z++)
        {
            for (int x = 0; x < amountX; x++)
            {
                Vector3 spawnPos = bottomLeft + new Vector3(stepX * x, 0, stepZ * z);
                Debug.Log(spawnPos);
                Debug.Log(z * amountZ + x);
              //  Debug.Log(stepZ * z);
               // Debug.Log((stepX * x));
                generatedObjects[z*amountZ+ x].transform.localPosition = spawnPos;
            }
        }



    }
}
