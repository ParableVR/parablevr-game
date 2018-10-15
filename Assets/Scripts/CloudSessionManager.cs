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
        private GameObject dragging = null;

        private Vector3 lastPos;
        private Quaternion lastQuat;

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

                        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                        {
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
                                Debug.LogError(string.Format(
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
                // be a little more sensible with hitting the DB
                if (updateFrame == 6)
                {
                    if (Camera.main.gameObject.transform.position != lastPos)
                    {
                        refUsers.Child(userName__NO_TOUCH).Child("pos_x").SetValueAsync(
                            Camera.main.gameObject.transform.position.x);
                        refUsers.Child(userName__NO_TOUCH).Child("pos_y").SetValueAsync(
                            Camera.main.gameObject.transform.position.y);
                        refUsers.Child(userName__NO_TOUCH).Child("pos_z").SetValueAsync(
                            Camera.main.gameObject.transform.position.z);
                    }

                    if (Camera.main.gameObject.transform.rotation != lastQuat)
                    {
                        refUsers.Child(userName__NO_TOUCH).Child("quat_pitch").SetValueAsync(
                            Camera.main.gameObject.transform.rotation.x);
                        refUsers.Child(userName__NO_TOUCH).Child("quat_yaw").SetValueAsync(
                            Camera.main.gameObject.transform.rotation.y);
                        refUsers.Child(userName__NO_TOUCH).Child("quat_roll").SetValueAsync(
                            Camera.main.gameObject.transform.rotation.z);
                    }

                    lastPos = Camera.main.gameObject.transform.position;
                    lastQuat = Camera.main.gameObject.transform.rotation;
                    updateFrame = 0;
                }

                updateFrame += 1;
            }
        }


        void HandleUserLogin(object sender, ChildChangedEventArgs args)
        {
            // user joined
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
                            float.Parse(values.Where(x => x.Key == "pos_x").First().Value.ToString()),
                            float.Parse(values.Where(x => x.Key == "pos_y").First().Value.ToString()),
                            float.Parse(values.Where(x => x.Key == "pos_z").First().Value.ToString())),
                        new Quaternion(
                            float.Parse(values.Where(x => x.Key == "quat_pitch").First().Value.ToString()),
                            float.Parse(values.Where(x => x.Key == "quat_yaw").First().Value.ToString()),
                            float.Parse(values.Where(x => x.Key == "quat_roll").First().Value.ToString()), 1),
                        sessionParent);

                    user.name = args.Snapshot.Key; // user's name

                    // user grabbed item inventory
                    GameObject inv = new GameObject("Inventory");
                    inv.transform.SetParent(user.transform);
                    inv.transform.Translate(user.transform.position);
                    
                    // player tags
                    GameObject textMeshContainer = new GameObject("TextMesh Container");
                    textMeshContainer.transform.SetParent(user.transform); // move it under the obj, makes it easier to browse
                    textMeshContainer.transform.Translate(user.transform.position); // move to initial position at the obj
                    textMeshContainer.transform.Translate(0, -0.1f, 0, user.transform); // move it just below

                    textMeshContainer.AddComponent<LookAtCamera>();

                    // create the actual text
                    TextMesh textMesh = textMeshContainer.AddComponent<TextMesh>();
                    textMesh.text = user.name;
                    textMesh.characterSize = 0.02f;
                    textMesh.fontSize = 152;
                    textMesh.anchor = TextAnchor.MiddleCenter;
                    textMesh.alignment = TextAlignment.Center;
                    
                    playerObjects.Add(user);
                });
            }
        }

        void HandleUserLogoff(object sender, ChildChangedEventArgs args)
        {
            // user left
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
            // remote user performed some activity, like moving or grabbing
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            if (args.Snapshot.Key != userName__NO_TOUCH)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    List<DataSnapshot> values = args.Snapshot.Children.ToList();

                    GameObject player = playerObjects.Where(x => x.name == args.Snapshot.Key).First();
                    player.transform.position = new Vector3(
                        float.Parse(values.Where(x => x.Key == "pos_x").First().Value.ToString()),
                        float.Parse(values.Where(x => x.Key == "pos_y").First().Value.ToString()),
                        float.Parse(values.Where(x => x.Key == "pos_z").First().Value.ToString()));

                    player.transform.rotation = new Quaternion(
                        float.Parse(values.Where(x => x.Key == "quat_pitch").First().Value.ToString()),
                        float.Parse(values.Where(x => x.Key == "quat_yaw").First().Value.ToString()),
                        float.Parse(values.Where(x => x.Key == "quat_roll").First().Value.ToString()), 1);
                });
            }
        }

        // handling objects remotely and locally
        public void HandleUserSelfGrab(GameObject gameObject)
        {
            dragging = gameObject;
            CloudComponent cloudComponent = gameObject.GetComponent<CloudComponent>();
            refUsers.Child(userName__NO_TOUCH).Child("item").SetValueAsync(cloudComponent.cId);
        }

        public void HandleUserSelfDrop(GameObject gameObject)
        {
            dragging = null;
            refUsers.Child(userName__NO_TOUCH).Child("item").RemoveValueAsync();
        }

        public void HandleUserRemoteGrab(string username, string cId)
        {
            List<CloudComponent> cloudComponents = FindObjectsOfType<CloudComponent>()
                .Where(x => x.cId == cId).ToList();

            if (cloudComponents.Count == 1)
            {
                GameObject grabbed = cloudComponents.First().gameObject;
                Destroy(grabbed.GetComponent<Rigidbody>()); // remove dragged rigid body to prevent gravity

                Transform playerInv = GameObject.Find(string.Format("/SceneContent/CloudSession/{0}/Inventory", username)).transform;
                grabbed.transform.SetParent(playerInv);
            }
            else
            {
                Debug.LogWarning(string.Format("HandleUserRemoteGrab returned {0} results, when it was expecting 1", cloudComponents.Count));
            }
        }

        public void HandleUserRemoteDrop(string username, string cId)
        {
            List<CloudComponent> cloudComponents = FindObjectsOfType<CloudComponent>()
                .Where(x => x.cId == cId).ToList();

            if (cloudComponents.Count == 1)
            {
                GameObject grabbed = cloudComponents.First().gameObject;
                grabbed.AddComponent<Rigidbody>(); // restore rigidbody for normal functionality

                Transform cloudObjects = GameObject.Find("/SceneContent/CloudObjects").transform;
                grabbed.transform.SetParent(cloudObjects);
            }
            else
            {
                Debug.LogWarning(string.Format("HandleUserRemoteGrab returned {0} results, when it was expecting 1", cloudComponents.Count));
            }
        }


        void OnApplicationQuit()
        {
            // cleanup sessions and logout
            refUsers.Child(userName__NO_TOUCH).RemoveValueAsync();
        }
    }
}
