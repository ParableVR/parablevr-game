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

                new WWW(
                    "https://parablevr-game-api.azurewebsites.net/api/events/do/create/5bb4259bcc7ed83ee3cfbd89?code=WQEsFmDQHSx42xbo46EJetmg4cSeZevInEYdJm2K4axhWlGl8Y19CQ==",
                    postData, postHeaders);
            }

            timer.Reset();
        }
    }
}
