using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using Newtonsoft.Json;
using parable.objects;

namespace parable.eventloggers
{
    public class PerceptionEvent : MonoBehaviour, IFocusable
    {
        Stopwatch timer;
        private string apiEndpoint = "https://parablevr-game-api.azurewebsites.net/api/events/do/create/object/{0}?code=CfDJ8AAAAAAAAAAAAAAAAAAAAABMBUlMTuLoZQ1iOBSnklN6UfGdh600fUexNhNV8umegz0BiCjpvJJAw1ZyfAPY9t8FPbJk3ewvhqDfd_5ITJBB6cUvh6cRfBeITjv88BVqAW5fMTitHu-rNQtusm49CTL0GTF7pgRZDcu7MJqg2C8ekhkE_cTyTHTKMJj7ImmMdw";

        void Start()
        {
            timer = new Stopwatch();
        }

        void Update()
        {

        }

        void IFocusable.OnFocusEnter()
        {
            timer.Start();
        }

        void IFocusable.OnFocusExit()
        {
            timer.Stop();

            // check elapsed, if it's long enough, log it
            if (timer.ElapsedMilliseconds > 1000 * 1) // 1 sec
            {
                Dictionary<string, string> postHeaders = new Dictionary<string, string>();
                postHeaders.Add("Content-Type", "application/json");

                byte[] postData = System.Text.Encoding.UTF8
                    .GetBytes(JsonConvert.SerializeObject(new
                    {
                        type = "perception",
                        user = "",
                        contact_duration = timer.ElapsedMilliseconds,
                        result = false,
                        object_coordinator = "",
                        objects_involved = new string[]
                        {
                            gameObject.GetComponent<CloudComponent>().cId
                        }
                    }));
                
                new WWW(string.Format(
                    apiEndpoint,
                    GameObject.Find("/SceneContent/CloudSession")
                        .GetComponent<CloudSessionManager>().eventSessionID__NO_TOUCH),
                    postData, postHeaders);
            }

            timer.Reset();
        }
    }
}
