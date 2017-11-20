using UnityEngine;
using UnityEditor;
using System.Collections;


//[CustomEditor(typeof(ArmController))]
public class ArmControllerEditor : Editor {
    
        private ArmController _arm;

    
        void OnEnable() {
            if (serializedObject == null) return;

            _arm = target as ArmController;
            //Find hips
           Transform spine2 = _arm.gameObject.transform.Find("Hips").GetChild(2).GetChild(0).GetChild(0);
            
            

            if (_arm == null) {
                Debug.Log("No torso controller found");
                return;
            }
        
        
            _arm.Arms[0].Shoulder.transform = spine2.transform.GetChild(0).GetChild(0);        
            _arm.Arms[0].Elbow.transform = spine2.transform.GetChild(0).GetChild(0).GetChild(0);
            _arm.Arms[0].Wrist.transform = spine2.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0);

            _arm.Arms[1].Shoulder.transform = spine2.transform.GetChild(2).GetChild(0);        
            _arm.Arms[1].Elbow.transform = spine2.transform.GetChild(2).GetChild(0).GetChild(0);
            _arm.Arms[1].Wrist.transform = spine2.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0);
        }
        
}
