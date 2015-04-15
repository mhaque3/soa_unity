using UnityEngine;
using System.Collections;

public class RotorRotate : MonoBehaviour {

    public Vector3 axis;
    public float rate;
    
    // Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Rotate(axis * rate);
	}
}
