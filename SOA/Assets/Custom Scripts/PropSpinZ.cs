using UnityEngine;
using System.Collections;

public class PropSpinZ : MonoBehaviour {

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
        transform.Rotate(new Vector3(0, 0, 1), DegreesPerFrame * Time.deltaTime);
    }
}
