using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace parable
{
    public class Door : MonoBehaviour
    {
        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.name == "Thermite")
            {
                // log the reaction event
                gameObject.GetComponent<ReactionEvent>()
                    .Trigger(new List<GameObject>() { col.gameObject });

                Destroy(gameObject);
                Destroy(col.gameObject);
            }
        }
    }
}
