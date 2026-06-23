using System;
using System.Collections.Generic;
using Mls.Connections;
using UnityEngine;

namespace Mls.Connections.Samples
{
    /// <summary>
    /// Minimal rowing sample: lists the discovered rowing machines, lets you connect one by tapping it, and
    /// shows that rower's live distance + drive. Drop this on a GameObject in a scene and build for Android —
    /// the host platform prompts for BLE permissions. Every callback arrives on the Unity main thread, so the
    /// UI can be driven straight from the subscriptions.
    /// </summary>
    public sealed class ConnectionsSample : MonoBehaviour
    {
        private ConnectionsModule _connections;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        // Only rowing machines are shown; this sample is rowing-only.
        private DeviceInfo[] _rowers = Array.Empty<DeviceInfo>();

        // DistanceUpdates emits the distance since the last emission (a delta), so accumulate it for the total.
        private double _totalMeters;

        private string _status = "starting…";
        private string _distance = "distance: –";
        private string _drive = "drive: –";

        private void Start()
        {
            try
            {
                // In debug builds (and the Editor) a simulated rowing machine is included, so the sample
                // runs without hardware. Release builds talk only to real BLE devices.
                _connections = ConnectionsModule.Create(includeFakes: Debug.isDebugBuild);
                _status = "scanning for rowing machines…";

                _subscriptions.Add(_connections.Devices(devices =>
                {
                    var rowers = new List<DeviceInfo>();
                    foreach (DeviceInfo device in devices)
                    {
                        if (device.Supports("rowing"))
                        {
                            rowers.Add(device);
                        }
                    }

                    _rowers = rowers.ToArray();
                }));

                _subscriptions.Add(_connections.Rowing.DistanceUpdates(d => { _totalMeters += d.Meters; _distance = $"distance: {_totalMeters:0} m"; }));
                _subscriptions.Add(_connections.Rowing.Drive(drive =>
                {
                    var spm = drive.StrokesPerMinute is double rate ? $"{rate:0.0}" : "–";
                    _drive = $"drive: {drive.PowerWatts:0} W, {spm} spm";
                }));
            }
            catch (Exception e)
            {
                _status = "ERROR: " + e.Message;
                Debug.LogError("[ConnectionsSample] " + e);
            }
        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = 36;
            GUI.skin.button.fontSize = 36;

            // Stay clear of the camera notch / status bar by laying out inside the safe area. Screen.safeArea
            // has a bottom-left origin; IMGUI is top-left, so flip Y.
            Rect safe = Screen.safeArea;
            float top = Screen.height - (safe.y + safe.height);
            GUILayout.BeginArea(new Rect(safe.x + 20, top + 20, safe.width - 40, safe.height - 40));
            GUILayout.Label("[Connections] rowing sample");
            GUILayout.Label(_status);
            GUILayout.Space(20);

            if (_rowers.Length == 0)
            {
                GUILayout.Label("No rowing machines found yet…");
            }
            else
            {
                GUILayout.Label("Rowing machines:");
                foreach (DeviceInfo rower in _rowers)
                {
                    DrawRowerRow(rower);
                }
            }

            GUILayout.Space(20);
            GUILayout.Label(_distance);
            GUILayout.Label(_drive);
            GUILayout.EndArea();
        }

        private void DrawRowerRow(DeviceInfo rower)
        {
            GUILayout.BeginHorizontal();
            ConnectionStatusKind status = rower.ConnectionStatus.Kind;
            GUILayout.Label($"{rower.Name ?? rower.Id}  —  {status}", GUILayout.Width((Screen.width - 80) * 0.62f));

            switch (status)
            {
                case ConnectionStatusKind.Connected:
                    if (GUILayout.Button("Disconnect"))
                    {
                        _connections.Forget(rower.Id);
                    }

                    break;
                case ConnectionStatusKind.Connecting:
                    GUILayout.Label("connecting…");
                    break;
                default: // Disconnected / Failed
                    if (GUILayout.Button("Connect"))
                    {
                        _totalMeters = 0;
                        _distance = "distance: 0 m";
                        _drive = "drive: –";
                        _connections.Connect(rower.Id);
                    }

                    break;
            }

            GUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            foreach (IDisposable subscription in _subscriptions)
            {
                subscription?.Dispose();
            }

            _connections?.Dispose();
        }
    }
}
