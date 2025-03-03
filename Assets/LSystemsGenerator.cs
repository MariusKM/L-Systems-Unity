﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LSystemsGenerator : MonoBehaviour
{

    private string axiom;
    private string currentString;
    public LSystemRuleSet.LSystemType systemType;
    private Dictionary<char, string> rules;
    [Range(1, 10)]
    public int iterations;
    public float stepLength, scalefac;
    private float angle;
    public bool isRandom, useNoise;
    [Range(0.0f, 1.0f)]
    public float RandMin, RandMax;
    public Material branchMaterial, leafMaterial;
    public GameObject leafPrefab;
    public float leafSize = 0.1f;
    private Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
    private bool generateSystem = false;
    private bool isGenerating = false;
    private GameObject currentNode;
    private List<TransformInfo> allObjects = new List<TransformInfo>();
    private List<Vector3> endPoints = new List<Vector3>();
    private List<BoxCollider> colliders = new List<BoxCollider>();
    private int nodeCounter = 0;
    float nodeLength;
    public float branchWidth = 0.05f;
    public float widthMod = 0.75f;
    private float nodeWidth;
    public List<LSystemRuleSet.ParametricModule> modules = new List<LSystemRuleSet.ParametricModule>();
    public List<char> parametricChar = new List<char>();
    private Vector3 startPos;
    private bool isleaf = false;
    [HideInInspector]
    public GameObject generatedObject;
    string newString = "";
    public void Init()
    {
        currentNode = null;
        Reset();
        startPos = transform.position;

        LSystemRuleSet ruleSet = new LSystemRuleSet(systemType);
        if (systemType == LSystemRuleSet.LSystemType.Leaf) isleaf = true;
        rules = ruleSet.getRules();
        axiom = ruleSet.getAxiom();
        angle = ruleSet.getAngle();
        parametricChar = ruleSet.getParametricChars();
        currentString = axiom;
        nodeWidth = branchWidth * Random.Range(0.75f, 1);
        Generate();
        transform.position = startPos;
        transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
    }

    private void Reset()
    {
        modules.Clear();
        parametricChar.Clear();
        nodeCounter = 0;
        nodeLength = 0;
        nodeWidth = 0;
        transformStack = new Stack<TransformInfo>();
        allObjects = new List<TransformInfo>();
        endPoints = new List<Vector3>();
        startPos = Vector3.zero;

        isleaf = false;
    }

    public void saveLastAsPrefab()
    {
        //Set the path as within the Assets folder,
        // and name it as the GameObject's name with the .Prefab format
        string localPath = "Assets/Prefabs/" + generatedObject.name + ".prefab";

        // Make sure the file name is unique, in case an existing Prefab has the same name.
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        // Create the new Prefab.
        PrefabUtility.SaveAsPrefabAssetAndConnect(generatedObject, localPath, InteractionMode.UserAction);
    }

    void createBranch(Vector3 endPos)
    {

        LineRenderer lr = currentNode.GetComponent<LineRenderer>();
        if (!lr) lr = currentNode.AddComponent<LineRenderer>();

        lr.startWidth = nodeWidth;
        lr.endWidth = nodeWidth;
        lr.alignment = LineAlignment.View;
        lr.useWorldSpace = false;
        Vector3 localEnd = currentNode.transform.InverseTransformPoint(endPos);
        lr.SetPositions(new Vector3[] { Vector3.zero, localEnd });
        lr.material = branchMaterial;

    }

    void createLeaf(Vector3 endPos)
    {
        GameObject leaf = GameObject.Instantiate(leafPrefab);
        leaf.transform.position = endPos;
        leaf.transform.position = transform.position;
        leaf.transform.parent = currentNode.transform;
        leaf.transform.rotation = transform.rotation;
        leaf.transform.localScale = Vector3.one * leafSize;
        leaf.tag = "Leaf";
        scaleLeaf(leaf);
        allObjects.Add(leaf.GetComponent<TransformInfo>());

    }

    private void scaleLeaf(GameObject leaf)
    {
        ArrayList leafLRs = new ArrayList();
        leafLRs.AddRange(leaf.GetComponentsInChildren<LineRenderer>());

        foreach (LineRenderer lr in leafLRs)
        {

            float originalWidth = lr.GetComponent<TransformInfo>().nodeWidth;
            float newWidth = leaf.transform.localScale.x * originalWidth;

            lr.SetWidth(newWidth, newWidth);
        }


    }

    private void sortPoints()
    {
        endPoints.Add(startPos);
        foreach (TransformInfo t in allObjects)
        {
            if (t.nodeLength > 0) endPoints.Add((t.endPoint != Vector3.zero) ? t.endPoint : t.positon);
        }

        var convexHull = ConvexHull.compute(endPoints);
        var convexHull2D = convertPoints2D(convexHull);
        generateMesh(convexHull, convexHull2D);

    }

    Vector3 getClosest(Vector3 v, List<Vector3> points)
    {
        Vector3 closest = new Vector3();
        float minDist = float.MaxValue;
        foreach (Vector3 V3 in points)
        {
            if (v != V3)
            {
                float dist = Vector3.Distance(v, V3);
                if (dist < minDist)
                {
                    closest = V3;
                    minDist = dist;
                }
            }
        }
        return closest;


    }

    public void initColliders()
    {
        foreach (TransformInfo t in allObjects)
        {
            GameObject g = t.gameObject;
            if (t.nodeLength > 0)
            {
                BoxCollider collider = g.AddComponent<BoxCollider>();
                collider.size = new Vector3(t.nodeWidth, 0.01f, t.nodeLength);
                colliders.Add(collider);
            }
        }

    }

    public void setUpPhysics()
    {

        float maxMass = 0;
        float maxLength = 0;
        float maxWidth = 0;
        GameObject groundPlane = GameObject.Find("GroundPlane");
        foreach (TransformInfo t in allObjects)
        {
            GameObject g = t.gameObject;

            g.layer = 9;


            if (t.nodeLength > 0)
            {
                Rigidbody rB = g.AddComponent<Rigidbody>();
               // HingeJoint hingeJoint = g.AddComponent<HingeJoint>();
                g.AddComponent<RigidBodyOverride>();
                if (g == generatedObject)
                {

                    maxMass = rB.mass;
                    maxLength = t.nodeLength;
                    maxWidth = t.nodeWidth;
                //    hingeJoint.connectedBody = groundPlane.GetComponent<Rigidbody>();


                }
                else
                {
                    float nodeLenFac = (t.nodeLength / maxLength);
                    float nodeWdithFac = (t.nodeWidth / maxWidth);
                    rB.mass = (nodeLenFac + nodeWdithFac) / (maxMass * 2) / 10;
                    if (t.gameObject.tag == "Leaf") rB.mass *= 0.01f;
                  //  hingeJoint.connectedBody = (g.transform.parent.GetComponent<Rigidbody>() == null) ? g.transform.parent.transform.parent.GetComponent<Rigidbody>() : g.transform.parent.GetComponent<Rigidbody>();
                }

                rB.useGravity = false;
                rB.angularDrag = 0.5f;
                rB.drag = 1;
              /*  hingeJoint.anchor = Vector3.zero;
                hingeJoint.axis = Vector3.up;

                hingeJoint.useSpring = false;
                JointSpring jointSpring = new JointSpring();
                jointSpring.spring = 0.1f;
                // Debug.Log(g.transform.localRotation.y);
                //  Debug.Log(g.transform.localRotation.eulerAngles.y);
                jointSpring.targetPosition = g.transform.localRotation.eulerAngles.y;
                jointSpring.damper = 0.5f;
                hingeJoint.spring = jointSpring;
                hingeJoint.useLimits = true;
                hingeJoint.breakForce = 150;
                hingeJoint.breakTorque = 150;
                JointLimits limits = new JointLimits();
                limits.min = 0 - (0.56f * angle);
                limits.max = 0 + (0.44f * angle);
                limits.bounciness = 0.1f;
                limits.bounceMinVelocity = 0.1f;
                limits.contactDistance = 5f;
                hingeJoint.enableCollision = false;
                hingeJoint.limits = limits;*/
            
            }




        }
    }

    private List<Vector2> convertPoints2D(List<Vector3> points)
    {
        List<Vector2> points2D = new List<Vector2>();

        foreach (Vector3 v3 in points)
        {
            Vector2 v2 = v3;
            points2D.Add(v2);
        }

        return points2D;
    }
    private List<Vector3> convertPoints3D(List<Vector2> points)
    {
        List<Vector3> points3D = new List<Vector3>();

        foreach (Vector2 v2 in points)
        {
            Vector3 v3 = v2;
            points3D.Add(v3);
        }

        return points3D;
    }

    void generateMesh(List<Vector3> points, List<Vector2> points2D)
    {
        // Use the triangulator to get indices for creating triangles
        Vector2[] vertices2D = points2D.ToArray();
        Vector3[] vertices = points.ToArray();
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices


        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        GameObject GO = new GameObject();
        GO.transform.position = GO.transform.position - new Vector3(0, 0.06f, 0);
        GO.transform.parent = currentNode.transform;
        GO.name = "LeafMesh";
        GO.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = GO.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer mR = GO.GetComponent<MeshRenderer>();
        mR.material = leafMaterial;
        filter.sharedMesh = msh;
        GO.AddComponent<SerializeMesh>();
        /*  ObjExporter.MeshToFile(filter, "Assets/Prefabs/Meshes/"+generatedObject.name+".obj");
          msh = FastObjImporter.Instance.ImportFile("Assets/Prefabs/Meshes/" + generatedObject.name + ".obj");
          filter.sharedMesh = msh;*/


    }

    void processString()
    {
        for (int n = 0; n < iterations; n++)
        {
            string newString = "";
            char[] currentStringChar = currentString.ToCharArray();

            for (int i = 0; i < currentStringChar.Length; i++)
            {

                char currentChar = currentStringChar[i];
                if (rules.ContainsKey(currentChar))
                {
                    newString += rules[currentChar];
                }
                else
                {
                    newString += currentChar.ToString();
                }
            }
            currentString = newString;

        }
      //  Debug.Log(currentString);
    }

    void processStringParametric()
    {

        newString = "";
        char[] currentStringChar = currentString.ToCharArray();
        int param = 0;
       
        processModules(currentStringChar, param, iterations);
        currentString = newString;


        Debug.Log(currentString);
     



    }

    void createModulesFromString(char[] inputChar, int param)
    {
      
        foreach (char c in inputChar)
        {
            LSystemRuleSet.ParametricModule module = new LSystemRuleSet.ParametricModule(c, param, parametricChar.Contains(c));
            modules.Add(module);
        }
    }


    void processModules(char[] currentStringChar, int param, int iteration)
    {
        int paramoffset = 1;
        iteration--;
        if (iteration < 0)
        {
            createModulesFromString(currentStringChar, param - paramoffset);
            return;

        }
   
        for (int i = 0; i < currentStringChar.Length; i++)
        {

            char currentChar = currentStringChar[i];
            if (rules.ContainsKey(currentChar))
            {
                string ruleString = rules[currentChar];
                newString += ruleString;
                processModules(ruleString.ToCharArray(), param+paramoffset, iteration);

            }
            else
            {
                newString += currentChar.ToString();
                LSystemRuleSet.ParametricModule module = new LSystemRuleSet.ParametricModule(currentChar, param, parametricChar.Contains(currentChar));
                modules.Add(module);
            }
        }
    }



    void Generate()
    {


        isGenerating = true;
      
        if (systemType == LSystemRuleSet.LSystemType.Parametric) processStringParametric();
        else processString();
        char[] currentCharacters = currentString.ToCharArray();
        switch (systemType)
        {
            case LSystemRuleSet.LSystemType.Plant:
                StartCoroutine(generatePlant(currentCharacters));
                break;
            case LSystemRuleSet.LSystemType.FractalPlant:
                StartCoroutine(generatePlant(currentCharacters));
                break;
            case LSystemRuleSet.LSystemType.FracatalTree:
                StartCoroutine(generateBinaryTree(currentCharacters));
                break;
            case LSystemRuleSet.LSystemType.FractalBush:
                StartCoroutine(generatePlant(currentCharacters));
                break;

            case LSystemRuleSet.LSystemType.Leaf:
                StartCoroutine(generatePlant(currentCharacters));
                break;

            case LSystemRuleSet.LSystemType.Weed:
                StartCoroutine(generatePlant(currentCharacters));

                break;
            case LSystemRuleSet.LSystemType.Sticks:
                StartCoroutine(generatePlant(currentCharacters));
                break;

            case LSystemRuleSet.LSystemType.Parametric:
                StartCoroutine(generateParametricTree());
                break;

        }


    }

    private void createChildNode()
    {

        GameObject Go = new GameObject();
        Go.transform.position = transform.position;
        Go.transform.rotation = transform.rotation;
        if (nodeCounter == 0)
        {
            generatedObject = Go;
            Go.name = systemType.ToString() + Random.seed;
        }
        else
        {
            Go.name = "ChildNode " + nodeCounter;
        }

        if (nodeCounter != 0) Go.transform.parent = currentNode.transform;
        currentNode = Go;
        if (!currentNode.GetComponent<TransformInfo>())
        {
            TransformInfo ti = currentNode.AddComponent<TransformInfo>();
            ti.nodeWidth = nodeWidth;
            allObjects.Add(ti);
        }

        nodeLength = 0;

        nodeCounter++;
    }

    IEnumerator generatePlant(char[] currentCharacters)
    {



        for (int i = 0; i < currentCharacters.Length; i++)
        {

            if (currentNode == null)
            {
                createChildNode();
            }

            char currentChar = currentCharacters[i];
            //  Debug.Log(currentChar);

            TransformInfo ti;
            float rand = 1;
            switch (currentChar)
            {
                case 'F':
                    // move forward
                    if (isRandom) rand = stepLength * Random.Range(RandMin, RandMax);
                    else rand = stepLength;

                    nodeLength += rand;
                    transform.Translate(Vector3.forward * rand);
                    //  transform.position = new Vector3(transform.position.x, transform.position.y, startPos.z);
                    ti = currentNode.GetComponent<TransformInfo>();
                    ti.endPoint = transform.position;
                    ti.nodeLength = nodeLength;
                    createBranch(transform.position);
#if !UNITY_EDITOR
                  
                    yield return new WaitForFixedUpdate();
#endif

                    break;

                case '0':

                    createLeaf(transform.position);

#if !UNITY_EDITOR
                  
                    yield return new WaitForFixedUpdate();
#endif
                    break;

                /*  case 'f':
                      // move forward
                      initialPosition = transform.position;
                      rndLength = stepLength * Random.Range(0.5f, 1.0f);
                      transform.Translate(Vector3.forward * rndLength); 
                      break;*/

                case '>':
                    stepLength *= scalefac;

                    break;
                case '<':
                    stepLength /= scalefac;

                    break;


                case '-':
                    // rotate -         
                    if (nodeLength > 0)
                    {
                        nodeWidth *= widthMod;
                        createChildNode();
                    }
                    if (isRandom)
                    {
                        if (useNoise)
                        {
                            rand = Mathf.PerlinNoise(currentNode.transform.position.x, currentNode.transform.position.y);
                            // rand = Mathf.PerlinNoise(m.parameter, 0);
                        }
                        else
                        {
                            rand = Random.Range(RandMin, RandMax);
                        }
                    }
                    transform.Rotate(Vector3.up * (-angle * rand));
                    currentNode.transform.rotation = transform.rotation;
                    break;

                case '+':
                    // rotate + 
                    if (nodeLength > 0)
                    {
                        nodeWidth *= widthMod;
                        createChildNode();
                    }

                    if (isRandom)
                    {
                        if (useNoise)
                        {
                            rand = Mathf.PerlinNoise(currentNode.transform.position.x, currentNode.transform.position.y);
                            // rand = Mathf.PerlinNoise(m.parameter, 0);
                        }
                        else
                        {
                            rand = Random.Range(RandMin, RandMax);
                        }
                    }
                    transform.Rotate(Vector3.up * (angle * rand));
                    currentNode.transform.rotation = transform.rotation;
                    break;

                case '[':


                    ti = currentNode.GetComponent<TransformInfo>();
                    ti.positon = transform.position;
                    ti.rotation = transform.rotation;
                    ti.nodeWidth = nodeWidth;

                    transformStack.Push(ti);

                    nodeWidth *= widthMod;
                    createChildNode();
                    break;

                case ']':


                    ti = transformStack.Pop();
                    currentNode = ti.gameObject;
                    transform.position = ti.positon;
                    transform.rotation = ti.rotation;
                    nodeWidth = ti.nodeWidth;
                    nodeLength = ti.nodeLength;
                    break;


            }
            //  if (currentChar!= '>' && currentChar != '<') Debug.Log(currentChar); 
        }
#if UNITY_EDITOR
        yield return new WaitForSeconds(0);
#endif

        if (isleaf) sortPoints();

        isGenerating = false;
    }


    IEnumerator generateBinaryTree(char[] currentCharacters)
    {


        for (int i = 0; i < currentCharacters.Length; i++)
        {

            if (currentNode == null)
            {
                createChildNode();
            }
            char currentChar = currentCharacters[i];
            // Debug.Log(currentChar);
            Vector3 initialPosition;
            TransformInfo ti;
            float rndLength;

            switch (currentChar)
            {
                case '0':

                    // move forward and end in Leaf;

                    createLeaf(transform.position);
                    /*
                 
                    initialPosition = transform.position;
                    rndLength = stepLength * Random.Range(0.5f, 1.0f);
                    transform.Translate(Vector3.forward * rndLength);
                    //   createBranch(initialPosition, transform.position);
                    createLeaf(transform.position);*/

#if !UNITY_EDITOR
                  
                    yield return new WaitForFixedUpdate();
#endif
                    break;

                case '1':

                    // move forward
                    rndLength = stepLength * Random.Range(0.5f, 1.0f);
                    nodeLength += rndLength;
                    transform.Translate(Vector3.forward * rndLength);
                    ti = currentNode.GetComponent<TransformInfo>();
                    ti.endPoint = transform.position;
                    ti.nodeLength = nodeLength;
                    createBranch(transform.position);

#if !UNITY_EDITOR
                  
                    yield return new WaitForFixedUpdate();
#endif

                    break;



                case '[':
                    ti = currentNode.GetComponent<TransformInfo>();
                    ti.positon = transform.position;
                    ti.rotation = transform.rotation;
                    ti.nodeWidth = nodeWidth;

                    transformStack.Push(ti);

                    nodeWidth *= widthMod;
                    createChildNode();

                    // rotate -     
                    if (nodeLength > 0)
                    {
                        nodeWidth *= widthMod;
                        createChildNode();
                    }
                    transform.Rotate(Vector3.up * (-angle * Random.Range(0.9f, 1.0f)));
                    currentNode.transform.rotation = transform.rotation;
                    break;

                case ']':
                    ti = transformStack.Pop();
                    currentNode = ti.gameObject;
                    transform.position = ti.positon;
                    transform.rotation = ti.rotation;
                    nodeWidth = ti.nodeWidth;
                    nodeLength = ti.nodeLength;
                    // rotate +
                    // rotate + 
                    if (nodeLength > 0)
                    {
                        nodeWidth *= widthMod;
                        createChildNode();
                    }
                    transform.Rotate(Vector3.up * (angle * Random.Range(0.9f, 1.0f)));
                    currentNode.transform.rotation = transform.rotation;

                    break;


            }
        }
#if UNITY_EDITOR
        yield return new WaitForSeconds(0);
#endif
        isGenerating = false;

    }

    IEnumerator generateParametricTree()
    {

        foreach (LSystemRuleSet.ParametricModule m in modules)
        {
            if (currentNode == null)
            {
                createChildNode();
            }

            Vector3 initialPosition;
            TransformInfo ti;
            float rndLength, newAng;
            float rand;

            char currentChar = m.moduleIdentifier;

            currentNode.GetComponent<TransformInfo>().param = m.parameter;

            Debug.Log(currentChar);

            switch (currentChar)
            {
                case '0':

                    // move forward and end in Leaf;
                    if (m.hasParam)
                    {
                        if (m.parameter < 3)
                        {
                            break;
                        }
                    }
                    createLeaf(transform.position);
                    /*
                 
                    initialPosition = transform.position;
                    rndLength = stepLength * Random.Range(0.5f, 1.0f);
                    transform.Translate(Vector3.forward * rndLength);
              */

#if !UNITY_EDITOR
                  
                    yield return new WaitForFixedUpdate();
#endif
                    break;

                case 'F':

                    rand = 1;
                    // move forward
                    if (isRandom) rand = Random.Range(RandMin, RandMax);

                    rndLength = stepLength * ((m.hasParam) ? 1 + (m.parameter / 10) * rand : 1);
                    nodeLength += rndLength;
                    transform.Translate(Vector3.forward * rndLength);
                    ti = currentNode.GetComponent<TransformInfo>();
                    ti.endPoint = transform.position;
                    ti.nodeLength = nodeLength;
                    createBranch(transform.position);

#if !UNITY_EDITOR
                  
                    yield return new WaitForFixedUpdate();
#endif

                    break;

                case '-':
                    rand = 1;
                    // rotate -         
                    if (nodeLength > 0)
                    {
                        nodeWidth *= widthMod;
                        createChildNode();
                    }
                    if (isRandom)
                    {
                        if (useNoise)
                        {
                            rand = Mathf.PerlinNoise(currentNode.transform.position.x, currentNode.transform.position.y);
                            // rand = Mathf.PerlinNoise(m.parameter, 0);
                        }
                        else
                        {
                            rand = Random.Range(RandMin, RandMax);
                        }
                    }

                    newAng = -angle * ((m.hasParam) ? 1 + (m.parameter * rand) : 1);
                    transform.Rotate(Vector3.up * newAng);
                    // transform.Rotate(Vector3.up * (-angle * Random.Range(0.9f, 1.0f)));
                    currentNode.transform.rotation = transform.rotation;
                    break;

                case '+':

                    rand = 1;
                    // rotate + 
                    if (nodeLength > 0)
                    {
                        nodeWidth *= widthMod;
                        createChildNode();
                    }
                    if (isRandom)
                    {
                        if (useNoise)
                        {
                            rand = Mathf.PerlinNoise(currentNode.transform.position.x, currentNode.transform.position.y);
                            // rand = Mathf.PerlinNoise(m.parameter, 0);
                        }
                        else
                        {
                            rand = Random.Range(RandMin, RandMax);
                        }
                    }

                    newAng = angle * ((m.hasParam) ? 1 + (m.parameter * rand) : 1);
                    transform.Rotate(Vector3.up * newAng);
                    //transform.Rotate(Vector3.up * (angle * Random.Range(0.9f, 1.0f)));
                    currentNode.transform.rotation = transform.rotation;

                    break;

                case '[':
                    ti = currentNode.GetComponent<TransformInfo>();
                    ti.positon = transform.position;
                    ti.rotation = transform.rotation;
                    ti.nodeWidth = nodeWidth;

                    transformStack.Push(ti);

                    nodeWidth *= widthMod;
                    createChildNode();
                    break;

                case ']':
                    ti = transformStack.Pop();
                    currentNode = ti.gameObject;
                    transform.position = ti.positon;
                    transform.rotation = ti.rotation;
                    nodeWidth = ti.nodeWidth;



                    break;


            }
        }

#if UNITY_EDITOR
        yield return new WaitForSeconds(0);
#endif
        isGenerating = false;

    }
}
