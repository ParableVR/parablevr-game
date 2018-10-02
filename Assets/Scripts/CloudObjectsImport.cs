using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

namespace parable
{
    public class CloudObjectsImport : MonoBehaviour {
        public string scenarioID;
        private string apiEndpoint = "https://parablevr-game-api.azurewebsites.net/api/locations/get/scenario/id/{0}?code=PULseLqY2AIy7Tj8wBjW9DmQL4IS4vzCp9eAOWF1y/DJqMBIu4wPow==";

        // Use this for initialization
        void Start () {
            if (!string.IsNullOrEmpty(scenarioID))
            {
                Transform cloudObjParent = GameObject.Find("/SceneContent/CloudObjects").transform;
                WWW ep = new WWW(string.Format(apiEndpoint, scenarioID));
                while (!ep.isDone) ;

                if (!string.IsNullOrEmpty(ep.text))
                {
                    ScenarioResponse res = JsonConvert.DeserializeObject<ScenarioResponse>(ep.text);

                    foreach (Object obj in res.scenario.objects)
                    {
                        GameObject gameObject = (GameObject)Instantiate(
                            Resources.Load(obj.path, typeof(GameObject)),
                            new Vector3(obj.x, obj.y, obj.z),
                            new Quaternion(obj.pitch, obj.yaw, obj.roll, 1),
                            cloudObjParent);

                        gameObject.transform.localScale = new Vector3(obj.scale_x, obj.scale_y, obj.scale_z);

                        gameObject.name = obj.name;
                        gameObject.AddComponent<ObjectPopupName>(); // show name on focus

                        // components required for picking up the object
                        gameObject.AddComponent<HoloToolkit.Unity.InputModule.HandDraggable>();
                        gameObject.AddComponent<Rigidbody>();
                        gameObject.AddComponent<BoxCollider>();
                    }
                }
            }

            return;
        }
	
	    // Update is called once per frame
	    void Update () {
		    
	    }
    }

    public class ScenarioResponse
    {
        public string message { get; set; }
        public Scenario scenario { get; set; }
    }


    public class Scenario
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string goal { get; set; }
        public DateTime when_created { get; set; }
        public DateTime? when_deleted { get; set; }
        public string linked_preb_scenario { get; set; }
        public string linked_next_scenario { get; set; }
        public List<Object> objects { get; set; }
    }


    public class Object
    {
        public string id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float yaw { get; set; }
        public float pitch { get; set; }
        public float roll { get; set; }
        public float scale_x { get; set; }
        public float scale_y { get; set; }
        public float scale_z { get; set; }
        public bool significant { get; set; }
    }
}

