using Newtonsoft.Json;
using parable.objects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace parable.eventloggers
{
    public class TransformEvent : MonoBehaviour
    {
        private string apiEndpoint = "https://parablevr-game-api.azurewebsites.net/api/events/do/create/object/{0}?code=CfDJ8AAAAAAAAAAAAAAAAAAAAABMBUlMTuLoZQ1iOBSnklN6UfGdh600fUexNhNV8umegz0BiCjpvJJAw1ZyfAPY9t8FPbJk3ewvhqDfd_5ITJBB6cUvh6cRfBeITjv88BVqAW5fMTitHu-rNQtusm49CTL0GTF7pgRZDcu7MJqg2C8ekhkE_cTyTHTKMJj7ImmMdw";

        public void Trigger(GameObject result, IEnumerable<GameObject> involvedObjects)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            gameObjects.AddRange(involvedObjects); // original ingredient objs

            List<string> objects_involved = new List<string>();
            objects_involved.Add(result.GetComponent<CloudComponent>().cId); // first item is the result
            objects_involved.AddRange(
                gameObjects.Select(x => x.GetComponent<CloudComponent>().cId));

            // use this.gameObject for the medium it was transformed by
            // use result for the transform result

            Dictionary<string, string> postHeaders = new Dictionary<string, string>();
            postHeaders.Add("Content-Type", "application/json");

            byte[] postData = System.Text.Encoding.UTF8
                .GetBytes(JsonConvert.SerializeObject(new
                {
                    type = "transform",
                    user = "",
                    result = true,
                    object_coordinator = gameObject.GetComponent<CloudComponent>().cId,
                    objects_involved
                }));

            new WWW(string.Format(
                apiEndpoint,
                GameObject.Find("/SceneContent/CloudSession")
                    .GetComponent<CloudSessionManager>().eventSessionID__NO_TOUCH),
                postData, postHeaders);
        }
    }
}
