using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RedBaseSim : MonoBehaviour 
{
    public float Civilians;
    SimControl simControlScript;

    public Canvas uiCanvas;
    public Camera uiCamera;
    public GameObject labelUI;
    GameObject labelInstance;
    RectTransform labelTransform;
    public Vector3 labelPosition;
    Text[] labels;
	// Use this for initialization
	void Start () 
    {
        simControlScript = GameObject.FindObjectOfType<SimControl>();

        uiCanvas = (Canvas)Object.FindObjectOfType<Canvas>();
        GameObject MainCamera = GameObject.Find("Main Camera");
        uiCamera = MainCamera.GetComponent<Camera>();
        labelInstance = labelUI;
        labelInstance.transform.SetParent(uiCanvas.transform, false);
        labelTransform = labelInstance.transform as RectTransform;
        labels = labelInstance.GetComponentsInChildren<Text>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        labelPosition = uiCamera.WorldToScreenPoint(transform.position + new Vector3(0, 0, 1f)) - uiCanvas.transform.position;
        labelTransform.anchoredPosition = new Vector2(labelPosition.x, labelPosition.y);
        labels[0].text = Civilians.ToString("n0");
	}

    public GameObject AssignTarget()
    {
        // totally random for now - we can get more clever later...

        int targetCount = simControlScript.NgoSites.Count + simControlScript.Villages.Count;
        int targetIndex = Random.Range(0, targetCount);

        if (targetIndex < simControlScript.NgoSites.Count)
        {
            return simControlScript.NgoSites[targetIndex];
        }
        else
        {
            return simControlScript.Villages[targetIndex - simControlScript.NgoSites.Count];
        }
    }
}
