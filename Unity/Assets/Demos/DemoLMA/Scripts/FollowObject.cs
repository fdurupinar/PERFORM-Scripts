using UnityEngine;
using System.Collections;

public class FollowObject : MonoBehaviour {
    public GameObject FollowedObj;
	void Update () {
				this.transform.position = FollowedObj.transform.position + new Vector3(0,4, 0f);
        //transform.Translate(0, 0, 0.01f);
	
	}
}
