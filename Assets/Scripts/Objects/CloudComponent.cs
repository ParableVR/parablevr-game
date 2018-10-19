using HoloToolkit.Unity.InputModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace parable.objects
{
    public class CloudComponent : UnityEngine.MonoBehaviour
    {
        public string cId;
        public bool cSignificant;

        void OnDestroy()
        {
            GameObject cloudSession = GameObject.Find("/SceneContent/CloudSession");
            if (cloudSession != null)
            {
                CloudSessionManager sessionManager = cloudSession.GetComponent<CloudSessionManager>();
                if (sessionManager != null)
                {
                    int q = sessionManager.QueryIsBeingDragged(gameObject);
                    if (q > 0)
                    {
                        // is the object currently in somebodies hand?
                        if (q == 1)
                        {
                            // was in local player's hand, drop it
                            GetComponent<HandDraggable>().StopDragging();
                        }
                        else
                        {
                            // remote (or other) person's hand
                            // delete the inv entry

                            //need to force a drop when it's removed for the local usr
                        }
                    }
                }
            }
        }
    }
}
