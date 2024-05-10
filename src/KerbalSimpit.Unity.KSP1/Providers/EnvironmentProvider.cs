﻿using UnityEngine;
using EnvironmentMessages = KerbalSimpit.Core.KSP.Messages.Environment;

namespace KerbalSimpit.Unity.KSP1.Providers
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class EnvironmentProvider : MonoBehaviour
    {
        public void Start()
        {
            DontDestroyOnLoad(this); // Make this provider persistent

            GameEvents.onFlightReady.Add(this.FlightReadyHandler);
            GameEvents.onGameSceneSwitchRequested.Add(this.FlightShutdownHandler);

            GameEvents.onVesselDocking.Add(this.VesselDockingHandler);
            GameEvents.onVesselsUndocking.Add(this.VesselUndockingHandler);
            GameEvents.onVesselSwitching.Add(this.VesselSwitchingHandler);
        }

        public void OnDestroy()
        {
            GameEvents.onFlightReady.Remove(FlightReadyHandler);
            GameEvents.onGameSceneSwitchRequested.Remove(FlightShutdownHandler);

            GameEvents.onVesselDocking.Remove(this.VesselDockingHandler);
            GameEvents.onVesselsUndocking.Remove(this.VesselUndockingHandler);
            GameEvents.onVesselSwitching.Remove(this.VesselSwitchingHandler);
        }

        private void FlightReadyHandler()
        {
            KerbalSimpitUnityKSP1.Simpit.SetOutgoingData(new EnvironmentMessages.SceneChange()
            {
                Type = EnvironmentMessages.SceneChange.SceneChangeTypeEnum.Flight
            });
        }

        private void FlightShutdownHandler(GameEvents.FromToAction<GameScenes, GameScenes> scenes)
        {
            if (scenes.from != GameScenes.FLIGHT)
            {
                return;
            }

            KerbalSimpitUnityKSP1.Simpit.SetOutgoingData(new EnvironmentMessages.SceneChange()
            {
                Type = EnvironmentMessages.SceneChange.SceneChangeTypeEnum.NotFlight
            });
        }

        private void VesselDockingHandler(uint data0, uint data1)
        {
            KerbalSimpitUnityKSP1.Simpit.SetOutgoingData(new EnvironmentMessages.VesselChange()
            {
                Type = EnvironmentMessages.VesselChange.TypeEnum.Docking
            });
        }

        private void VesselUndockingHandler(Vessel data0, Vessel data1)
        {
            KerbalSimpitUnityKSP1.Simpit.SetOutgoingData(new EnvironmentMessages.VesselChange()
            {
                Type = EnvironmentMessages.VesselChange.TypeEnum.Undocking
            });
        }

        private void VesselSwitchingHandler(Vessel data0, Vessel data1)
        {
            KerbalSimpitUnityKSP1.Simpit.SetOutgoingData(new EnvironmentMessages.VesselChange()
            {
                Type = EnvironmentMessages.VesselChange.TypeEnum.Switching
            });
        }
    }
}
