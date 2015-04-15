using UnityEngine;
using System.Collections;

public class PropSpinY : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    public float DegreesPerFrame;
	void Update () 
    {

	}

    void LateUpdate()
    {
        transform.Rotate(new Vector3(0, 1f, 0), DegreesPerFrame * Time.deltaTime);
    }
}
