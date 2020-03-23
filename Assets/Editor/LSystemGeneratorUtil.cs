using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LSystemsGenerator))]
public class LSystemGeneratorUtil : Editor
{
  
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LSystemsGenerator myScript = (LSystemsGenerator)target;
        if (GUILayout.Button("Build Fractal"))
        {
            myScript.Init();
        }

        if (GUILayout.Button("Save As Prefab"))
        {
            myScript.saveLastAsPrefab();
        }

        if (GUILayout.Button("Add Colliders"))
        {
            myScript.initColliders();
        }
    }
}
