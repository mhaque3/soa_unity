using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using soa;

public class VillageSim : MonoBehaviour 
{
    public float CasualtyRate;
    public float Casualties;
    public float SupplyRate;
    public float Supply;

    public int destination_id;
    public List<GridCell> gridCells;

    public Canvas uiCanvas;
    public Camera uiCamera;
    public GameObject labelUI;
    GameObject labelInstance;
    RectTransform labelTransform;
    public Vector3 labelPosition;
    Text[] labels;

    // Awake is called before anything else
    void Awake()
    {

    }

    // Use this for initialization upon activation
	void Start () 
    {
        uiCanvas = (Canvas)Object.FindObjectOfType<Canvas>();
        GameObject MainCamera = GameObject.Find("Main Camera");
        uiCamera = MainCamera.GetComponent<Camera>();
        labelInstance = labelUI;
        labelInstance.transform.SetParent(uiCanvas.transform, false);
        labelTransform = labelInstance.transform as RectTransform;
        labels = labelInstance.GetComponentsInChildren<Text>();
	}

    float simTimer;
    public float simInterval;
	// Update is called once per frame
	void Update () 
    {
        simTimer += Time.deltaTime;
        if (simTimer > simInterval)
        {
            // Update resource counts
            Casualties += CasualtyRate;

            Supply -= SupplyRate;
            if (Supply < 0f)
                Supply = 0f;

            simTimer = 0f;
        }

        labelPosition = uiCamera.WorldToScreenPoint(transform.position + new Vector3(0, 0, -1f)) - uiCanvas.transform.position;
        labelTransform.anchoredPosition = new Vector2(labelPosition.x, labelPosition.y);
        labels[0].text = Casualties.ToString("n0");
	}
}
