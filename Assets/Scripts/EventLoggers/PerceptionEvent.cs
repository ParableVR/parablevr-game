using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

namespace parable.eventloggers
{
    public class PerceptionEvent : MonoBehaviour, IFocusable
    {
        Stopwatch timer;
        void Start()
        {
            timer = new Stopwatch();
        }
        
        void Update()
        {

        }

        void IFocusable.OnFocusEnter()
        {
            timer.Start();
        }

        void IFocusable.OnFocusExit()
        {
            timer.Stop();

            // check elapsed, if it's long enough, log it
            UnityEngine.Debug.Log(timer.ElapsedMilliseconds);

            timer.Reset();
        }
    }
}
