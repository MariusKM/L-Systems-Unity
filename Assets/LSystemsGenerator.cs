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

    // Start is called before the first frame update
    void Start()
    {
        LSystemRuleSet ruleSet = new LSystemRuleSet(systemType);
        rules = ruleSet.getRules();
        axiom = ruleSet.getAxiom();
        angle = ruleSet.getAngle();
        currentString = axiom;
    
        Generate();
    }

    void createBranch(Vector3 initialPos, Vector3 endPos)
    {

        GameObject go = new GameObject();

        go.transform.position = initialPos;
        go.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.startWidth = 0.05f * Random.Range(0.75f, 1);
        lr.endWidth = 0.05f * Random.Range(0.75f, 1);
        lr.alignment = LineAlignment.View;//TransformZ;
        lr.SetPositions(new Vector3[] { initialPos, endPos });
        lr.material = branchMaterial;



    }

    void createLeaf( Vector3 endPos)
    {

        GameObject go = new GameObject();

        go.transform.position = endPos;
        Vector3 scale = go.transform.localScale*1.5f;

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


    IEnumerator generatePlant(char[] currentCharacters)
    {


        for (int i = 0; i < currentCharacters.Length; i++)
        {
            char currentChar = currentCharacters[i];
            Vector3 initialPosition;
            TransformInfo ti;
            float rndLength = 0;
            switch (currentChar)
            {
                case 'F':
                    // move forward
                    initialPosition = transform.position;
                    rndLength = stepLength * Random.Range(0.5f, 1.0f);
                    transform.Translate(Vector3.forward * stepLength);
                    createBranch(initialPosition, transform.position);
                  
                    yield return new WaitForEndOfFrame();
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
                    transform.Rotate(Vector3.up * (-angle * Random.Range(0.9f, 1.0f)));
                    break;

                case '+':
                    // rotate + 
                    transform.Rotate(Vector3.up * (angle * Random.Range(0.9f, 1.0f)));
                    break;

                case '[':
                    ti = new TransformInfo();
                    ti.positon = transform.position;
                    ti.rotation = transform.rotation;
                    transformStack.Push(ti);
                    break;

                case ']':
                    ti = transformStack.Pop();
                    transform.position = ti.positon;
                    transform.rotation = ti.rotation;

                    break;


            }
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
                    createBranch(initialPosition, transform.position);
                    createLeaf(transform.position);
                  
                    yield return new WaitForEndOfFrame();
                    break;

                case '1':

                    // move forward
                    initialPosition = transform.position;
                    rndLength = stepLength * Random.Range(0.5f, 1.0f);
                    transform.Translate(Vector3.forward * rndLength);
                    createBranch(initialPosition, transform.position);
                
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
