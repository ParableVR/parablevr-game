﻿using parable.objects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace parable
{
    public class Mixer : MonoBehaviour
    {
        private Vector3 centre;
        void Start()
        {
            // setup the center pos of the sphere to inside the dryer object
            centre = new Vector3(
                transform.position.x + .6f,
                transform.position.y + .75f,
                transform.position.z + .4f);
        }

        void Update()
        {
            List<Collider> cols = Physics.OverlapSphere(
                centre, .6f, 1 << 8).ToList(); // 8 = layer for cloudobjects

            List<GameObject> gameObjects = cols
                .Select(x => x.gameObject)
                .Where(x =>
                    x.gameObject.GetComponent<CloudComponent>().cId == "5bb31f90023eb3359042462e" || // aluminum chips
                    x.gameObject.GetComponent<CloudComponent>().cId == "5bb31f99023eb3359042462f") // iron oxide powder
                .ToList();

            if (gameObjects.Count == 2)
            {
                gameObjects.ForEach(obj =>
                {
                    Destroy(obj);
                });

                GameObject gameObject = (GameObject)Instantiate(
                    Resources.Load("Bottles/Mesh/Bottle 01", typeof(GameObject)),
                    centre,
                    new Quaternion(0, 0, 0, 1),
                    GameObject.Find("/SceneContent/CloudObjects").transform);

                gameObject.layer = 8; // layer 8 = cloudobjects layer
                gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                gameObject.name = "Thermite";
                gameObject.AddComponent<ObjectPopupName>(); // show name on focus

                // add various props from the cloudobject that aren't already present
                CloudComponent cloudComponent = gameObject.AddComponent<CloudComponent>();
                cloudComponent.cId = null; // these values will eventually be from the db
                cloudComponent.cSignificant = true;

                // components required for picking up the object
                gameObject.AddComponent<HoloToolkit.Unity.InputModule.HandDraggable>();
                gameObject.AddComponent<Rigidbody>();
                gameObject.AddComponent<BoxCollider>();
            }
        }
    }
}