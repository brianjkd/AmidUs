using System;
using AmidUs.Ui;
using MLAPI;
using MLAPI.Transports.UNET;
using Steamworks;
using UnityEngine;

namespace AmidUs.Server
{
	public class HostGame : MonoBehaviour 
	{
		private void SetConnectionIdToSelf()
		{
			// supports either UnetTransport or SteamP2PTransport, uses whichever is set in MLAPI's NetworkingManager
			var unetTransport = FindObjectOfType<UnetTransport>();
			if (unetTransport != null)
			{
				unetTransport.ConnectAddress = "localhost";
				return;
			}

			var steamTransport = FindObjectOfType<SteamP2PTransport.SteamP2PTransport>();
			if (steamTransport != null)
			{
				if (SteamManager.Initialized)
				{
					steamTransport.ConnectToSteamID = SteamUser.GetSteamID().m_SteamID;
					return;
				}
			}
		
			Debug.LogError("Failed to reset connection id to self");
		}
	
		private void SetConnectionId(string id)
		{
			// supports either UnetTransport or SteamP2PTransport
			var unetTransport = FindObjectOfType<UnetTransport>();
			if (unetTransport != null)
			{
				unetTransport.ConnectAddress = id;
				return;
			}

			var steamTransport = FindObjectOfType<SteamP2PTransport.SteamP2PTransport>();
			if (steamTransport != null)
			{
				steamTransport.ConnectToSteamID = Convert.ToUInt64(id);
				return;
			}
		
			Debug.LogError("Failed to set connection id");
		}
		
		public void MakeServer()
		{
			SetConnectionIdToSelf();
			UnLockRoom();
			NetworkingManager.Singleton.NetworkConfig.ConnectionApproval = true;

			NetworkingManager.Singleton.OnClientConnectedCallback += ClientConnectedCallback;
			NetworkingManager.Singleton.OnClientDisconnectCallback += ClientDisconnectedCallback;
			NetworkingManager.Singleton.OnServerStarted += ServerStarted;
			NetworkingManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallback;

			NetworkingManager.Singleton.StartHost();
		}
		
		void ShutdownServer()
		{
			NetworkingManager.Singleton.OnClientConnectedCallback -= ClientConnectedCallback;
			NetworkingManager.Singleton.OnClientDisconnectCallback -= ClientDisconnectedCallback;
			NetworkingManager.Singleton.OnServerStarted -= ServerStarted;
			NetworkingManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCallback;

			NetworkingManager.Singleton.StopHost();
		}

		private void ConnectionApprovalCallback(byte[] connectionData, ulong clientId, NetworkingManager.ConnectionApprovedDelegate connectionApprovedDelegate)
		{
			connectionApprovedDelegate( true, null, !_isRoomLocked, null, null );

		}

		private void ClientConnectedCallback(ulong clientId)
		{
			Debug.Log( $"Client connected {clientId}" );
		}
	
		private void ClientDisconnectedCallback(ulong clientId)
		{
			Debug.Log( $"Client disconnected {clientId}" );
		}

		private void ServerStarted()
		{
			Debug.Log( "Server started" );
		}

		public void MakeClient(string hostToConnectId)
		{
			SetConnectionId(hostToConnectId);
		
			NetworkingManager.Singleton.OnClientConnectedCallback += ClientConnected;

			NetworkingManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

			NetworkingManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes( _clientCode.ToString() );
			NetworkingManager.Singleton.StartClient();

			var uiManager = FindObjectOfType<UiManager>();
			uiManager.LobbyUi.SetRoomCode(hostToConnectId);
			uiManager.Show(PanelType.Lobby);
		}

		void ClientConnected( ulong clientId ) 
		{
			Debug.Log( $"I'm connected {clientId}" );
		}
	
		void ClientDisconnected( ulong clientId )
		{
			Debug.Log( $"I'm disconnected {clientId}" );
			NetworkingManager.Singleton.OnClientDisconnectCallback -= ClientDisconnected;
			NetworkingManager.Singleton.OnClientConnectedCallback -= ClientConnected;
		
			var uiManager = FindObjectOfType<UiManager>();
			uiManager.Show(PanelType.MainMenu);
		}

		public void AbandonGame()
		{
			if (NetworkingManager.Singleton.IsHost)
			{
				ShutdownServer();
			}
			else
			{
				NetworkingManager.Singleton.StopClient();
			}
		}

		private void LockRoom()
		{
			// lock room while game is in progress
			_isRoomLocked = true;
		}

		private void UnLockRoom()
		{
			// unlock room while game is not in progress
			_isRoomLocked = false;
		}

		public void StartGame()
		{
			var taskManager = FindObjectOfType<Server.GameManager>();
			taskManager.OnGameStart();
			_gameStarted = true;
			LockRoom();
		}
		
		private string _clientCode = "";
		private bool _isRoomLocked;
		private bool _gameStarted;
	}
}

