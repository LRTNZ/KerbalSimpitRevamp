﻿using KerbalSimpit.Core;
using KerbalSimpit.Core.Extensions;
using KerbalSimpit.Core.KSP.Extensions;
using KerbalSimpit.Core.Messages;
using KerbalSimpit.Core.Peers;
using KerbalSimpit.Core.Utilities;
using KerbalSimpit.Debugger.Controls;
using KerbalSimpit.Debugger.Utilities;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace KerbalSimpit.Debugger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable,
        ISimpitMessageSubscriber<CustomLog>
    {
        public static Simpit Simpit { get; private set; } = null!;

        private Dictionary<SimpitPeer, PeerInfo> _peers;

        public MainWindow()
        {
            InitializeComponent();

            _peers = new Dictionary<SimpitPeer, PeerInfo>();

            SimpitConfiguration configuration = JsonSerializer.Deserialize<SimpitConfiguration>(File.ReadAllText("simpit.config.json"), new JsonSerializerOptions()
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            }) ?? new SimpitConfiguration();

            MainWindow.Simpit = new Simpit(new BasicSimpitLogger(configuration.LogLevel)).RegisterKerbal().AddIncomingSubscriber<CustomLog>(this);


            CompositionTarget.Rendering += this.Update;
            MainWindow.Simpit.OnPeerAdded += this.HandlePeerAdded;
            MainWindow.Simpit.OnPeerRemoved += this.HandlePeerRemoved;
            this.Closing += (sender, args) => this.Dispose();


            foreach (SimpitConfiguration.SerialConfiguration serial in configuration.Serial)
            {
                MainWindow.Simpit.AddSerialPeer(serial.Name, serial.BaudRate);
            }

            MainWindow.Simpit.Start();

            this.AddSimpleTextSubscriber<Core.KSP.Messages.Vessel.Incoming.Rotation>(rot => $"Pitch: {DebugHelper.Get(rot.Pitch)}, Yaw: {DebugHelper.Get(rot.Yaw)}, Roll: {DebugHelper.Get(rot.Roll)}, Mask: {rot.Mask}");
            this.AddSimpleTextSubscriber<Core.KSP.Messages.Vessel.Incoming.Translation>(tran => $"X: {DebugHelper.Get(tran.X)}, Y: {DebugHelper.Get(tran.Y)}, Z: {DebugHelper.Get(tran.Z)}, Mask: {tran.Mask}");
            this.AddSimpleTextSubscriber<Core.KSP.Messages.Vessel.Incoming.Throttle>(throttle => DebugHelper.Get(throttle.Value));
        }

        private void Update(object? sender, EventArgs e)
        {
            MainWindow.Simpit.Flush();
        }

        public void Dispose()
        {
            CompositionTarget.Rendering -= this.Update;

            MainWindow.Simpit.Stop();

            Application.Current.Shutdown();
        }

        private void AddSimpleTextSubscriber<T>(Func<T?, string> text)
            where T : ISimpitMessageData
        {
            SimpleTextSubscriber<T> subscriber = new SimpleTextSubscriber<T>(text);

            this.IncomingContent.Children.Add(subscriber.Control);
            MainWindow.Simpit.AddIncomingSubscriber(subscriber);
        }

        public void Process(SimpitPeer peer, ISimpitMessage<CustomLog> message)
        {
            MainWindow.Simpit.Logger.LogInformation($"{nameof(CustomLog)} - {message.Data.Flags}: {message.Data.Value}");
        }

        private void HandlePeerAdded(object? sender, SimpitPeer e)
        {
            PeerInfo info = new PeerInfo(e);
            _peers.Add(e, info);

            this.InfoContent.Children.Add(info);
        }

        private void HandlePeerRemoved(object? sender, SimpitPeer e)
        {
            if (_peers.Remove(e, out PeerInfo? info) == false)
            {
                throw new InvalidOperationException();
            }

            this.InfoContent.Children.Remove(info);
            info.Dispose();
        }

        private void ToggleFlightScene_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.Simpit.SetOutgoingData(new Core.KSP.Messages.Environment.SceneChange()
            {
                Type = Core.KSP.Messages.Environment.SceneChange.SceneChangeTypeEnum.Flight
            });
        }

        private void ToggleFlightScene_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.Simpit.SetOutgoingData(new Core.KSP.Messages.Environment.SceneChange()
            {
                Type = Core.KSP.Messages.Environment.SceneChange.SceneChangeTypeEnum.NotFlight
            });
        }

        private void ToggleRatio_Checked(object sender, RoutedEventArgs e)
        {
            DebugHelper.Ratio = true;
        }

        private void ToggleRatio_Unchecked(object sender, RoutedEventArgs e)
        {
            DebugHelper.Ratio = false;
        }
    }
}