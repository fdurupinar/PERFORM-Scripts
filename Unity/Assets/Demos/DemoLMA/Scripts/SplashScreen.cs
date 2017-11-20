using UnityEngine;
using System.Collections;

public class SplashScreen : MonoBehaviour {
    void Start() {
        Application.LoadLevel("DemoPersonalityComparison");
    }
    void OnGUI() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Personality comparison");
        
        GUILayout.EndHorizontal();

        
     
    }
}
