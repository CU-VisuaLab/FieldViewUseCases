using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideBars : MonoBehaviour {
    private float barHeight;
    private CollectedImage[] collectedImages;
    private GameObject collectedImagePoints;
    private GameObject cameraObject;
    private float maxHeight;
    private float maxWidth;
    private bool maxHeightSet;

	void Start ()
    {
        cameraObject = Camera.main.gameObject;
        collectedImages = FindObjectsOfType<CollectedImage>();
        barHeight = transform.Find("RightBar").localScale.y;
        maxHeight = 0;
        Material mat = Resources.Load("OffscreenDatapoint") as Material;
        maxHeightSet = false;
        foreach(CollectedImage img in collectedImages)
        {
            GameObject newPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newPoint.transform.name = img.transform.name;
            newPoint.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            newPoint.transform.parent = transform;
            newPoint.GetComponent<Renderer>().material = mat;
            maxHeight = Mathf.Max(maxHeight, img.transform.position.y);
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!maxHeightSet)
        {
            float tempHeight = maxHeight;
            maxHeight = 0;
            bool allSet = true;
            foreach (CollectedImage img in collectedImages)
            {
                if (img.initializedSphere())
                {
                    maxHeight = Mathf.Max(maxHeight, img.transform.Find("Sphere").position.y);
                    
                }
                else
                {
                    allSet = false;
                }
            }
            maxHeightSet = allSet;
            if (!allSet) maxHeight = tempHeight;
        }
        UpdateCollectedImages();
	}

    private void UpdateCollectedImages()
    {
        Vector3 centerPoint = cameraObject.transform.position;
        foreach (CollectedImage img in collectedImages)
        {
            if (InFieldOfView(img.transform.position))
            {
                transform.Find(img.transform.name).gameObject.SetActive(false);
                continue;
            }
            else transform.Find(img.transform.name).gameObject.SetActive(true);
            float v1x = transform.position.x - centerPoint.x;
            float v1z = transform.position.z - centerPoint.z;
            float v2x = img.transform.position.x - centerPoint.x;
            float v2z = img.transform.position.z - centerPoint.z;
            float angle = Mathf.Atan2(v1z, v1x) - Mathf.Atan2(v2z, v2x);
            float pointHeight = img.transform.Find("Sphere").position.y / (maxHeight - cameraObject.transform.position.y) * barHeight - 0.5f * barHeight;
            if (angle >= 0 && angle < Mathf.PI)
            {
                transform.Find(img.transform.name).localPosition = new Vector3(.3f + 0.1f * (angle / Mathf.PI), pointHeight, 0);
                float scale = 0.05f * (0.45f - transform.Find(img.transform.name).localPosition.x) / 0.1f;
                transform.Find(img.transform.name).localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                transform.Find(img.transform.name).localPosition = new Vector3(-.3f + 0.1f * (angle / (1.5f * Mathf.PI)), pointHeight, 0);
                float scale = 0.05f * (0.45f + transform.Find(img.transform.name).localPosition.x) / 0.1f;
                transform.Find(img.transform.name).localScale = new Vector3(scale,scale,scale);
            }
            float percentage = img.temperatureValue / 40f;
            Vector3 labColor = new Vector3(50, (percentage - 0.5f) * 128, (percentage - 0.5f) * 128);
            Vector3 rgbColor = LabToRGB.Convert(labColor);
            transform.Find(img.transform.name).GetComponent<Renderer>().material.color = new Color(rgbColor.x, rgbColor.y, rgbColor.z, 0.65f);
        }
    }

    private bool InFieldOfView(Vector3 pos)
    {
        Vector3 groundedPos = new Vector3(pos.x, cameraObject.transform.position.y, pos.z);
        Vector3 groundedForward = new Vector3(cameraObject.transform.forward.x, 0, cameraObject.transform.forward.z);
        Vector3 targetDir = groundedPos - cameraObject.transform.position;
        return Vector3.Angle(targetDir, groundedForward) < 20;

    }
}
