
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class MountainData : MonoBehaviour {

    private CSVReader CSV;
    public GameObject DataPoint0, DataPoint1;
    private List<GameObject> dataPoints;
    private List<Vector3> dataPointTops;
    private GameObject focalObject;

    // Use this for initialization
    void Start ()
    {
        CSV = GameObject.Find("CSVReader").GetComponent<CSVReader>();
        CSV.ParseData();
        CSV.SetDataSets();

        string[] headers = CSV.GetColumnHeads();
        
        LoadData();
    }

    // Update is called once per frame
    void Update ()
    {
        GameObject newFocalObject = GetFocalObject();
        if (focalObject == null || focalObject != newFocalObject)
        {
            if (focalObject != null)
            {
                focalObject.GetComponentInChildren<Canvas>().enabled = false;
            }
            focalObject = newFocalObject;
            focalObject.GetComponentInChildren<Canvas>().enabled = true;
        }

    }
    
    void LoadData()
    {
        dataPointTops = new List<Vector3>();
        dataPoints = new List<GameObject>();
        foreach (string[] row in CSV.rowData.GetRange(1,CSV.rowData.Count - 2))
        {
            string name = row[0];
            double longitude = double.Parse(row[1]);
            double latitude = double.Parse(row[2]);
            int elevation = int.Parse(row[3]);
            bool isNew = row[4].ToLower().Contains("true");

            GameObject dataPoint;
            if (!isNew) dataPoint = Instantiate(DataPoint0);
            else dataPoint = Instantiate(DataPoint1);
            dataPoint.transform.parent = transform;
            dataPoint.transform.name = name;
            double xPos = 140 * (longitude + 109) / 7;
            double zPos = 80 * (latitude - 37) / 4;
            dataPoint.transform.localPosition = new Vector3((float)xPos, (elevation - 14000) / 20f, (float)zPos);
            dataPoint.transform.localScale = new Vector3(1, (elevation - 14000) / 20f, 1);
            dataPoint.transform.Find("Canvas/Image/Text").GetComponent<Text>().text = name + " (Elevation " + elevation + ")\n\n(" +
                longitude + "," + latitude + ")";
            dataPoint.transform.Find("Canvas").position =
                dataPoint.transform.position + new Vector3(0, transform.localScale.y * (elevation - 14000) / 20f + 0.3f, 0);
            dataPoint.transform.Find("Canvas").localScale = new Vector3(8,8 / dataPoint.transform.localScale.y, 8);
            dataPoint.transform.Find("Canvas").GetComponent<Canvas>().enabled = false;
            dataPointTops.Add(dataPoint.transform.position + new Vector3(0, transform.localScale.y * (elevation - 14000) / 20, 0));
            dataPoints.Add(dataPoint);
        }
    }
    private GameObject GetFocalObject()
    {
        float minDist = float.MaxValue;
        GameObject focalObject = dataPoints[0];
        for (var i = 0; i < dataPointTops.Count; i++)
        {
            Vector3 targetDir = dataPointTops[i] - Camera.main.transform.position;
            float angleBetween = Vector3.Angle(targetDir, Camera.main.transform.forward);
            if (angleBetween < minDist)
            {
                minDist = angleBetween;
                focalObject = dataPoints[i];
            }
        }
        return focalObject;
    }
}
