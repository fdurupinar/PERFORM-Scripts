using UnityEngine;
using System.Collections;
public class SpotLightBehavior : MonoBehaviour {


    public GameObject Target;  
    	
	// Update is called once per frame
	void Update () {

        transform.position = Target.transform.position;

        transform.position += Vector3.up * 2f;
        	
	}
}
