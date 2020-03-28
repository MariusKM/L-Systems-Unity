using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LSystemSpawner))]
public class LSystemSpawnerUtil : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LSystemSpawner myScript = (LSystemSpawner)target;
        if (GUILayout.Button("Spawn Objects"))
        {
            myScript.Init();
        }
        if (GUILayout.Button("Fit to Ground Plane"))
        {
            myScript.fitToGroundPlane();
        }
        if (GUILayout.Button("Clear Objects"))
        {
            myScript.Clear();
        }


    }
}

