using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemRuleSet : MonoBehaviour
{
    private Dictionary<char, string> rules = new Dictionary<char, string>();
    private string axiom;
    private float angle;


    public enum LSystemType
    {
        Algae,
        FracatalTree,
        Plant,
        FractalPlant,
        FractalBush

    }


    public Dictionary<char, string> getRules()
    {
        return this.rules;
    }

    public string getAxiom()
    {
        return this.axiom;
    }

    public float getAngle()
    {
        return this.angle;
    }
    public LSystemRuleSet(LSystemType type)
    {
        initRules(type);
    }
    private void initRules(LSystemType type)
    {
        rules.Clear();
        axiom = "";
        switch (type)
        {

            case LSystemType.Algae:
                rules.Add('A', "AB");
                rules.Add('B', "A");
                this.axiom = "A";
                
                break;

            case LSystemType.FracatalTree:
                rules.Add('1', "11");
                rules.Add('0', " 1[0]0");
                this.axiom = "0";
                this.angle = 45;
                break;

            case LSystemType.Plant:
                rules.Add('F', "FF+[+F-F-F]-[-F+F+F]");
                this.axiom = "F";
                this.angle = 25;
                break;

            case LSystemType.FractalPlant:
                rules.Add('F', "FF");
                rules.Add('X', "F+[[X]-X]-F[-FX]+X");
                this.axiom = "X";
                this.angle = 25;
                break;
            case LSystemType.FractalBush:
                rules.Add('F', " F[+FF][-FF]F[-F][+F]F");
                this.axiom = "F";
                this.angle = 35;
                break;
        }
    }
}



