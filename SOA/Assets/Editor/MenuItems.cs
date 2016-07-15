using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class MenuItems
    {
        [MenuItem("View/Hide\u2215Show Network Connectivity")]
        private static void NewMenuOption()
        {
            SimControl.HideShowNetworkConnectivity();
        }

        [MenuItem("View/Track/Stop Tracking")]
        private static void StopTracking()
        {
            SimControl sim = GameObject.FindObjectOfType<SimControl>();
            sim.omcScript.trackingOn = false;
        }

        [MenuItem("View/Track/Blue Balloon")]
        private static void TrackBalloon()
        {
            SimControl sim = GameObject.FindObjectOfType<SimControl>();
            sim.omcScript.TrackObjectWithID(105);
        }

        [MenuItem("View/Track/Blue Heavy UAV 100")]
        private static void Track100()
        {
            SimControl sim = GameObject.FindObjectOfType<SimControl>();
            sim.omcScript.TrackObjectWithID(100);
        }

        [MenuItem("View/Track/Blue Heavy UAV 101")]
        private static void Track101()
        {
            SimControl sim = GameObject.FindObjectOfType<SimControl>();
            sim.omcScript.TrackObjectWithID(101);
        }

        [MenuItem("View/Track/Blue Small UAV 102")]
        private static void Track102()
        {
            SimControl sim = GameObject.FindObjectOfType<SimControl>();
            sim.omcScript.TrackObjectWithID(102);
        }

        [MenuItem("View/Track/Blue Small UAV 103")]
        private static void Track103()
        {
            SimControl sim = GameObject.FindObjectOfType<SimControl>();
            sim.omcScript.TrackObjectWithID(103);
        }

        [MenuItem("View/Track/Blue Small UAV 104")]
        private static void Track104()
        {
            SimControl sim = GameObject.FindObjectOfType<SimControl>();
            sim.omcScript.TrackObjectWithID(104);
        }
    }
}
