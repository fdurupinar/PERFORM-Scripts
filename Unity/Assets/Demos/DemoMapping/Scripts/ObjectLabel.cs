using UnityEngine;
using System.Collections;
[RequireComponent(typeof(GUIText))]
public class ObjectLabel : MonoBehaviour {


    public Transform Target;  // Object that this label should follow
    public Vector3 Offset =  Vector3.up;    // Units in world space to offset; 1 unit above object by default
    public bool ClampToScreen = false;  // If true, label will be visible even if object is off screen
    public float ClampBorderSize = 0.05f;  // How much viewport space to leave at the borders when a label is being clamped
    public Camera Cam = null;
    
    Transform _thisTransform;
    Transform _camTransform;
	// Use this for initialization
	void Start () {
        if(GameObject.Find("Camera"))
	    Cam = GameObject.Find("Camera").GetComponent<Camera>();
	    if(Cam)
	        ResetTransforms(Cam);
	}

    public void ResetTransforms(Camera cam) {
        
        _thisTransform = transform;

        //_cam = GameObject.Find("Camera1").camera;
        _camTransform = cam.transform;
        

    }	
	// Update is called once per frame
	void Update () {
        if(!Cam)
            return;
        if (ClampToScreen) {
            Vector3 relativePosition = _camTransform.InverseTransformPoint(Target.position);
            relativePosition.z = Mathf.Max(relativePosition.z, 1.0f);
            _thisTransform.position = Cam.WorldToViewportPoint(_camTransform.TransformPoint(relativePosition + Offset));
            _thisTransform.position = new Vector3(Mathf.Clamp(_thisTransform.position.x, ClampBorderSize, 1.0f - ClampBorderSize),
                                             Mathf.Clamp(_thisTransform.position.y, ClampBorderSize, 1.0f - ClampBorderSize),
                                             _thisTransform.position.z);

        }
        else {
            _thisTransform.position = Cam.WorldToViewportPoint(Target.position + Offset);
            

        }
        
	
	}
}
