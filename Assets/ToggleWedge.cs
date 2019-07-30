using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ToggleWedge : MonoBehaviour, IInputClickHandler {
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }
    public void OnInputClicked(InputClickedEventData eventData)
    {
        transform.root.GetComponent<CollectedImage>().ToggleWedge();
    }

}
