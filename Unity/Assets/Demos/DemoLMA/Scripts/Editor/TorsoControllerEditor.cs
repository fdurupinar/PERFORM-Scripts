using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;


[CustomEditor(typeof(TorsoController))]
public class TorsoControllerEditor : Editor {

     private TorsoController _torso;

     [SerializeField]
     string _filePath = ""; 
    
    void OnEnable() {

        return;

        if (serializedObject == null) return;

        _torso = target as TorsoController;
        if (_torso == null) {
            Debug.Log("No torso controller found");
            return;
        }
        GameObject agent = _torso.gameObject;

      //  _torso.Torso.Root = agent.transform;
      

        AssignBones(agent.transform);
        //Attach bones automatically
        foreach (Transform child in agent.transform) {
            AssignBones(child);            
        }

        _torso.Root = _torso.Hips;

        
        
    }
    /*
    public override void OnInspectorGUI() {
        _filePath = EditorGUILayout.TextField("File path", _filePath);

        if (GUILayout.Button("Record transforms"))
            RecordTransforms(_filePath);


        if (GUILayout.Button("Load transforms"))
            LoadTransforms(_filePath);

        this.Repaint();
    }
    */
    void LoadTransforms(string filePath) {
        _torso.BodyChain = _torso.BodyChainToArray(_torso.Root);

        string[] content = File.ReadAllLines(filePath);

        for(int i = 0; i < content.Length; i++) {
            String[] tokens = content[i].Split('\t');
            _torso.BodyChain[i].transform.position = new Vector3(float.Parse(tokens[0]),float.Parse(tokens[1]),float.Parse(tokens[2]));
            _torso.BodyChain[i].transform.rotation = new Quaternion(float.Parse(tokens[3]), float.Parse(tokens[4]), float.Parse(tokens[5]), float.Parse(tokens[6]));

            
        }
    }
        
    
    void RecordTransforms(string filePath) {
        
        
        _torso.BodyChain = _torso.BodyChainToArray(_torso.Root);

        using (StreamWriter sw = new StreamWriter(filePath)) {
            foreach (Transform t in _torso.BodyChain)
                sw.WriteLine(t.position.x + "\t" + t.position.y + "\t" + t.position.z + "\t" + 
                             t.rotation.x + "\t" + t.rotation.y + "\t" + t.rotation.z + "\t" + t.rotation.w);
        }
    
    }
    void AssignBones(Transform child) {
        
        if (child.name.ToUpper().Contains("HIP"))
            _torso.Hips = child.transform;
        else if (child.name.ToUpper().Contains("SPINE1"))
            _torso.Spine1 = child.transform;
        else if (child.name.ToUpper().Contains("SPINE2"))
            _torso.Spine2 = child.transform;
        else if (child.name.ToUpper().Contains("SPINE"))
            _torso.Spine = child.transform;
        else if (child.name.ToUpper().Contains("LEFTSHOULDER"))
            _torso.Clavicle[0] = child.transform;
        else if (child.name.ToUpper().Contains("RIGHTSHOULDER"))
            _torso.Clavicle[1] = child.transform;
        else if (child.name.ToUpper().Contains("LEFTARM"))
            _torso.Shoulder[0] = child.transform;
        else if (child.name.ToUpper().Contains("RIGHTARM"))
            _torso.Shoulder[1] = child.transform;
        else if (child.name.ToUpper().Contains("LEFTFOREARM"))
            _torso.Elbow[0] = child.transform;
        else if (child.name.ToUpper().Contains("RIGHTFOREARM"))
            _torso.Elbow[1] = child.transform;
        else if ((child.name.ToUpper().Contains("LEFTHAND")) && !child.name.ToUpper().Contains("THUMB") && !child.name.ToUpper().Contains("PINKY") && !child.name.ToUpper().Contains("MIDDLE") && !child.name.ToUpper().Contains("INDEX") && !child.name.ToUpper().Contains("RING"))
            _torso.Wrist[0] = child.transform;
        else if ((child.name.ToUpper().Contains("RIGHTHAND")) && !child.name.ToUpper().Contains("THUMB") && !child.name.ToUpper().Contains("PINKY") && !child.name.ToUpper().Contains("MIDDLE") && !child.name.ToUpper().Contains("INDEX") && !child.name.ToUpper().Contains("RING"))
            _torso.Wrist[1] = child.transform;
        else if (child.name.ToUpper().Contains("LEFTUPLEG"))
            _torso.Pelvis[0] = child.transform;
        else if (child.name.ToUpper().Contains("RIGHTUPLEG"))
            _torso.Pelvis[1] = child.transform;
        else if (child.name.ToUpper().Contains("LEFTLEG"))
            _torso.Knee[0] = child.transform;
        else if (child.name.ToUpper().Contains("RIGHTLEG"))
            _torso.Knee[1] = child.transform;
        else if (child.name.ToUpper().Contains("LEFTFOOT"))
            _torso.Foot[0] = child.transform;
        else if (child.name.ToUpper().Contains("RIGHTFOOT"))
            _torso.Foot[1] = child.transform;
        else if (child.name.ToUpper().Contains("LEFTTOEBASE"))
            _torso.Toe[0] = child.transform;
        else if (child.name.ToUpper().Contains("RIGHTTOEBASE"))
            _torso.Toe[1] = child.transform;
        else if (child.name.ToUpper().Contains("LEFTTOE_END"))
            _torso.ToeEnd[0] = child.transform;
        else if (child.name.ToUpper().Contains("RIGHTTOE_END"))
            _torso.ToeEnd[1] = child.transform;
        else if (child.name.ToUpper().Contains("NECK"))
            _torso.Neck = child.transform;
        else if (child.name.ToUpper().Contains("HEAD") &&  !child.name.ToUpper().Contains("END"))
            _torso.Head = child.transform;

        foreach (Transform gc in child) {
            AssignBones(gc);
        }
        
    }

 

}
