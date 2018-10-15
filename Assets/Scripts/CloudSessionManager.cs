using Newtonsoft.Json;
using parable.objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace parable
{
    public class CloudSessionManager : MonoBehaviour
    {
        public string scenarioID;
        public string sessionID__NO_TOUCH;
        public string eventSessionID__NO_TOUCH;
        public string userName__NO_TOUCH;

        private Transform sessionParent;

        private string apiEndpointGameSession = "https://parablevr-game-api.azurewebsites.net/api/sessions/do/create?code=CfDJ8AAAAAAAAAAAAAAAAAAAAADNJEKs1Fi0AOz4An6VL1DBRb7xE2oplVqSskn286sr12-oGQZguS5xo7W86jYnVT_rOSGkg5wQXz-aVaiUPmwm74ahDxq4Ap8wk5jXUXjJc9w91-fa6oeTHog9r7wNEWa9aWDMUqfSm6wGWz56ZqnbwQGQBxqn04omuz5xYm487A";
        private string apiEndpointEventSession = "https://parablevr-game-api.azurewebsites.net/api/events/do/create/session?code=CfDJ8AAAAAAAAAAAAAAAAAAAAAB71DJNaYU9xwx70CEkF5aiOsYxa8tC-WaPmfWoJGEhOI29DxplwmjIJApLmOOrU07wEsXWg4zPryU0rlhxkNDIQ5lxrty3-Aeqf9jmN4I1hGLb8FMh8xllOAk--zrDFmF1zX09X68--JUP1hgEDVbCRYnhOoQxd0rsx0hjB6XnJw";

        public FirebaseApp fbApp = null;
        public DatabaseReference refUsers;
        private List<GameObject> playerObjects = new List<GameObject>();

        private Vector3 lastPos;
        private int updateFrame = 0;

        void Start()
        {
            sessionParent = GameObject.Find("/SceneContent/CloudSession").transform;

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

                        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                            var dependencyStatus = task.Result;
                            if (dependencyStatus == Firebase.DependencyStatus.Available)
                            {
                                FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://parablevr-game.firebaseio.com/");
                                fbApp = FirebaseApp.DefaultInstance;

                                // create the user's avatar here, added before listen to changes
                                userName__NO_TOUCH = Guid.NewGuid().ToString();

                                refUsers = FirebaseDatabase.DefaultInstance.GetReference("users");
                                refUsers.ChildAdded += HandleUserLogin;
                                refUsers.ChildRemoved += HandleUserLogoff;
                                refUsers.ChildChanged += HandleUserUpdate;
                            }
                            else
                            {
                                Debug.LogError(System.String.Format(
                                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                            }
                        });
                    }
                }
            }
        }

        void Update()
        {
            if (fbApp != null)
            {
                if (updateFrame == 10)
                {
                    if (Camera.main.gameObject.transform.position != lastPos)
                    {
                        refUsers.Child(userName__NO_TOUCH).Child("x").SetValueAsync(
                            Camera.main.gameObject.transform.position.x);
                        refUsers.Child(userName__NO_TOUCH).Child("z").SetValueAsync(
                            Camera.main.gameObject.transform.position.z);
                    }

                    updateFrame = 0;
                }

                lastPos = Camera.main.gameObject.transform.position;
                updateFrame += 1;
            }
        }


        void HandleUserLogin(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            // ignore if it is the players's own record
            if (args.Snapshot.Key != userName__NO_TOUCH)
            {
                // gameobject interaction cannot be called asyncronously
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    List<DataSnapshot> values = args.Snapshot.Children.ToList();

                    GameObject user = (GameObject)Instantiate(
                        Resources.Load("Player", typeof(GameObject)),
                        new Vector3(
                            float.Parse(values.Where(x => x.Key == "x").First().Value.ToString()),
                            2.5f,
                            float.Parse(values.Where(x => x.Key == "z").First().Value.ToString())),
                        Quaternion.identity,
                        sessionParent);

                    user.name = args.Snapshot.Key; // user's name
                    user.AddComponent<ObjectPopupName>(); // show name on focus

                    playerObjects.Add(user);
                });
            }
        }

        void HandleUserLogoff(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            if (args.Snapshot.Key != userName__NO_TOUCH)
            {
                // gameobject interaction cannot be called asyncronously
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    GameObject player = playerObjects.Where(x => x.name == args.Snapshot.Key).First();
                    playerObjects.Remove(player);
                    Destroy(player);
                });
            }
        }

        void HandleUserUpdate(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            if (args.Snapshot.Key != userName__NO_TOUCH)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    List<DataSnapshot> values = args.Snapshot.Children.ToList();

                    GameObject player = playerObjects.Where(x => x.name == args.Snapshot.Key).First();
                    player.transform.position = new Vector3(
                        float.Parse(values.Where(x => x.Key == "x").First().Value.ToString()),
                        2.5f,
                        float.Parse(values.Where(x => x.Key == "z").First().Value.ToString()));
                });
            }
        }


        void OnApplicationQuit()
        {
            // cleanup sessions and logout
            refUsers.Child(userName__NO_TOUCH).RemoveValueAsync();
        }
    }
}
