using System.Collections;
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
    public Material branchMaterial;
    public Sprite leafSprite;
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

    private Vector3 startPos;
    private bool isleaf = false;
    [HideInInspector]
    public GameObject generatedObject;

    // Start is called before the first frame update
    void Start()
    {
        //  Init();
    }

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
        currentString = axiom;
        nodeWidth = branchWidth * Random.Range(0.75f, 1);
        Generate();
        transform.position = startPos;
        transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
    }

    private void Reset()
    {
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
        GameObject go = new GameObject();
        go.transform.position = endPos;
        Vector3 scale = go.transform.localScale * 1.5f;
        go.transform.localScale = scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = leafSprite;
        Color srCol = new Color();
        ColorUtility.TryParseHtmlString("#00FAFF", out srCol);
        sr.color = srCol;
    }

    private void sortPoints()
    {
        /*  float cutoff = -0.8f;
          List<TransformInfo> temp = new List<TransformInfo>();

          foreach (TransformInfo t in allObjects)
          {
              if (t.nodeLength > cutoff) temp.Add(t);
          }
          TransformInfo.TIC comparator = new TransformInfo.TIC();
          allObjects = temp;
          allObjects.Sort(comparator);*/
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
                HingeJoint hingeJoint = g.AddComponent<HingeJoint>();
                g.AddComponent<RigidBodyOverride>();
                if (g == generatedObject)
                {
                         
                    maxMass = rB.mass;
                    maxLength = t.nodeLength;
                    maxWidth = t.nodeWidth;
                    hingeJoint.connectedBody = groundPlane.GetComponent<Rigidbody>();

                }
                else
                {
                    float nodeLenFac = (t.nodeLength / maxLength);
                    float nodeWdithFac = (t.nodeWidth / maxWidth);
                    rB.mass = (nodeLenFac + nodeWdithFac) / (maxMass * 2) / 10;
                    hingeJoint.connectedBody = (g.transform.parent.GetComponent<Rigidbody>() == null) ? g.transform.parent.transform.parent.GetComponent<Rigidbody>() : g.transform.parent.GetComponent<Rigidbody>();
                }

                rB.useGravity = false;
                rB.angularDrag = 0.5f;
                rB.drag = 1;
                hingeJoint.anchor = Vector3.zero;
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
                JointLimits limits = new JointLimits();
                limits.min = 0 - (0.56f * angle);
                limits.max = 0 + (0.44f * angle);
                limits.bounciness = 0.000f;
                limits.bounceMinVelocity = 0.0f;
                limits.contactDistance = 5f;
                hingeJoint.enableCollision = false;
                hingeJoint.limits = limits;
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
        GO.transform.parent = currentNode.transform;
        GO.name = "LeafMesh";
        GO.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = GO.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer mR = GO.GetComponent<MeshRenderer>();
        mR.material = branchMaterial;
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
        //      Debug.Log(currentString);

    }

    private void FixedUpdate()
    {
        if (generateSystem)
        {
            if (!isGenerating)
            {
                Generate();
            }
            generateSystem = false;

        }
    }

    void Generate()
    {


        isGenerating = true;
        processString();
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
            float rndLength = 0;
            switch (currentChar)
            {
                case 'F':
                    // move forward
                    rndLength = stepLength * Random.Range(0.5f, 1.0f);

                    nodeLength += rndLength;
                    transform.Translate(Vector3.forward * rndLength);
                    //  transform.position = new Vector3(transform.position.x, transform.position.y, startPos.z);
                    ti = currentNode.GetComponent<TransformInfo>();
                    ti.endPoint = transform.position;
                    ti.nodeLength = nodeLength;
                    createBranch(transform.position);
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
                    transform.Rotate(Vector3.up * (-angle * Random.Range(0.9f, 1.0f)));
                    currentNode.transform.rotation = transform.rotation;
                    break;

                case '+':
                    // rotate + 
                    if (nodeLength > 0)
                    {
                        nodeWidth *= widthMod;
                        createChildNode();
                    }
                    transform.Rotate(Vector3.up * (angle * Random.Range(0.9f, 1.0f)));
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
            char currentChar = currentCharacters[i];
            Vector3 initialPosition;
            TransformInfo ti;
            float rndLength;

            switch (currentChar)
            {
                case '0':
                    // move forward and end in Tree;
                    initialPosition = transform.position;
                    rndLength = stepLength * Random.Range(0.5f, 1.0f);
                    transform.Translate(Vector3.forward * rndLength);
                    //   createBranch(initialPosition, transform.position);
                    createLeaf(transform.position);

                    yield return new WaitForEndOfFrame();
                    break;

                case '1':

                    // move forward
                    initialPosition = transform.position;
                    rndLength = stepLength * Random.Range(0.5f, 1.0f);
                    transform.Translate(Vector3.forward * rndLength);
                    //createBranch(initialPosition, transform.position);

                    yield return new WaitForEndOfFrame();

                    break;



                case '[':
                    ti = new TransformInfo();
                    ti.positon = transform.position;
                    ti.rotation = transform.rotation;
                    transformStack.Push(ti);

                    // rotate -
                    transform.Rotate(Vector3.up * (-angle * Random.Range(0.9f, 1.0f)));
                    break;

                case ']':
                    ti = transformStack.Pop();
                    transform.position = ti.positon;
                    transform.rotation = ti.rotation;
                    // rotate +
                    transform.Rotate(Vector3.up * (angle * Random.Range(0.9f, 1.0f)));

                    break;


            }
        }
        isGenerating = false;

    }

    /* public class V3C : IComparer<Vector3>
     {
         public int Compare(Vector3 x, Vector3 y)
         {


         }
     }*/
}
