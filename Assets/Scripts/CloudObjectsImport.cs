using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using parable.objects;
using parable.eventloggers;
using HoloToolkit.Unity.InputModule;

namespace parable
{
    public class CloudObjectsImport : MonoBehaviour
    {
        public string scenarioID;
        private string apiEndpoint = "https://parablevr-game-api.azurewebsites.net/api/locations/get/scenario/id/{0}?code=CfDJ8AAAAAAAAAAAAAAAAAAAAAAhY076YteyT3NjroIt-aCpWMezMlrw_f3vZPrOj2xTx8sp8K9fcDB93PdsZXTr4jpSgE0evPindOQcNlVTih63J7q2jAeiN_XJe705PsWgJ3q0uS-vceosq1rlY6y8XeFlJaDu0b4lUqz7rCw4w-jn5tKV4-6_SCR-AyBDN1-fMA";

        public void Import()
        {
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
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            GameObject gameObject = (GameObject)Instantiate(
                            Resources.Load(obj.path, typeof(GameObject)),
                            new Vector3(obj.x, obj.y, obj.z),
                            new Quaternion(obj.pitch, obj.yaw, obj.roll, 1),
                            GameObject.Find("/SceneContent/CloudObjects").transform);

                            gameObject.layer = 8; // layer 8 = cloudobjects layer
                            gameObject.transform.localScale = new Vector3(obj.scale_x, obj.scale_y, obj.scale_z);

                            gameObject.name = obj.name;
                            gameObject.AddComponent<ObjectPopupName>(); // show name on focus

                            // add various props from the cloudobject that aren't already present
                            CloudComponent cloudComponent = gameObject.AddComponent<CloudComponent>();
                            cloudComponent.cId = obj.id;
                            cloudComponent.cSignificant = obj.significant;

                            // components required for picking up the object
                            HandDraggable draggable = gameObject.AddComponent<HandDraggable>();
                            draggable.StartedDragging += () => GameObject.Find("/SceneContent/CloudSession")
                                .GetComponent<CloudSessionManager>()
                                .HandleUserSelfGrab(gameObject);
                            draggable.StoppedDragging += () => GameObject.Find("/SceneContent/CloudSession")
                                .GetComponent<CloudSessionManager>()
                                .HandleUserSelfDrop(gameObject);

                            gameObject.AddComponent<Rigidbody>();
                            gameObject.AddComponent<BoxCollider>();

                            // perception logging
                            gameObject.AddComponent<PerceptionEvent>();
                        });
                    }
                }
            }
        }
    }
}

