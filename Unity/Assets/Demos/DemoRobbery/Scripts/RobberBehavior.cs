using UnityEngine;
using System.Collections;

public class RobberBehavior : MonoBehaviour {
   

    public void Fire() {
        Vector3 posFire = gameObject.transform.position + transform.up*0.5f;

        Transform[] allChildren = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform t in allChildren)
            if (t.name.Contains("Hips")) {
                posFire = t.transform.position + t.up*0.5f;
                break;
            }
    

    GameObject g = (GameObject)Instantiate(Resources.Load("WarningShot"),posFire, gameObject.transform.rotation);
        
    }
}
