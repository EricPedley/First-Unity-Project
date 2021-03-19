using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Line))]
public class LineInspector : Editor
{
    private void OnSceneGUI () {
		Line line = target as Line;
        Transform trans = line.transform;
        Quaternion rotation = trans.rotation;
        Vector3 p0 = trans.TransformPoint(line.p0);
        Vector3 p1 = trans.TransformPoint(line.p1);
		Handles.color = Color.white;
		Handles.DrawLine(p0,p1);
	}
}
