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
using HoloToolkit.Unity.InputModule;

namespace parable
{
    public class CloudSessionManager : MonoBehaviour
    {
        public string scenarioID;
        public string sessionID__NO_TOUCH;
        public string eventSessionID__NO_TOUCH;
        public string userName__NO_TOUCH;

        private Transform sessionParent;
        private CloudObjectsImport cloudObjectsImport;
        private GameObject loadingDialog;

        private string apiEndpointGameSession = "https://parablevr-game-api.azurewebsites.net/api/sessions/do/create?code=CfDJ8AAAAAAAAAAAAAAAAAAAAADNJEKs1Fi0AOz4An6VL1DBRb7xE2oplVqSskn286sr12-oGQZguS5xo7W86jYnVT_rOSGkg5wQXz-aVaiUPmwm74ahDxq4Ap8wk5jXUXjJc9w91-fa6oeTHog9r7wNEWa9aWDMUqfSm6wGWz56ZqnbwQGQBxqn04omuz5xYm487A";
        private string apiEndpointEventSession = "https://parablevr-game-api.azurewebsites.net/api/events/do/create/session?code=CfDJ8AAAAAAAAAAAAAAAAAAAAAB71DJNaYU9xwx70CEkF5aiOsYxa8tC-WaPmfWoJGEhOI29DxplwmjIJApLmOOrU07wEsXWg4zPryU0rlhxkNDIQ5lxrty3-Aeqf9jmN4I1hGLb8FMh8xllOAk--zrDFmF1zX09X68--JUP1hgEDVbCRYnhOoQxd0rsx0hjB6XnJw";

        public FirebaseApp fbApp = null;
        public DatabaseReference refUsers;
        private List<GameObject> playerObjects = new List<GameObject>();
        private GameObject dragging = null;

        private int updateFrame = 0;

        void Start()
        {
            sessionParent = GameObject.Find("/SceneContent/CloudSession").transform;
            cloudObjectsImport = GameObject.Find("/SceneContent/CloudObjects").GetComponent<CloudObjectsImport>();
            loadingDialog = GameObject.Find("/SceneContent/SessionStart/DialogLoading");
        }

        void Update()
        {
            if (fbApp != null)
            {
                // be a little more sensible with hitting the DB
                if (updateFrame == 4)
                {
                    refUsers.Child(userName__NO_TOUCH).UpdateChildrenAsync(new Dictionary<string, object>()
                    {
                        { "pos_x", Camera.main.gameObject.transform.position.x },
                        { "pos_y", Camera.main.gameObject.transform.position.y },
                        { "pos_z", Camera.main.gameObject.transform.position.z },

                        { "quat_x", Camera.main.gameObject.transform.rotation.x },
                        { "quat_y", Camera.main.gameObject.transform.rotation.y },
                        { "quat_z", Camera.main.gameObject.transform.rotation.z },
                        { "quat_w", Camera.main.gameObject.transform.rotation.w }
                    });

                    if (dragging != null)
                    {
                        refUsers.Child(userName__NO_TOUCH).Child("inventory").UpdateChildrenAsync(new Dictionary<string, object>()
                        {
                            { "pos_x", dragging.transform.position.x },
                            { "pos_y", dragging.transform.position.y },
                            { "pos_z", dragging.transform.position.z },

                            { "quat_x", dragging.transform.rotation.x },
                            { "quat_y", dragging.transform.rotation.y },
                            { "quat_z", dragging.transform.rotation.z },
                            { "quat_w", dragging.transform.rotation.w }
                        });
                    }

                    updateFrame = 0;
                }

                updateFrame += 1;
            }
        }


        public IEnumerator StartSession()
        {
            try
            {
                if (!string.IsNullOrEmpty(scenarioID))
                {
                    // create the game session
                    try
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

                            try
                            {
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

                                    // startup the objects import from the scenario db
                                    cloudObjectsImport.Import();

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
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    loadingDialog.SetActive(false)); // hide loading dialog
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            yield return null;
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
                    GameObject user = (GameObject)Instantiate(
                        Resources.Load("Player", typeof(GameObject)),
                        new Vector3(
                            float.Parse(args.Snapshot.Child("pos_x").Value.ToString()),
                            float.Parse(args.Snapshot.Child("pos_y").Value.ToString()),
                            float.Parse(args.Snapshot.Child("pos_z").Value.ToString())),
                        new Quaternion(
                            float.Parse(args.Snapshot.Child("quat_x").Value.ToString()),
                            float.Parse(args.Snapshot.Child("quat_y").Value.ToString()),
                            float.Parse(args.Snapshot.Child("quat_z").Value.ToString()),
                            float.Parse(args.Snapshot.Child("quat_w").Value.ToString())),
                        sessionParent);

                    user.name = args.Snapshot.Key; // user's name

                    // user grabbed item inventory
                    GameObject inv = new GameObject("Inventory");
                    inv.transform.SetParent(user.transform);
                    inv.transform.Translate(user.transform.position);

                    // player nametag
                    ObjectPopupName popup = user.AddComponent<ObjectPopupName>();
                    popup.offset = new Vector3(0, -0.1f, 0);
                    popup.charSize = 0.02f;
                    popup.fontSize = 152;

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
                    GameObject player = playerObjects.Where(x => x.name == args.Snapshot.Key).First();
                    player.transform.position = new Vector3(
                        float.Parse(args.Snapshot.Child("pos_x").Value.ToString()),
                        float.Parse(args.Snapshot.Child("pos_y").Value.ToString()),
                        float.Parse(args.Snapshot.Child("pos_z").Value.ToString()));

                    player.transform.rotation = new Quaternion(
                        float.Parse(args.Snapshot.Child("quat_x").Value.ToString()),
                        float.Parse(args.Snapshot.Child("quat_y").Value.ToString()),
                        float.Parse(args.Snapshot.Child("quat_z").Value.ToString()),
                        float.Parse(args.Snapshot.Child("quat_w").Value.ToString()));

                    if (args.Snapshot.HasChild("inventory"))
                    {
                        List<CloudComponent> cloudComponents = FindObjectsOfType<CloudComponent>()
                            .Where(x => x.cId == args.Snapshot.Child("inventory").Child("cid").Value.ToString()).ToList();

                        if (cloudComponents.Count == 1)
                        {
                            GameObject grabbed = cloudComponents.First().gameObject;
                            Rigidbody rigidBody = grabbed.GetComponent<Rigidbody>();
                            if (rigidBody != null) Destroy(rigidBody); // remove dragged rigid body to prevent gravity
                            HandDraggable handDraggable = grabbed.GetComponent<HandDraggable>();
                            if (handDraggable != null) Destroy(handDraggable); // remove dragability to prevent snatching

                            CloudPlayerDragging playerDragging = grabbed.GetComponent<CloudPlayerDragging>();
                            if (playerDragging == null) grabbed.AddComponent<CloudPlayerDragging>(); // add empty component to use in querying

                            // update the dragged object's position
                            grabbed.transform.position = new Vector3(
                                float.Parse(args.Snapshot.Child("inventory").Child("pos_x").Value.ToString()),
                                float.Parse(args.Snapshot.Child("inventory").Child("pos_y").Value.ToString()),
                                float.Parse(args.Snapshot.Child("inventory").Child("pos_z").Value.ToString()));

                            grabbed.transform.rotation = new Quaternion(
                                float.Parse(args.Snapshot.Child("inventory").Child("quat_x").Value.ToString()),
                                float.Parse(args.Snapshot.Child("inventory").Child("quat_y").Value.ToString()),
                                float.Parse(args.Snapshot.Child("inventory").Child("quat_z").Value.ToString()),
                                float.Parse(args.Snapshot.Child("inventory").Child("quat_w").Value.ToString()));

                            // listen for delete events, so we can re-apply the rigidbody
                            EventHandler<ChildChangedEventArgs> onInvDelete = null; // set to null first, so it can be used to unsub inside the delegate
                            onInvDelete = delegate (object senderInv, ChildChangedEventArgs argsInv)
                            {
                                if (argsInv.Snapshot.Key == "inventory")
                                {
                                    // inventory was removed (item was dropped)
                                    if (grabbed.GetComponent<Rigidbody>() == null) grabbed.AddComponent<Rigidbody>();
                                    if (grabbed.GetComponent<HandDraggable>() == null) grabbed.AddComponent<HandDraggable>();
                                    if (grabbed.GetComponent<CloudPlayerDragging>() != null) Destroy(grabbed.GetComponent<CloudPlayerDragging>());

                                    refUsers.Child(args.Snapshot.Key).ChildRemoved -= onInvDelete; // remove handler
                                }
                            };

                            refUsers.Child(args.Snapshot.Key).ChildRemoved += onInvDelete;
                        }
                        else
                        {
                            Debug.LogWarning(string.Format("HandleUserRemoteGrab returned {0} results, when it was expecting 1", cloudComponents.Count));
                        }
                    }
                });
            }
        }

        // handling objects remotely and locally
        public void HandleUserSelfGrab(GameObject gameObject)
        {
            dragging = gameObject;
            CloudComponent cloudComponent = gameObject.GetComponent<CloudComponent>();

            refUsers.Child(userName__NO_TOUCH).Child("inventory").UpdateChildrenAsync(new Dictionary<string, object>()
            {
                { "cid", cloudComponent.cId },

                { "pos_x", cloudComponent.gameObject.transform.position.x },
                { "pos_y", cloudComponent.gameObject.transform.position.y },
                { "pos_z", cloudComponent.gameObject.transform.position.z },

                { "quat_x", cloudComponent.gameObject.transform.rotation.x },
                { "quat_y", cloudComponent.gameObject.transform.rotation.y },
                { "quat_z", cloudComponent.gameObject.transform.rotation.z },
                { "quat_w", cloudComponent.gameObject.transform.rotation.w }
            });
        }

        public void HandleUserSelfDrop(GameObject gameObject)
        {
            dragging = null;
            refUsers.Child(userName__NO_TOUCH).Child("inventory").RemoveValueAsync();
        }


        public int QueryIsBeingDragged(GameObject gameObject)
        {
            string cId = gameObject.GetComponent<CloudComponent>().cId;

            // check local first
            if (dragging != null)
            {
                if (dragging.GetComponent<CloudComponent>().cId == cId) return 1; // local
            }


            // check remote
            List<CloudPlayerDragging> playersDragging = FindObjectsOfType<CloudPlayerDragging>()
                .Where(x => x.GetComponent<CloudComponent>().cId == cId).ToList();

            if (playersDragging.Count > 0) return 2; // remote
            return 0; // none
        }


        void OnApplicationQuit()
        {
            if (fbApp != null)
            {
                // cleanup sessions and logout
                refUsers.Child(userName__NO_TOUCH).RemoveValueAsync();
            }
        }
    }
}
