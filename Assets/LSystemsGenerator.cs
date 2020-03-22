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

    private int nodeCounter = 0;
    float nodeLength;
    public float nodeWidth;

    // Start is called before the first frame update
    void Start()
    {
        LSystemRuleSet ruleSet = new LSystemRuleSet(systemType);
        rules = ruleSet.getRules();
        axiom = ruleSet.getAxiom();
        angle = ruleSet.getAngle();
        currentString = axiom;
        nodeWidth = 0.05f * Random.Range(0.75f, 1);
        Generate();
    }

    void createBranch( Vector3 endPos)
    {

        LineRenderer lr = currentNode.GetComponent<LineRenderer>();
        if (!lr) lr  = currentNode.AddComponent<LineRenderer>();

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

    private bool isBranch(char[] currentCharacters, int index, int range)
    {

        int rangeIndex = index + range;
        if (rangeIndex > currentCharacters.Length) rangeIndex = currentCharacters.Length;

        for (int i = index + 1; i < rangeIndex; i++)
        {
            if (currentCharacters[i] == ']') return false;
            if (currentCharacters[i] == 'F') return true;
        }
        return false;
    }

    private void createChildNode()
    {

        GameObject Go = new GameObject();
        Go.transform.position = transform.position;
        Go.transform.rotation = transform.rotation;
        Go.name = "ChildNode " + nodeCounter;
        if (nodeCounter != 0) Go.transform.parent = currentNode.transform;
        currentNode = Go;
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
                    transform.Translate(Vector3.forward * stepLength);
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
                    /// Try to only use one stack; two might be unneccesaary
                    if (!currentNode.GetComponent<TransformInfo>()) ti = currentNode.AddComponent<TransformInfo>();
                    else ti = currentNode.GetComponent<TransformInfo>();
                    ti.positon = transform.position;
                    ti.rotation = transform.rotation;
                    ti.nodewidth = nodeWidth;
                    transformStack.Push(ti);

                    nodeWidth /= 1.5f;
                    createChildNode();
                    break;

                case ']':

                  
             
                    ti = transformStack.Pop();
                    currentNode = ti.gameObject;
                    transform.position = ti.positon;
                    transform.rotation = ti.rotation;
                    nodeWidth = ti.nodewidth;
                    break;


            }
            //  if (currentChar!= '>' && currentChar != '<') Debug.Log(currentChar); 
        }
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
}
