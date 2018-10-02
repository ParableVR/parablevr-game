using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace parable
{
    public class ObjectPopupName : MonoBehaviour, IFocusable
    {
        GameObject textMeshContainer;
        TextMesh textMesh;

        void IFocusable.OnFocusEnter()
        {
            // create an empty container for text mesh
            textMeshContainer = new GameObject("TextMesh Container");
            textMeshContainer.transform.SetParent(transform); // move it under the obj, makes it easier to browse
            textMeshContainer.transform.Translate(transform.position); // move to initial position at the obj
            textMeshContainer.transform.Translate(0, 0.4f, 0, transform); // move it just above the obj

            // create the text mesh
            textMesh = textMeshContainer.AddComponent<TextMesh>();
            textMesh.text = name;
            textMesh.characterSize = 0.015f;
            textMesh.fontSize = 80;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
        }

        void IFocusable.OnFocusExit()
        {
            Destroy(textMeshContainer);
        }

        void Update()
        {
            if (textMeshContainer != null)
            {
                textMeshContainer.transform.LookAt(Camera.main.transform); // always face the camera
                textMeshContainer.transform.localEulerAngles += new Vector3(0, 180, 0); // text comes out reversed?
            }
        }
    }
}
