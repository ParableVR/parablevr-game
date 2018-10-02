using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using parable.objects;
using parable.eventloggers;

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
                    CloudScenarioResponse res = JsonConvert.DeserializeObject<CloudScenarioResponse>(ep.text);

                    foreach (CloudObject obj in res.scenario.objects)
                    {
                        GameObject gameObject = (GameObject)Instantiate(
                            Resources.Load(obj.path, typeof(GameObject)),
                            new Vector3(obj.x, obj.y, obj.z),
                            new Quaternion(obj.pitch, obj.yaw, obj.roll, 1),
                            cloudObjParent);

                        gameObject.layer = 8; // layer 8 = cloudobjects layer
                        gameObject.transform.localScale = new Vector3(obj.scale_x, obj.scale_y, obj.scale_z);

                        gameObject.name = obj.name;
                        gameObject.AddComponent<ObjectPopupName>(); // show name on focus

                        // add various props from the cloudobject that aren't already present
                        CloudComponent cloudComponent = gameObject.AddComponent<CloudComponent>();
                        cloudComponent.cId = obj.id;
                        cloudComponent.cSignificant = obj.significant;

                        // components required for picking up the object
                        gameObject.AddComponent<HoloToolkit.Unity.InputModule.HandDraggable>();
                        gameObject.AddComponent<Rigidbody>();
                        gameObject.AddComponent<BoxCollider>();

                        // perception logging
                        gameObject.AddComponent<PerceptionEvent>();
                    }
                }
            }

            return;
        }
    }
}

