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
                Destroy(gameObject);
                Destroy(col.gameObject);
            }
        }
    }
}
