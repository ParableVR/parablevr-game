using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace parable.eventloggers
{
    public class TransformEvent : MonoBehaviour
    {
        public void Trigger(GameObject result, IEnumerable<GameObject> involvedObjects)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            gameObjects.AddRange(involvedObjects); // original ingredient objs

            // use this.gameObject for the medium it was transformed by
            // use result for the transform result

            Debug.Log(gameObjects.Count);
        }
    }
}
