using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using soa;

public class VillageSim : MonoBehaviour 
{
    SoaSite thisSoaSite;
    SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> beliefDictionary;

    public float CasualtyRate;
    public float Casualties;
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
	void Start () 
    {
        uiCanvas = (Canvas)Object.FindObjectOfType<Canvas>();
        GameObject MainCamera = GameObject.Find("Main Camera");
        uiCamera = MainCamera.GetComponent<Camera>();
        labelInstance = labelUI;
        labelInstance.transform.SetParent(uiCanvas.transform, false);
        labelTransform = labelInstance.transform as RectTransform;
        labels = labelInstance.GetComponentsInChildren<Text>();
        thisSoaSite = gameObject.GetComponent<SoaSite>();
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

            // Broadcast status
            beliefDictionary = thisSoaSite.getBeliefDictionary();
            Belief_Village b = (Belief_Village)beliefDictionary[Belief.BeliefType.VILLAGE][thisSoaSite.unique_id];
            if (b != null)
            {
                // Add the same belief but just update the supply, casualties, and civilians fields
                thisSoaSite.addBelief(new Belief_Village(b.getId(), b.getCells(), Supply, Casualties));
            }
        }

        labelPosition = uiCamera.WorldToScreenPoint(transform.position + new Vector3(0, 0, -1f)) - uiCanvas.transform.position;
        labelTransform.anchoredPosition = new Vector2(labelPosition.x, labelPosition.y);
        labels[0].text = Casualties.ToString("n0");
	}
}
