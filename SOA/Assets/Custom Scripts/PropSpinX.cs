using UnityEngine;
using System.Collections;

public class PropSpinX : MonoBehaviour {

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
        transform.Rotate(new Vector3(1, 0, 0), DegreesPerFrame * Time.deltaTime);
    }
}
