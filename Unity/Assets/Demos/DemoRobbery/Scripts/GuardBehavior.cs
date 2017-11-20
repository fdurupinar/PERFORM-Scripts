using UnityEngine;
using System.Collections;

public class GuardBehavior : MonoBehaviour {
    
   
    public void Fall() {
        GetComponent<AnimationInfo>().DisableLMA = true;
    }
}
