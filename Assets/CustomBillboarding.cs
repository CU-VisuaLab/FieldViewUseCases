using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBillboarding : MonoBehaviour {
    private GameObject cam;
	// Use this for initialization
	void Start () {
        cam = Camera.main.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(cam.transform);
	}
}
