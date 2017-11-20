using UnityEngine;
using System.Collections;

public class CollisionHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision){
        collision.collider.gameObject.transform.Translate(0, -1, 0);
        Debug.Log("here");

    }
        
}
