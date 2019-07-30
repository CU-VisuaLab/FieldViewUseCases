using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpInfo : MonoBehaviour {
    public List<CollectedImage> leftImages;
    public List<CollectedImage> rightImages;

    private GameObject cameraObject;
    private GameObject leftArrow;
    private GameObject rightArrow;
    private GameObject leftStatus;
    private GameObject rightStatus;

    // Use this for initialization
    void Start () {
        cameraObject = Camera.main.gameObject;
        InitializeCollectedImages();
        leftArrow = transform.Find("LeftArrow").gameObject;
        rightArrow = transform.Find("RightArrow").gameObject;
        leftStatus = transform.Find("LeftStatus").gameObject;
        rightStatus = transform.Find("RightStatus").gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        UpdateImageLists();
        UpdateArrows();
	}
    private void InitializeCollectedImages()
    {
        leftImages = new List<CollectedImage>();
        rightImages = new List<CollectedImage>();
        CollectedImage[] collectedImages = FindObjectsOfType<CollectedImage>();
        Vector3 centerPoint = cameraObject.transform.position;
        foreach (CollectedImage img in collectedImages)
        {
            float v1x = transform.position.x - centerPoint.x;
            float v1z = transform.position.z - centerPoint.z;
            float v2x = img.transform.position.x - centerPoint.x;
            float v2z = img.transform.position.z - centerPoint.z;
            float angle = Mathf.Atan2(v1z, v1x) - Mathf.Atan2(v2z, v2x);
            if (angle >= 0 && angle < Mathf.PI) rightImages.Add(img);
            else leftImages.Add(img);
            InFieldOfView(new Vector3(img.transform.position.x,cameraObject.transform.position.y, img.transform.position.z));
        }
    }
    private void UpdateImageLists()
    {
        Vector3 centerPoint = cameraObject.transform.position;
        for (var i = 0; i < leftImages.Count; i++)
        {
            CollectedImage leftImg = leftImages[i];
            float v1x = transform.position.x - centerPoint.x;
            float v1z = transform.position.z - centerPoint.z;
            float v2x = leftImg.transform.position.x - centerPoint.x;
            float v2z = leftImg.transform.position.z - centerPoint.z;
            float angle = Mathf.Atan2(v1x, v1z) - Mathf.Atan2(v2x, v2z);
            if (!(angle >= 0 && angle < Mathf.PI))
            {
                rightImages.Add(leftImg);
                leftImages.Remove(leftImg);
                i--;
            }
        }
        for (var i = 0; i < rightImages.Count; i++)
        {
            CollectedImage rightImg = rightImages[i];
            float v1x = transform.position.x - centerPoint.x;
            float v1z = transform.position.z - centerPoint.z;
            float v2x = rightImg.transform.position.x - centerPoint.x;
            float v2z = rightImg.transform.position.z - centerPoint.z;
            float angle = Mathf.Atan2(v1x, v1z) - Mathf.Atan2(v2x, v2z);
            if (angle >= 0 && angle < Mathf.PI)
            {
                leftImages.Add(rightImg);
                rightImages.Remove(rightImg);
                i--;
            }
        }
    }
    private void UpdateArrows()
    {
        float leftTotal = 0;
        int leftFieldOfViewCount = 0, rightFieldOfViewCount = 0;
        foreach (CollectedImage leftImg in leftImages)
        {
            if (InFieldOfView(leftImg.transform.position))
            {
                leftFieldOfViewCount++;
            }
            else
            {
                leftTotal += leftImg.temperatureValue;
            }
        }
        float rightTotal = 0;
        foreach (CollectedImage rightImg in rightImages)
        {
            if (InFieldOfView(rightImg.transform.position))
            {
                rightFieldOfViewCount++;
            }
            else
            {
                rightTotal += rightImg.temperatureValue;
            }
        }

        if (leftImages.Count - leftFieldOfViewCount == 0)
        {
            leftArrow.SetActive(false);
            leftStatus.SetActive(false);
        }
        else
        {
            leftArrow.SetActive(true);
            leftStatus.SetActive(true);
            float average = leftTotal / (leftImages.Count - leftFieldOfViewCount);
            float percentage = average / 40f;
            Vector3 labColor = new Vector3(50, (percentage - 0.5f) * 128, (percentage - 0.5f) * 128);
            Vector3 rgbColor = LabToRGB.Convert(labColor);
            foreach (Transform child in leftArrow.transform)
            {
                child.GetComponent<Renderer>().material.color = new Color(rgbColor.x, rgbColor.y, rgbColor.z);
            }
        }
        if (rightImages.Count - rightFieldOfViewCount == 0)
        {
            rightArrow.SetActive(false);
            rightStatus.SetActive(false);
        }
        else
        {
            rightArrow.SetActive(true); 
            rightStatus.SetActive(true);
            float average = rightTotal / (rightImages.Count - rightFieldOfViewCount);
            float percentage = average / 40f;
            Vector3 labColor = new Vector3(50, (percentage - 0.5f) * 128, (percentage - 0.5f) * 128);
            Vector3 rgbColor = LabToRGB.Convert(labColor);
            foreach (Transform child in rightArrow.transform)
            {
                child.GetComponent<Renderer>().material.color = new Color(rgbColor.x, rgbColor.y, rgbColor.z);
            }
        }

        leftStatus.transform.Find("StatusText").GetComponent<Text>().text = (leftImages.Count - leftFieldOfViewCount).ToString();
        rightStatus.transform.Find("StatusText").GetComponent<Text>().text = (rightImages.Count - rightFieldOfViewCount).ToString();
    }

    private bool InFieldOfView(Vector3 pos)
    {
        //Vector3 groundedPos = new Vector3(pos.x, cameraObject.transform.position.y, pos.z);
        Vector3 groundedForward = new Vector3(cameraObject.transform.forward.x, 0, cameraObject.transform.forward.z);
        Vector3 targetDir = pos - cameraObject.transform.position;
        return Vector3.Angle(targetDir, groundedForward) < 20;
         
    }
}
