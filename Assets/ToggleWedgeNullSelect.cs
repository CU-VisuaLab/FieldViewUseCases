using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ToggleWedgeNullSelect : MonoBehaviour, IInputClickHandler
{

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (FindObjectOfType<GazeManager>().HitInfo.transform != null && FindObjectOfType<GazeManager>().HitInfo.transform.GetComponent<CollectedImage>() != null)
        {
            FindObjectOfType<GazeManager>().HitInfo.transform.root.GetComponent<CollectedImage>().ToggleWedge();

        }

        CollectedImage[] images = FindObjectsOfType<CollectedImage>();
        float minAngle = float.MaxValue;
        CollectedImage minImage = null;
        foreach(CollectedImage image in images)
        {
            if (AngleToward(image.transform.position) < minAngle)
            {
                minAngle = AngleToward(image.transform.position);
                minImage = image;
            }
        }
        minImage.ToggleWedge();
    }

    public float AngleToward(Vector3 pos)
    {
        Vector3 targetDir = pos - Camera.main.transform.position;
        return Vector3.Angle(targetDir, Camera.main.transform.forward);
    }
}
