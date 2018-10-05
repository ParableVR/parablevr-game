using Newtonsoft.Json;
using parable.objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace parable
{
    public class CloudSessionManager : MonoBehaviour
    {
        public string scenarioID;
        public string sessionID__NO_TOUCH;
        public string eventSessionID__NO_TOUCH;
        private string apiEndpointGameSession = "https://parablevr-game-api.azurewebsites.net/api/sessions/do/create?code=CfDJ8AAAAAAAAAAAAAAAAAAAAADNJEKs1Fi0AOz4An6VL1DBRb7xE2oplVqSskn286sr12-oGQZguS5xo7W86jYnVT_rOSGkg5wQXz-aVaiUPmwm74ahDxq4Ap8wk5jXUXjJc9w91-fa6oeTHog9r7wNEWa9aWDMUqfSm6wGWz56ZqnbwQGQBxqn04omuz5xYm487A";
        private string apiEndpointEventSession = "https://parablevr-game-api.azurewebsites.net/api/events/do/create/session?code=CfDJ8AAAAAAAAAAAAAAAAAAAAAB71DJNaYU9xwx70CEkF5aiOsYxa8tC-WaPmfWoJGEhOI29DxplwmjIJApLmOOrU07wEsXWg4zPryU0rlhxkNDIQ5lxrty3-Aeqf9jmN4I1hGLb8FMh8xllOAk--zrDFmF1zX09X68--JUP1hgEDVbCRYnhOoQxd0rsx0hjB6XnJw";

        void Start()
        {
            if (!string.IsNullOrEmpty(scenarioID))
            {
                // create the game session
                Dictionary<string, string> postHeaders = new Dictionary<string, string>();
                postHeaders.Add("Content-Type", "application/json");

                byte[] postData = System.Text.Encoding.UTF8
                    .GetBytes(JsonConvert.SerializeObject(new
                    {
                        name = "iteration 2 demo",
                        scenario = scenarioID,
                        people = new List<object>()
                        {
                        // hard code the host user for now
                        new { user = "5b957503c62d819750ee7ed0" }
                        }
                    }));

                WWW gameSessionRes = new WWW(apiEndpointGameSession, postData, postHeaders);
                while (!gameSessionRes.isDone) ;

                if (!string.IsNullOrEmpty(gameSessionRes.text))
                {
                    CloudSessionResponse cloudSession = JsonConvert
                        .DeserializeObject<CloudSessionResponse>(gameSessionRes.text);
                    sessionID__NO_TOUCH = cloudSession.session.id;

                    // create event session
                    postData = System.Text.Encoding.UTF8
                        .GetBytes(JsonConvert.SerializeObject(new
                        {
                            session = sessionID__NO_TOUCH
                        }));

                    WWW gameSessionEventRes = new WWW(apiEndpointEventSession, postData, postHeaders);
                    while (!gameSessionEventRes.isDone) ;

                    if (!string.IsNullOrEmpty(gameSessionEventRes.text))
                    {
                        CloudEventSessionResponse cloudEvent = JsonConvert
                            .DeserializeObject<CloudEventSessionResponse>(gameSessionEventRes.text);
                        eventSessionID__NO_TOUCH = cloudEvent.event_session.id;
                    }
                }
            }
        }
    }
}
