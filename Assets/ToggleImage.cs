using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ToggleImage : MonoBehaviour, IInputClickHandler {
    private GameObject imgObject;
    private GameObject imgBorder;

    // Use this for initialization
    void Start () {
        imgObject = transform.root.Find("SourceImage").gameObject;
        imgBorder = transform.root.Find("BorderImages").gameObject;
    }
	
	// Update is called once per frame
	void Update () {

    }
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (imgObject.activeSelf)
        {
            imgObject.SetActive(false);
            imgBorder.SetActive(false);
        }
        else
        {
            imgObject.SetActive(true);
            imgBorder.SetActive(true);

        }
    }

}
