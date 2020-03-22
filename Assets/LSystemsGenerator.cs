using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public bool generateSystem = false;
    private bool isGenerating = false;
    private bool newBranch;
    public GameObject currentNode;
    public List<TransformInfo> allObjects = new List<TransformInfo>();
    public List<Vector3> endPoints = new List<Vector3>();
    private int nodeCounter = 0;
    float nodeLength;
    public float nodeWidth;
    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        Debug.Log(startPos);
        LSystemRuleSet ruleSet = new LSystemRuleSet(systemType);
        rules = ruleSet.getRules();
        axiom = ruleSet.getAxiom();
        angle = ruleSet.getAngle();
        currentString = axiom;
        nodeWidth = 0.05f * Random.Range(0.75f, 1);
        Generate();
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
        foreach(Vector3 V3 in points)
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
        filter.mesh = msh;

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
        Go.name = "ChildNode " + nodeCounter;
        if (nodeCounter != 0) Go.transform.parent = currentNode.transform;
        currentNode = Go;
        if (!currentNode.GetComponent<TransformInfo>())
        {
            TransformInfo ti = currentNode.AddComponent<TransformInfo>();
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

                    yield return new WaitForFixedUpdate();
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
                        nodeWidth /= 1.5f;
                        createChildNode();
                    }
                    transform.Rotate(Vector3.up * (-angle * Random.Range(0.9f, 1.0f)));
                    break;

                case '+':
                    // rotate + 
                    if (nodeLength > 0)
                    {
                        nodeWidth /= 1.5f;
                        createChildNode();
                    }
                    transform.Rotate(Vector3.up * (angle * Random.Range(0.9f, 1.0f)));
                    break;

                case '[':


                    ti = currentNode.GetComponent<TransformInfo>();
                    ti.positon = transform.position;
                    ti.rotation = transform.rotation;
                    ti.nodeWidth = nodeWidth;
                 
                    transformStack.Push(ti);

                    nodeWidth /= 1.5f;
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
        sortPoints();
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
