using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Network;
using UnityEngine;

namespace Gameplay
{
    public class PlayerNetworkInput : MonoBehaviour, INetworkRunnerCallbacks
    {
        private PlayerControls _controls;
        private NetworkRunner _runner;

        [SerializeField]
        private bool isActive = true;

        private void Awake()
        {
            _controls = new PlayerControls();
            _runner = NetworkManager.Instance.NetworkRunner;

            if (_runner == null)
            {
                Debug.LogError("PlayerNetworkInput requires a NetworkRunner in the parent hierarchy.");
            }
        }

        private void Start()
        {
            // Register this object for input callbacks with the runner
            if (_runner != null)
            {
                _runner.AddCallbacks(this);
            }
        }

        private void OnDestroy()
        {
            // Unregister when destroyed
            if (_runner != null)
            {
                _runner.RemoveCallbacks(this);
            }
        }

        // This method is called by Fusion when it's time to collect input
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            throw new NotImplementedException();
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            // Only provide input if this is the local player and inputs are active
            // if (!isActive || !runner.IsPlayer || !runner.IsLocalPlayer(Object.InputAuthority))
            //     return;

            var data = new NetworkInputData();

            // Collect all input values from the Input System
            data.Movement = _controls.Player.Move.ReadValue<Vector2>();
            data.Look = _controls.Player.Look.ReadValue<Vector2>();
            data.Jump = _controls.Player.Jump.IsPressed();
            data.Sprint = _controls.Player.Sprint.IsPressed();

            // Send to network
            input.Set(data);
        }

        // Enable/disable inputs
        public void SetInputEnabled(bool enabled)
        {
            isActive = enabled;
            if (enabled)
                _controls.Enable();
            else
                _controls.Disable();
        }

        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        #region Required INetworkRunnerCallbacks Implementation

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

        #endregion
    }
}