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

        public Vector3 offset = new Vector3(0, 0.4f, 0);
        public float charSize = 0.015f;
        public int fontSize = 80;

        void IFocusable.OnFocusEnter()
        {
            // create an empty container for text mesh
            textMeshContainer = new GameObject("TextMesh Container");
            textMeshContainer.transform.SetParent(transform); // move it under the obj, makes it easier to browse
            textMeshContainer.transform.Translate(transform.position); // move to initial position at the obj
            textMeshContainer.transform.Translate(offset, transform); // move it just above the obj

            textMeshContainer.AddComponent<LookAtCamera>();

            // create the text mesh
            textMesh = textMeshContainer.AddComponent<TextMesh>();
            textMesh.text = name;
            textMesh.characterSize = charSize;
            textMesh.fontSize = fontSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
        }

        void IFocusable.OnFocusExit()
        {
            Destroy(textMeshContainer);
        }
    }
}
