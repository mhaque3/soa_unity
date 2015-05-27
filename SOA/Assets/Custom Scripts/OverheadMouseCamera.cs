using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OverheadMouseCamera : MonoBehaviour 
{
	// good default values for sensitivity of pan/zoom but set these from Unity editor...
	public float sensitivityX = 0.1f;
	public float sensitivityZ = 0.1f;
	public float sensitivityY = 100f;

	//public GameObject terrainMap;
	//WorldSpaceTranslation wstScript;

	public List<GameObject> Platforms;
	public GameObject currentSelectedPlatform;
	public int trackedPlatformIndex;

	public bool trackingOn;
	public bool MousePan;
  
	// Use this for initialization
	void Start () 
	{
	}

	public void AddPlatform(GameObject newPlatform)
	{
		Platforms.Add(newPlatform);
	}

	public void DeletePlatform(GameObject thisPlatform)
	{
		Platforms.Remove(thisPlatform);
	}

	void SetTrackedPlatform(GameObject selectedPlatform)
	{
		currentSelectedPlatform = selectedPlatform;
		int index = Platforms.IndexOf(selectedPlatform);
		if (index > -1)
		{
			trackedPlatformIndex = index;
		}
	}

	float dx, dy, dz;
	public Vector3 mouseWorldPoint;
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetButtonDown("Toggle Camera Tracking"))
		{
			trackingOn = !trackingOn;
		}
		if (Input.GetButtonDown("Camera Overhead View"))
		{
			trackingOn = true;
			GetComponent<Camera>().orthographicSize = 20.0f;
			trackedPlatformIndex = 0;
			transform.eulerAngles = new Vector3(70f, transform.eulerAngles.y, transform.eulerAngles.z);
		}
		if (Input.GetButtonDown("Switch Camera Target"))
		{
			trackingOn = true;
			if(trackedPlatformIndex < Platforms.Count-1)
			{
				trackedPlatformIndex++;
			}
			else
			{
				trackedPlatformIndex = 0;
			}
		}

		// Mouse panning...
		Vector3 mouseVector = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
		float mx = mouseVector.x;
		float my = mouseVector.y;

		// Don't bother with any of the checks unless mouse is over the display area...
		if (mx > 0f && mx <= 1f && my > 0f && my <= 1f)
		{
            // Mouse wheel Camera Angle Tilt...
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Camera Tilt"))
			{
				// Get current angle
                float cameraTiltAngle_deg = transform.eulerAngles.x;

				// Get increment from mouse wheel
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					cameraTiltAngle_deg += -Input.GetAxis("Mouse ScrollWheel") * sensitivityY / 5f;
				}

				// Get increment from keybaord
				if (Input.GetButton("Camera Tilt"))
		        {
					if (Input.GetAxis("Camera Tilt") > 0)
					{
						cameraTiltAngle_deg += sensitivityY / 100f;
					}
					else if (Input.GetAxis("Camera Tilt") < 0)
					{
						cameraTiltAngle_deg += -sensitivityY / 100f;
					}                                         
                }

                // Impose limits
				if (cameraTiltAngle_deg > 85.0f)
				{
                    cameraTiltAngle_deg = 85.0f;
				}
				else if (cameraTiltAngle_deg < 30.0f)
				{
                    cameraTiltAngle_deg = 30.0f;
				}

                // Rotate
                transform.eulerAngles = new Vector3(cameraTiltAngle_deg, 0, 0);
			}

			if (trackingOn)
			{
				transform.position = new Vector3(Platforms[trackedPlatformIndex].transform.position.x,
					transform.position.y, Platforms[trackedPlatformIndex].transform.position.z 
					- Mathf.Tan(Mathf.Deg2Rad*(90f - transform.eulerAngles.x))  * transform.position.y);
				// ...this last term centers the camaera on the tracked platform accounting for the tilt of the camera
			}
			else
			{
				// Mouse panning
				if (MousePan)
				{
					dx = dy = dz = 0.0f;
					if (mx < 0.03f)
						dx = -sensitivityX;
					else if (mx > 0.97f)
						dx = sensitivityX;

					if (my < 0.03f)
						dz = -sensitivityZ;
					else if (my > 0.97f)
						dz = sensitivityZ;

					// Accelerated Mouse panning closer to the edge...
					if (mx < 0.01f)
						dx = 2.0f * -sensitivityX;
					else if (mx > 0.99f)
						dx = 2.0f * sensitivityX;

					if (my < 0.01f)
						dz = 2.0f * -sensitivityZ;
					else if (my > 0.99f)
						dz = 2.0f * sensitivityZ;

					transform.position += new Vector3(dx, 0.0f, dz);
				}

				// Keyboard panning so long as the horizontal or vertical buttons are pressed down
				if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
				{
					dx = dy = dz = 0.0f;

					// Use default sensitivty values
					if (Input.GetAxis("Horizontal") > 0)
					{
						dx = sensitivityX;
					}
					else if (Input.GetAxis("Horizontal") < 0)
					{
						dx = -sensitivityX;
					}
					if (Input.GetAxis("Vertical") > 0)
					{
						dz = sensitivityZ;
					}
					else if (Input.GetAxis("Vertical") < 0)
					{
						dz = -sensitivityZ;
					}

					// Apply transform
					transform.position += new Vector3(dx, 0.0f, dz);
				}
			}

			// Mouse wheel zoom in/out...
			if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
			{
				dy = - Input.GetAxis("Mouse ScrollWheel") * sensitivityY;
				// TBD turn this into a percentage function...
				if (GetComponent<Camera>().orthographicSize < 100f)
				{
					dy = dy / 10f;
				}
				if (GetComponent<Camera>().orthographicSize < 10f)
				{
					dy = dy / 10f;
				}
				GetComponent<Camera>().orthographicSize += dy;
				if (GetComponent<Camera>().orthographicSize < 0.1f)
				{
					GetComponent<Camera>().orthographicSize = 0.1f;
				}
			}

			// Keyboard zoom in/out
			if (Input.GetButton("Camera Zoom"))
			{
				if (Input.GetAxis("Camera Zoom") > 0)
				{
					dy = sensitivityY/1000;
				}
				else if (Input.GetAxis("Camera Zoom") < 0)
				{
					dy = -sensitivityY/1000;
				}
				GetComponent<Camera>().orthographicSize += dy;
				if (GetComponent<Camera>().orthographicSize < 0.1f)
				{
					GetComponent<Camera>().orthographicSize = 0.1f;
				}
			}
		}
	}
}
