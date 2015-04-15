using UnityEngine;
using System.Collections;

public class RandomMotion : MonoBehaviour 
{

    public bool GroundHugger = false;
    public float radialGravity;
    public float speed;
    public GameObject targetObject;

    //GameObject actionManagerObject;
    //static ConstraintElements ceScript;
	// Use this for initialization
	void Start () 
    {
        //actionManagerObject = GameObject.Find("ActionManager");
        //ceScript = actionManagerObject.GetComponent<ConstraintElements>();
	}
	
	// Update is called once per frame
    float timeInterval = 0;
    float intervalTimer = 0;
    //float turnAngle = 0;
    float oldHeading;
	void Update () 
    {
        float dt = Time.deltaTime;
        float motion = speed * dt;
        transform.Translate(motion * Vector3.forward);
        if (GroundHugger)
        {
            transform.Translate(motion * Vector3.forward);
            transform.position = new Vector3(transform.position.x, Terrain.activeTerrain.SampleHeight(transform.position), transform.position.z);
            //transform.position.y - Terrain.activeTerrain.SampleHeight(transform.position), Vector3.forward.z / 4f))
        }


        // TBD...
        // choose a desired target location @ f hz (driven by external target generator?)
        Vector3 targetPosition;
        if (timeInterval > .5)
        {
            targetPosition = targetObject.transform.position;
            // identify the angle offset to the desired location

            // select a desired bank angle to achieve at f hz

        }
        //
        // at f/n hz:
        //
        //  tip "slowly" towards desired bank angle (Lerp at deltaTime?)
        //      store current bank angle
        //
        //  adjust yaw / deltaTime by some multiple of current bank angle
        //

        
        if (timeInterval > .1)
        {
            oldHeading = transform.eulerAngles.y;
            targetPosition = targetObject.transform.position;
            Vector3 deltaV = (new Vector3(targetPosition.x, transform.position.y, targetPosition.z)) - transform.position;

            Vector3 newHeading = Vector3.RotateTowards(transform.forward, deltaV, radialGravity, 0.0f);



            transform.rotation = Quaternion.LookRotation(newHeading);
            //turnAngle = .3f * (Random.value - 0.5f);
            //transform.Rotate(Vector3.up, turnAngle);


            //transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 1f*(oldHeading - transform.eulerAngles.y));
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f * (oldHeading - transform.eulerAngles.y));
            //Debug.Log(transform.eulerAngles);

            //Debug.Log("Delta V" + deltaV + "  New Heading Vector: " + newHeading);

            timeInterval = 0;
        }
        
        //if(turnAngle <= 1f)
        //    transform.Rotate(Vector3.forward, -turnAngle / 2 * (timeInterval - 1f));

        timeInterval += dt;

        intervalTimer += dt;
        if (intervalTimer > .5f)
        {
            //ceScript.CheckDistances(transform.position, transform.forward);

            intervalTimer = 0f;
        }
	
	}
}
