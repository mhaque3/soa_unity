using UnityEngine;
using System.Collections;
using soa;

public class SoaJammer : MonoBehaviour {

    SoaActor thisSoaActor;
    
    public float effectiveRange_km = 0;
    public bool isOn = false;
	
    // Use this for initialization
	void Start () {
        thisSoaActor = gameObject.GetComponentInParent<SoaActor>();
	}

    public Vector3 getPosition()
    {
        return thisSoaActor.getPositionVector_km();
    }
}
