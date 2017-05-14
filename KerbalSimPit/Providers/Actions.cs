using System;
using KSP.IO;
using KSP.UI.Screens;
using UnityEngine;

namespace KerbalSimPit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimPitActionProvider : MonoBehaviour
    {
        private EventData<byte, object> AGActivateChannel, AGDeactivateChannel;

        // TODO: Only using a single byte buffer for each of these is
        // technically unsafe. It's not impossible that multiple controllers
        // will attempt to send new packets between each Update(), and only
        // the last one will be affected. But it is unlikely, which is why
        // I'm not addressing it now.
        private volatile byte activateBuffer, deactivateBuffer,
            toggleBuffer, currentStateBuffer;

        public void Start()
        {
            activateBuffer = 0;
            deactivateBuffer = 0;
            toggleBuffer = 0;
            currentStateBuffer = 0;

            AGActivateChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived9");
            if (AGActivateChannel != null) AGActivateChannel.Add(actionActivateCallback);
            AGDeactivateChannel = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived10");
            if (AGDeactivateChannel != null) AGDeactivateChannel.Add(actionDeactivateCallback);

            updateCurrentState();
        }

        public void OnDestroy()
        {
            //if (stageChannel != null) stageChannel.Remove(stageCallback);
            if (AGActivateChannel != null) AGActivateChannel.Remove(actionActivateCallback);
            if (AGDeactivateChannel != null) AGDeactivateChannel.Remove(actionDeactivateCallback);
        }

        public void Update()
        {
            Vessel av = FlightGlobals.ActiveVessel;
            if (activateBuffer > 0)
            {
                activateGroups(activateBuffer);
                activateBuffer = 0;
            }
            if (deactivateBuffer > 0)
            {
                deactivateGroups(deactivateBuffer);
                deactivateBuffer = 0;
            }
            if (toggleBuffer > 0)
            {
                toggleGroups(toggleBuffer);
                toggleBuffer = 0;
            }
        }

        public void actionActivateCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
            activateBuffer = payload[0];
        }

        public void actionDeactivateCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
            deactivateBuffer = payload[0];
        }

        public void actionToggleCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;
            toggleBuffer = payload[0];
        }

        private bool updateCurrentState()
        {
            byte newState = getGroups();
            if (newState != currentStateBuffer)
            {
                // Send state
                return true;
            } else {
                return false;
            }
        }

        private void activateGroups(byte groups)
        {
            if ((groups & ActionGroupBits.StageBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating stage");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Stage, true);
                StageManager.ActivateNextStage();
            }
            if ((groups & ActionGroupBits.GearBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating gear");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Gear, true);
            }
            if ((groups & ActionGroupBits.LightBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating light");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Light, true);
            }
            if ((groups & ActionGroupBits.RCSBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating RCS");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.RCS, true);
            }
            if ((groups & ActionGroupBits.SASBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating SAS");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
            }
            if ((groups & ActionGroupBits.BrakesBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating brakes");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, true);
            }
            if ((groups & ActionGroupBits.AbortBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Activating abort");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Abort, true);
            }
        }

        private void deactivateGroups(byte groups)
        {
            if ((groups & ActionGroupBits.StageBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating stage");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Stage, false);
            }
            if ((groups & ActionGroupBits.GearBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating gear");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Gear, false);
            }
            if ((groups & ActionGroupBits.LightBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating light");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Light, false);
            }
            if ((groups & ActionGroupBits.RCSBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating RCS");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.RCS, false);
            }
            if ((groups & ActionGroupBits.SASBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating SAS");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
            }
            if ((groups & ActionGroupBits.BrakesBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating brakes");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, false);
            }
            if ((groups & ActionGroupBits.AbortBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Deactivating abort");
                FlightGlobals.ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Abort, false);
            }
        }

        private void toggleGroups(byte groups)
        {
            if ((groups & ActionGroupBits.StageBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling stage");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Stage);
                StageManager.ActivateNextStage();
            }
            if ((groups & ActionGroupBits.GearBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling gear");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Gear);
            }
            if ((groups & ActionGroupBits.LightBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling light");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Light);
            }
            if ((groups & ActionGroupBits.RCSBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling RCS");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.RCS);
            }
            if ((groups & ActionGroupBits.SASBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling SAS");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.SAS);
            }
            if ((groups & ActionGroupBits.BrakesBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling brakes");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Brakes);
            }
            if ((groups & ActionGroupBits.AbortBit) != 0)
            {
                if (KSPit.Config.Verbose) Debug.Log("KerbalSimPit: Toggling abort");
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Abort);
            }
        }

        private byte getGroups()
        {
            byte groups = 0;
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Stage])
            {
                groups |= (byte)(1 << ActionGroupBits.StageBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Gear])
            {
                groups |= (byte)(1 << ActionGroupBits.GearBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Light])
            {
                groups |= (byte)(1 << ActionGroupBits.LightBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.RCS])
            {
                groups |= (byte)(1 << ActionGroupBits.RCSBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.SAS])
            {
                groups |= (byte)(1 << ActionGroupBits.SASBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Brakes])
            {
                groups |= (byte)(1 << ActionGroupBits.BrakesBit);
            }
            if (FlightGlobals.ActiveVessel.ActionGroups[KSPActionGroup.Abort])
            {
                groups |= (byte)(1 << ActionGroupBits.AbortBit);
            }
            return groups;
        }
    }
}