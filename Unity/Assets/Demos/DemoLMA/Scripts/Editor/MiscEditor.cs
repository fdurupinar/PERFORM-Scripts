using UnityEngine;
using UnityEditor;
using System.Collections;


public class MiscEditor : EditorWindow {

    [MenuItem("ADAPT/Misc")]
    private static void Init() {
        MiscEditor window = (MiscEditor)EditorWindow.GetWindow(typeof(MiscEditor), false, "Misc");
       
    }

    private void OnGUI() {
       if (GUILayout.Button("Copy pose", GUILayout.ExpandWidth(false))) {
           GameObject agentTo = GameObject.Find("AgentPrefab.Carl:Hips");
           GameObject agentFrom = GameObject.Find("AgentFrom.Carl:Hips");

           CopyTransforms(agentFrom.transform, agentTo.transform);

       }
    }

 
    private void CopyTransforms(Transform rootFrom, Transform rootTo) {
        if (rootFrom == null || rootTo == null)
            return;
        
        rootTo.transform.position = rootFrom.transform.position;
        rootTo.transform.rotation = rootFrom.transform.rotation;
        rootTo.transform.localScale = rootFrom.transform.localScale;


        for (int i = 0; i < rootFrom.childCount; i++)
            CopyTransforms(rootFrom.GetChild(i), rootTo.GetChild(i));
            

    }
}
