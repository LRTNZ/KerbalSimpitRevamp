using System;
using System.Linq;
using KSP.IO;
using UnityEngine;

namespace KerbalSimpit.Providers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbalSimpitEchoProvider : MonoBehaviour
    {
        private EventData<byte, object> echoRequestEvent;
        private EventData<byte, object> echoReplyEvent;
        private EventData<byte, object> customLogEvent;
        private EventData<byte, object> sceneChangeEvent;

        public void Start()
        {
            echoRequestEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived1");
            if (echoRequestEvent != null) echoRequestEvent.Add(EchoRequestCallback);
            echoReplyEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived2");
            if (echoReplyEvent != null) echoReplyEvent.Add(EchoReplyCallback);
            customLogEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived" + InboundPackets.CustomLog);
            if (customLogEvent != null) customLogEvent.Add(CustomLogCallback);

            sceneChangeEvent = GameEvents.FindEvent<EventData<byte, object>>("toSerial" + OutboundPackets.SceneChange);

            GameEvents.onFlightReady.Add(FlightReadyHandler);
            GameEvents.onGameSceneSwitchRequested.Add(FlightShutdownHandler);
        }

        public void OnDestroy()
        {
            if (echoRequestEvent != null) echoRequestEvent.Remove(EchoRequestCallback);
            if (echoReplyEvent != null) echoReplyEvent.Remove(EchoReplyCallback);
            if (customLogEvent != null) customLogEvent.Remove(CustomLogCallback);

            GameEvents.onFlightReady.Remove(FlightReadyHandler);
            GameEvents.onGameSceneSwitchRequested.Remove(FlightShutdownHandler);
        }

        public void EchoRequestCallback(byte ID, object Data)
        {
            if (KSPit.Config.Verbose) Debug.Log(String.Format("KerbalSimpit: Echo request on port {0}. Replying.", ID));
            KSPit.SendToSerialPort(ID, CommonPackets.EchoResponse, Data);
        }

        public void EchoReplyCallback(byte ID, object Data)
        {
            Debug.Log(String.Format("KerbalSimpit: Echo reply received on port {0}.", ID));
        }

        public void CustomLogCallback(byte ID, object Data)
        {
            byte[] payload = (byte[])Data;

            byte logStatus = payload[0];
            String message = System.Text.Encoding.UTF8.GetString(payload.Skip(1).ToArray());

            if((logStatus & CustomLogBits.NoHeader) == 0)
            {
                message = "Simpit : " + message;
            }

            if ((logStatus & CustomLogBits.PrintToScreen) != 0)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => ScreenMessages.PostScreenMessage(message));
            }

            if ((logStatus & CustomLogBits.Verbose) == 0 || KSPit.Config.Verbose)
            {
                Debug.Log(message);
            }
        }

        private void FlightReadyHandler()
        {
            if (sceneChangeEvent != null)
            {
                sceneChangeEvent.Fire(OutboundPackets.SceneChange, 0x00);
            }
        }

        private void FlightShutdownHandler(GameEvents.FromToAction
                                           <GameScenes, GameScenes> scenes)
        {
            if (scenes.from == GameScenes.FLIGHT)
            {
                if (sceneChangeEvent != null)
                {
                    sceneChangeEvent.Fire(OutboundPackets.SceneChange, 0x01);
                }
            }
        }
    }
}
