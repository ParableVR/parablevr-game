using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace parable
{
    public class LookAtCamera : MonoBehaviour
    {
        void Update()
        {
            transform.LookAt(Camera.main.transform); // always face the camera
            transform.localEulerAngles += new Vector3(0, 180, 0); // text comes out reversed?
        }
    }
}
