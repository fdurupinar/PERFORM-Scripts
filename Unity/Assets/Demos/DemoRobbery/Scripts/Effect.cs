using UnityEngine;
using System.Collections;

public class Effect : MonoBehaviour 
{
    public float Lifetime = 0.5f;
    private float killTime;

	void Start () 
    {
        this.killTime = Time.time + this.Lifetime;
	}
	
	void Update () 
    {
        if (Time.time > this.killTime)
            Destroy(gameObject);
	}
}
