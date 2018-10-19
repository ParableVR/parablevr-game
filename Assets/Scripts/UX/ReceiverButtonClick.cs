using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Receivers;
using parable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiverButtonClick : InteractionReceiver {
    protected override void InputDown(GameObject gameObject, InputEventData eventData)
    {
        switch (gameObject.name)
        {
            case "DialogWelcomeConnectButton":
                // welcome button pressed
                GameObject.Find("/SceneContent/SessionStart/DialogWelcome").SetActive(false); // disable this dialog
                GameObject.Find("/SceneContent/SessionStart/DialogSelectScenario").SetActive(true); // show next dialog
                break;
            case "DialogSelectScenarioButtonChemLab":
                // select the chem lab scenario
                GameObject.Find("/SceneContent/SessionStart/DialogSelectScenario").SetActive(false); // disable this dialog
                GameObject.Find("/SceneContent/SessionStart/DialogLoading").SetActive(true); // show the loading dialog

                GameObject.Find("/SceneContent/CloudSession")
                    .GetComponent<CloudSessionManager>().StartSession(); // boot the session

                break;
        }
    }
}
