using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine.Networking;
#if NETFX_CORE
using Windows.Storage.Streams;
#endif
public class ScorchImage : MonoBehaviour
{
    public int scorch_rate;
    public string image_url;
    public string user;
    public float longitude;
    public float latitude;
}
public class Cell : MonoBehaviour
{
    public int cell_number;
    public float max_longitude;
    public float min_longitude;
    public float max_latitude;
    public float min_latitude;
    public List<ScorchImage> images;
}

public class GetCellCoverage : MonoBehaviour {
    public string hostname;
    public Material[] materials;
    private bool synced = true;
    private bool updated = true;
    private List<Cell> cell_boundaries;
    private List<ScorchImage> raw_data;
    private string dummy_str = "[{\"longitude\": \"-105.2679000000\", \"latitude\": \"40.0057500000\", \"scorch_rate\": \"0\", \"image_url\": \"/home/pi/Visualab-LocalServer/scorch_images/cell_0.jpg\", \"user\": \"billy\"}, {\"longitude\": \"-105.2679000000\", \"latitude\": \"40.0059500000\", \"scorch_rate\": \"40\", \"image_url\": \"/home/pi/Visualab-LocalServer/scorch_images/cell_1.jpg\", \"user\": \"preplanning\" }, {\"longitude\": \"-105.2671000000\", \"latitude\": \"40.0061500000\", \"scorch_rate\": \"80\", \"image_url\": \"/home/pi/Visualab-LocalServer/scorch_images/cell_14.jpg\", \"user\": \"jamie\"}, {\"longitude\": \"-105.2679000000\", \"latitude\": \"40.0061500000\", \"scorch_rate\": \"20\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_2.jpg\", \"user\": \"preplanning\"}, {\"longitude\": \"-105.2677000000\", \"latitude\": \"40.0057500000\", \"scorch_rate\": \"20\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_3.jpg\", \"user\": \"marvin\"}, {\"longitude\": \"-105.2677000000\", \"latitude\": \"40.0059500000\", \"scorch_rate\": \"40\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_4.jpg\", \"user\": \"billy\"}, {\"longitude\": \"-105.2675000000\", \"latitude\": \"40.0057500000\", \"scorch_rate\": \"40\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_6.jpg\", \"user\": \"marvin\"}, {\"longitude\": \"-105.2675000000\", \"latitude\": \"40.0059500000\", \"scorch_rate\": \"60\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_7.jpg\", \"user\": \"billy\"}, {\"longitude\": \"-105.2675000000\", \"latitude\": \"40.0061500000\", \"scorch_rate\": \"80\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_8.jpg\", \"user\": \"jamie\"}, {\"longitude\": \"-105.2673000000\", \"latitude\": \"40.0059500000\", \"scorch_rate\": \"60\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_10.jpg\", \"user\": \"jamie\"}, {\"longitude\": \"-105.2673000000\", \"latitude\": \"40.0061500000\", \"scorch_rate\": \"100\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_11.jpg\", \"user\": \"billy\"}, {\"longitude\": \"-105.2671000000\", \"latitude\": \"40.0057500000\", \"scorch_rate\": \"80\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_12.jpg\", \"user\": \"marvin\"}, {\"longitude\": \"-105.2671000000\", \"latitude\": \"40.0061500000\", \"scorch_rate\": \"80\", \"image_url\": \"/home/pi/VisuaLab-LocalServer/scorch_images/cell_14.jpg\", \"user\": \"preplanning\"}]";
    private List<GameObject> CollectedGameObjects;
    private List<GameObject> MissingGameObjects;
    private List<GameObject> SecondCollectedGameObjects;
    private GameObject cam;
    private int current_cell;

    // Use this for initialization
    void Start() {
        cam = Camera.main.gameObject;
        current_cell = -1;
        initializeCells();
        updateCoverage();
        renderVisualization();
    }

    // Update is called once per frame
    void Update()
    {
        if (synced)
        {
            updateCoverage();
            if (updated)
            {
                renderVisualization();
                updated = false;
            }
        }
        updateCameraPosition();
    }

    // Note that this is specific to our location, Farrand Field at CU-Boulder
    private void initializeCells()
    {
        cell_boundaries = new List<Cell>();
        CollectedGameObjects = new List<GameObject>();
        SecondCollectedGameObjects = new List<GameObject>();
        MissingGameObjects = new List<GameObject>();
        for (var i = 0; i < 15; i++)
        {
            Cell newCell = new global::Cell();
            newCell.cell_number = i;

            // Set latitude bounds
            if (i % 3 == 0) { newCell.min_latitude = 40.00565f; newCell.max_latitude = 40.00585f; }
            else if (i % 3 == 1) { newCell.min_latitude = 40.00585f; newCell.max_latitude = 40.00605f; }
            else { newCell.min_latitude = 40.00605f; newCell.max_latitude = 40.00625f; }

            // Set the longitude bounds
            if (i / 5 == 0) { newCell.min_longitude = -105.268f; newCell.max_longitude = -105.2678f; }
            else if (i / 5 == 1) { newCell.min_longitude = -105.2678f; newCell.max_longitude = -105.2676f; }
            else if (i / 5 == 2) { newCell.min_longitude = -105.2676f; newCell.max_longitude = -105.2674f; }
            else if (i / 5 == 3) { newCell.min_longitude = -105.2674f; newCell.max_longitude = -105.2672f; }
            else { newCell.min_longitude = -105.2672f; newCell.max_longitude = -105.267f; }

            cell_boundaries.Add(newCell);
            
            CollectedGameObjects.Add(GameObject.Find("Cell" + i + "Collected"));
            if (GameObject.Find("Cell" + i + "Collected_1") != null)
                SecondCollectedGameObjects.Add(GameObject.Find("Cell" + i + "Collected_1"));
            else
                SecondCollectedGameObjects.Add(GameObject.Find("Cell" + i + "Collected"));
            // NOTE: Statically define a second collected image for simulation only
            MissingGameObjects.Add(GameObject.Find("Cell" + i + "Missing"));
            CollectedGameObjects[i].GetComponent<Renderer>().enabled = false;
            cell_boundaries[i].images = new List<ScorchImage>();
        }
        CollectedGameObjects.Add(GameObject.Find("Cell1Collected_1"));
        CollectedGameObjects[15].GetComponent<Renderer>().enabled = false;
        CollectedGameObjects.Add(GameObject.Find("Cell2Collected_1"));
        CollectedGameObjects[16].GetComponent<Renderer>().enabled = false;
    }

    private void updateCoverage()
    {
        synced = false;
        raw_data = new List<ScorchImage>();

#if NETFX_CORE
        /* NOTE THIS BLOCK SHOULD BE USED FOR REMOTE CONNECTION ON THE HOLOLENS: COMMENTED OUT FOR DEMOUri uri = new System.Uri(hostname + "/fieldview/getAllJson");
        string result = "";
        Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
        using (var httpClient = new Windows.Web.Http.HttpClient())
        {
            try
            {
                httpResponse = await httpClient.GetAsync(uri);
                httpResponse.EnsureSuccessStatusCode();
                result = await httpResponse.Content.ReadAsStringAsync();
                raw_data = JsonConvert.DeserializeObject<List<ScorchImage>>(result);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }*/
        raw_data = JsonConvert.DeserializeObject<List<ScorchImage>>(dummy_str);
        Debug.Log(raw_data.Count);
        Debug.Log(raw_data[0].image_url);
#else

        raw_data = JsonConvert.DeserializeObject<List<ScorchImage>>(dummy_str);
#endif
        for (var i = 0; i < raw_data.Count; i++)
        {
            int cell_no = calculateCellNum(raw_data[i].longitude, raw_data[i].latitude);
            if (cell_no < 0) continue;
            ScorchImage scorch_image = new ScorchImage();
            scorch_image.latitude = raw_data[i].latitude;
            scorch_image.longitude = raw_data[i].longitude;
            scorch_image.image_url = raw_data[i].image_url;
            scorch_image.scorch_rate = raw_data[i].scorch_rate;
            scorch_image.user = raw_data[i].user;
            updated = true;
            cell_boundaries[cell_no].images.Add(scorch_image);
        }
        synced = true;
    }
    private int calculateCellNum(float longitude, float latitude)
    {
        int multiplier = -1, modulo = -1;
        if (longitude > -105.26720f && longitude < -105.267f) multiplier = 4;
        else if (longitude > -105.26740f && longitude < -105.26720f) multiplier = 3;
        else if (longitude > -105.26760f && longitude < -105.26740f) multiplier = 2;
        else if (longitude > -105.26780f && longitude < -105.26760f) multiplier = 1;
        else if (longitude > -105.2680f && longitude < -105.2678f) multiplier = 0;
        else Debug.Log("Bad Longitude: " + longitude);

        if (latitude < 40.00625f && latitude > 40.00605f) modulo = 2;
        else if (latitude < 40.00605f && latitude > 40.00585f) modulo = 1;
        else if (latitude < 40.00585f && latitude > 40.00565f) modulo = 0;
        else Debug.Log("Bad Lat: " + latitude);

        if (multiplier < 0 || modulo < 0) return -1;
        else return 3 * multiplier + modulo;
    }
    private void renderVisualization()
    {
        for (var i = 0; i < cell_boundaries.Count; i++)
        {
            //TODO: JUST TAKING THE FIRST ONE FOR NOW
            if (cell_boundaries[i].images.Count > 0)
            {
                MissingGameObjects[i].SetActive(false);
                CollectedGameObjects[i].GetComponent<Renderer>().enabled = true;
                /*if (SecondCollectedGameObjects[i] != null)
                    SecondCollectedGameObjects[i].GetComponent<Renderer>().enabled = false;*/
                CollectedGameObjects[i].transform.Find("Image").Find("Text").GetComponent<Text>().text = cell_boundaries[i].images[0].user;
                CollectedGameObjects[i].GetComponent<Renderer>().material = materials[cell_boundaries[i].images[0].scorch_rate / 20];
                float x_val = 111.321f * (cell_boundaries[i].images[0].longitude - (-105.268f)) / (-105.267f + 105.268f);
                float z_val = 66.7121f * (1 - (cell_boundaries[i].images[0].latitude - (40.00625f)) / (40.00565f - 40.00625f));
                CollectedGameObjects[i].transform.localPosition = new Vector3(x_val, CollectedGameObjects[i].transform.localPosition.y, z_val);
                //Task<BitmapImage> img_task = Task.Run(() => LoadImage(new Uri(hostname + "/fieldview/getImage/")));
                //BitmapImage img = img_task.Result;
                //CollectedGameObjects[i].transform.Find("Image").GetComponent<Image>() = img;
                //getImage(CollectedGameObjects[i].transform.Find("Image").gameObject, hostname + "/fieldview/getImage/home/pi/Visualab-LocalServer/scorch_images/cell0.jpg");
                //StartCoroutine(getImage(CollectedGameObjects[i].transform.Find("Image").gameObject, hostname + "/fieldview/getImage" + cell_boundaries[i].image_url));
                //StartCoroutine(getImage(CollectedGameObjects[i].transform.Find("Image").gameObject, "http://cmci.colorado.edu/mattwhitlock/img/metamorphosis/butterfly.png");
            }
        }
    }

    IEnumerator getImage(GameObject imgObject, string url)
    {
        WWW www = new WWW(url.Replace("http://10.201.149.98:5000/fieldview/getImage", ""));
        Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        yield return www;
        Debug.Log("LoadImage: " + url.Replace("http://10.201.149.98:5000/fieldview/getImage", ""));
        www.LoadImageIntoTexture(tex);
        Debug.Log("Loaded Image, making sprite");
        imgObject.GetComponent<Image>().sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
        Debug.Log("Made sprite, all done");

        Destroy(tex);

    }

    public async static Task<string> LoadImage(Uri uri)
    {
#if NETFX_CORE
        /*BitmapImage bitmapImage = new BitmapImage();

        try
        {
            using (Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient())
            {
                using (var response = await client.GetAsync(uri))
                {
                    response.EnsureSuccessStatusCode();

                    using (MemoryStream inputStream = new MemoryStream())
                    {
                        await inputStream.CopyToAsync(inputStream);
                        bitmapImage.SetSource(inputStream.AsRandomAccessStream());
                    }
                }
            }
            return bitmapImage;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to load the image: {0}", ex.Message);
        }

        return null;*/
#endif
        return null;
    }
    /*
     * NOTE: This function allows us to simulate adding a point to the cell coverage.
     * With proper connectivity, this should be done by querying the datastore, 
     * however our Raspberry Pi + Router ATAK simulation does not provide strong enough signal
     */
    public void add_point(int cell_no)
    {
        if (CollectedGameObjects[cell_no].GetComponent<Renderer>().enabled)
        {
            CollectedGameObjects[cell_no].GetComponent<Renderer>().enabled = false;
            if (cell_no < 15) MissingGameObjects[cell_no].SetActive(true);
            else CollectedGameObjects[cell_no - 14].transform.localScale /= 0.4f;
        }
        else
        {
            CollectedGameObjects[cell_no].GetComponent<Renderer>().enabled = true;
            if (cell_no < 15) MissingGameObjects[cell_no].SetActive(false);
            else CollectedGameObjects[cell_no - 14].transform.localScale *= 0.4f;
        }
    }
    private void updateCameraPosition()
    {
        float cameraLong = -105.268f + (0.001f * cam.transform.position.x / 111.321f);
        float cameraLat = 40.00625f + (0.0006f * (cam.transform.position.z) / 66.7121f);
        if (calculateCellNum(cameraLong,cameraLat) != current_cell)
        {
            Debug.Log("Now in " + calculateCellNum(cameraLong, cameraLat));
            if (current_cell != -1 && !MissingGameObjects[current_cell].GetComponent<Renderer>().enabled)
            {
                Debug.Log("Re-enable cell " + current_cell);
                MissingGameObjects[current_cell].GetComponent<Renderer>().enabled = true;
            }
            current_cell = calculateCellNum(cameraLong, cameraLat);
            if (current_cell != -1 && !CollectedGameObjects[current_cell].GetComponent<Renderer>().enabled)
            {
                Debug.Log("Disable cell " + current_cell);
                MissingGameObjects[current_cell].GetComponent<Renderer>().enabled = false;
            }
        }
        //Debug.Log(calculateCellNum(cameraLong, cameraLat));
    }
}
