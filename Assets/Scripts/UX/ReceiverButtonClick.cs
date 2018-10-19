using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Receivers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiverButtonClick : InteractionReceiver {
    protected override void InputDown(GameObject gameObject, InputEventData eventData)
    {
        Debug.Log(gameObject.name + " : InputDown");

        switch (gameObject.name)
        {
            case "DialogWelcomeConnectButton":
                // welcome button pressed
                GameObject.Find("/SceneContent/SessionStart/DialogWelcome").SetActive(false); // disable this dialog
                GameObject.Find("/SceneContent/SessionStart/DialogSelectScenario").SetActive(true); // show next dialog
                break;
        }
    }
}
