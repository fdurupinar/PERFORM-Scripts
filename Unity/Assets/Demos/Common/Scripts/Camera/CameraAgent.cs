
using UnityEngine;
using System.Collections;


public class CameraAgent : MonoBehaviour
{
    public Transform FollowedAgent;
    private float _yPos;
    
    void Start() { Init(); }
    void OnEnable() { Init(); }

    public void Init() {
        transform.position = FollowedAgent.position + FollowedAgent.forward * 2f;
        _yPos = transform.position.y;
    }

    void Update() {
        
        transform.position = FollowedAgent.position + FollowedAgent.forward * 2f  ;

        transform.position = new Vector3(transform.position.x, _yPos, transform.position.z);

        transform.LookAt(new Vector3(FollowedAgent.position.x, _yPos, FollowedAgent.position.z));
        
    }

}
