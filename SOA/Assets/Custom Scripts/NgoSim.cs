using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NgoSim : MonoBehaviour 
{
    public float CasualtyRate;
    public float Casualties;
    public float CivilianRate;
    public float Civilians;
    public float SupplyRate;
    public float Supply;

    public Canvas uiCanvas;
    public Camera uiCamera;
    public GameObject labelUI;
    GameObject labelInstance;
    RectTransform labelTransform;
    public Vector3 labelPosition;
    Text[] labels;

    // Use this for initialization
    void Start()
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
    void Update()
    {
        simTimer += Time.deltaTime;
        if (simTimer > simInterval)
        {
            Civilians += CivilianRate;
            Casualties += CasualtyRate;

            Supply -= SupplyRate;
            if (Supply < 0f)
                Supply = 0f;

            simTimer = 0f;
        }

        labelPosition = uiCamera.WorldToScreenPoint(transform.position + new Vector3(0, 0, -1f)) - uiCanvas.transform.position;
        labelTransform.anchoredPosition = new Vector2(labelPosition.x, labelPosition.y);
        labels[0].text = Casualties.ToString("n1");
        labels[1].text = Civilians.ToString("n1");
    }
}
