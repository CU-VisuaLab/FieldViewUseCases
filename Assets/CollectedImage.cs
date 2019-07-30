using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectedImage : MonoBehaviour {
    private GameObject cameraObject;
    private GameObject dropObject;
    private GameObject sphereObject;
    private GameObject imageObject;
    private GameObject borderObject;
    public float proximityThreshold = 20;
    [Tooltip("Assume values between 0 and 40 Celcius")]
    public float temperatureValue;
    private GameObject wedgeObject;
    private bool wedgeShowing;
    private bool sphereInitialized;

	// Use this for initialization
	void Start ()
    {
        wedgeShowing = false;
        sphereInitialized = false;
        cameraObject = Camera.main.gameObject;
        wedgeObject = transform.Find("Wedge").gameObject;
        sphereObject = transform.Find("Sphere").gameObject;
        imageObject = transform.Find("SourceImage").gameObject;
        borderObject = transform.Find("BorderImages").gameObject;
        InitializeDropObject();
        EncodeTemperature();
	}
	
	// Update is called once per frame
	void Update ()
    {
        float a = proximityThreshold / Vector3.Distance(cameraObject.transform.position, new Vector3(transform.position.x, cameraObject.transform.position.y, transform.position.z));
        if (wedgeShowing)
        {
            sphereObject.SetActive(false);
            imageObject.SetActive(true);
            borderObject.SetActive(true);
            if (InFieldOfView(transform.position, 20) && Vector3.Distance(new Vector3(transform.position.x, cameraObject.transform.position.y, transform.position.z), cameraObject.transform.position) < proximityThreshold)

            {
                dropObject.SetActive(true);
                wedgeObject.SetActive(false);
                transform.Find("SourceImage").localPosition = Vector3.zero;
                transform.Find("BorderImages").localPosition = Vector3.zero;
            }
            else
            {
                dropObject.SetActive(false);
                wedgeObject.SetActive(true);

                if (!InFieldOfView(transform.position, 8, true))
                {
                    if (Vector3.Distance(transform.position, cameraObject.transform.position) > proximityThreshold)
                    {
                        transform.Find("SourceImage").position = new Vector3((1 - a) * cameraObject.transform.position.x + a * transform.position.x, cameraObject.transform.position.y, (1 - a) * cameraObject.transform.position.z + a * transform.position.z);
                    }
                    else transform.Find("SourceImage").position = new Vector3(transform.position.x, cameraObject.transform.position.y, transform.position.z);
                    transform.Find("BorderImages").position = transform.Find("SourceImage").position;
                    UpdateWedge(false);
                }
                else UpdateWedge(true);
            }
        }
        else
        {
            dropObject.SetActive(false);
            wedgeObject.SetActive(false);
            sphereObject.SetActive(true);
            imageObject.SetActive(false);
            borderObject.SetActive(false);
            sphereObject.transform.position = new Vector3((1 - a) * cameraObject.transform.position.x + a * transform.position.x, (1 - 0.75f * a) * cameraObject.transform.position.y + 0.75f * a * transform.position.y, (1 - a) * cameraObject.transform.position.z + a * transform.position.z);
            sphereInitialized = true;
        }
    }

    private void InitializeDropObject()
    {
        dropObject = new GameObject();
        dropObject.transform.parent = transform;
        dropObject.transform.localPosition = Vector3.zero;
        dropObject.transform.name = "DropObject";
        for (var i = GetComponent<RectTransform>().rect.height / 2 + 0.5f; i < transform.position.y + 3.8f; i+=2)
        {
            GameObject newCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            newCylinder.transform.parent = dropObject.transform;
            newCylinder.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);
            newCylinder.transform.localPosition = new Vector3(0, -i, 0);
        }
    }

    private void UpdateWedge(bool offsetWedge)
    {
        if (offsetWedge)
        {
            Vector3 HUDPosition = FindObjectOfType<HeadsUpInfo>().transform.position;
            Vector3 centerPoint = cameraObject.transform.position;
            float v1x = HUDPosition.x - centerPoint.x;
            float v1z = HUDPosition.z - centerPoint.z;
            float v2x = transform.position.x - centerPoint.x;
            float v2z = transform.position.z - centerPoint.z;
            float angle = Mathf.Atan2(v1x, v1z) - Mathf.Atan2(v2x, v2z);
            float angleOffset = Mathf.PI * AngleToward(transform.position, true) / 180f;
            /*float a = Vector3.Distance(cameraObject.transform.position, transform.Find("SourceImage").position);
            float b = 2 * a * Mathf.Asin(angle / 2);
            Vector3 perpendicular = Vector3.Cross(new Vector3(v1x,0,v1z), new Vector3(v2x,0,v2z));*/
            Vector3 updatedForward = Vector3.zero;
            float hudDistance = FindObjectOfType<HeadsUpInfo>().transform.localPosition.z;
            if (angle >= 0 && angle < Mathf.PI)
            {
                //updatedForward = new Vector3(cameraObject.transform.forward.x + Mathf.Sin(angleOffset), 0, cameraObject.transform.forward.z + Mathf.Cos(angleOffset));
                Vector3 leftBarPos = GameObject.Find("LeftBar").transform.position;
                updatedForward = (0.35f * HUDPosition + 0.65f * leftBarPos)  - cameraObject.transform.position ;
            }
            else
            {
                //updatedForward = new Vector3(cameraObject.transform.forward.x + Mathf.Sin(angleOffset), 0, cameraObject.transform.forward.z + Mathf.Cos(angleOffset));
                Vector3 rightBarPos = GameObject.Find("RightBar").transform.position;
                updatedForward = (0.35f * HUDPosition + 0.65f * rightBarPos) - cameraObject.transform.position;
            }
            updatedForward /= Vector3.Magnitude(updatedForward);
            transform.Find("SourceImage").position = cameraObject.transform.position + proximityThreshold * new Vector3(updatedForward.x, 0, updatedForward.z);


            transform.Find("BorderImages").position = transform.Find("SourceImage").position;
        }
        wedgeObject.transform.localScale = new Vector3(0.5f, 0.5f, Vector3.Distance(transform.Find("SourceImage").position, transform.position) - 1f);
        wedgeObject.transform.Find("Cylinder").localScale = new Vector3(1, 0.01f / (Vector3.Distance(transform.Find("SourceImage").position, transform.position) - 1f), 1);
        wedgeObject.transform.LookAt(transform.Find("SourceImage").position);
    }

    private void EncodeTemperature()
    {
        float percentage = temperatureValue / 40f;
        Vector3 labColor = new Vector3(50, (percentage - 0.5f) * 128, (percentage - 0.5f) * 128);
        Vector3 rgbColorVector = LabToRGB.Convert(labColor);
        foreach(Transform child in transform.Find("BorderImages"))
        {
            child.GetComponent<Image>().color = new Color(rgbColorVector.x, rgbColorVector.y, rgbColorVector.z);
        }
        foreach (Transform child in dropObject.transform)
        {
            child.GetComponent<Renderer>().material.color = new Color(rgbColorVector.x, rgbColorVector.y, rgbColorVector.z);
        }
        foreach (Transform child in wedgeObject.transform)
        {
            child.GetComponent<Renderer>().material.color = new Color(rgbColorVector.x, rgbColorVector.y, rgbColorVector.z);
        }
        sphereObject.GetComponent<Renderer>().material.color = new Color(rgbColorVector.x, rgbColorVector.y, rgbColorVector.z);
    }
    private bool InFieldOfView(Vector3 pos, float threshold, bool grounded=false)
    {
        if (grounded)
        {
            Vector3 groundedPos = new Vector3(pos.x, 0, pos.z);
            Vector3 groundedForward = new Vector3(cameraObject.transform.forward.x, 0, cameraObject.transform.forward.z);
            Vector3 targetDir = groundedPos - cameraObject.transform.position;
            return Vector3.Angle(targetDir, groundedForward) < threshold;
        }
        else
        {
            Vector3 targetDir = pos - cameraObject.transform.position;
            return Vector3.Angle(targetDir, cameraObject.transform.forward) < threshold;
        }
    }
    private float AngleToward(Vector3 pos, bool grounded = false)
    {
        if (grounded)
        {
            Vector3 groundedPos = new Vector3(pos.x, 0, pos.z);
            Vector3 groundedForward = new Vector3(cameraObject.transform.forward.x, 0, cameraObject.transform.forward.z);
            Vector3 targetDir = groundedPos - cameraObject.transform.position;
            return Vector3.Angle(targetDir, groundedForward);
        }
        else
        {
            Vector3 targetDir = pos - cameraObject.transform.position;
            return Vector3.Angle(targetDir, cameraObject.transform.forward);
        }
    }
    public void ToggleWedge()
    {
        wedgeShowing = !wedgeShowing;
    }
    public bool initializedSphere()
    {
        return sphereInitialized;
    }
}
