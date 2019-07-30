using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingArrow : MonoBehaviour {
    public float bounceHeight;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.localPosition = new Vector3(0, bounceHeight * Mathf.Sin(Time.time * 5), 0);
    }
}
