﻿using System;
using ClientInterfaces.Network;
using Lidgren.Network;
using SS13_Shared;

namespace ClientServices.Network
{
    public class NetworkManager : INetworkManager
    {
        private const string ServerName = "SS13 Server";
        private readonly NetPeerConfiguration _netConfig = new NetPeerConfiguration("SS13_NetTag");
        private GameType _serverGameType;
        private NetClient _netClient;

        public bool IsConnected { get; private set; }
        
        public NetPeerStatistics CurrentStatistics
        {
            get { return _netClient.Statistics; }
        }

        public event EventHandler<IncomingNetworkMessageArgs> MessageArrived;  //Called when we recieve a new message.
        protected virtual void OnMessageArrived(NetIncomingMessage message)
        {
            if (MessageArrived != null) MessageArrived(this, new IncomingNetworkMessageArgs(message));
        }

        public event EventHandler Connected;     //Called when we connect to a server.
        protected virtual void OnConnected()
        {
            if (Connected != null) Connected(this, null);
        }

        public event EventHandler Disconnected;  //Called when we Disconnect from a server.
        protected virtual void OnDisconnected()
        {
            if (Disconnected != null) Disconnected(this, null);
        }

        public NetworkManager()
        {

            IsConnected = false;

            _netClient = new NetClient(_netConfig);
            _netClient.Start();
        }

        public void ConnectTo(string host)
        {
          _netClient.Connect(host,1212);
        }

        public void Disconnect()
        {
            Restart();
        }

        public void Restart()
        {
            _netClient.Shutdown("Leaving");
            _netClient = new NetClient(_netConfig);
            _netClient.Start();
        }

        public void UpdateNetwork()
        {
            if (IsConnected)
            {
                NetIncomingMessage msg;
                while ((msg = _netClient.ReadMessage()) != null)
                {
                    OnMessageArrived(msg);
                    _netClient.Recycle(msg);
                }
            }

            if (!IsConnected && _netClient.ServerConnection != null)
            {
                OnConnected();
                IsConnected = true;
            }
            else if (IsConnected && _netClient.ServerConnection == null)
            {
                OnDisconnected();
                IsConnected = false;
            }
        }

        public void ShutDown()
        {
            _netClient.Shutdown("Quitting");
        }

        public void SetGameType(NetIncomingMessage msg)
        {
            _serverGameType = (GameType)msg.ReadByte();
        }

        public void RequestMap()
        {
            var message = _netClient.CreateMessage();
            message.Write((byte)NetMessage.SendMap);
            _netClient.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void SendChangeTile(int x, int z, TileType newTile)
        {
            var netMessage = _netClient.CreateMessage();
            netMessage.Write(x);
            netMessage.Write(z);
            netMessage.Write((byte)newTile);
            _netClient.SendMessage(netMessage, NetDeliveryMethod.ReliableOrdered);
        }

        public NetOutgoingMessage CreateMessage()
        {
            return _netClient.CreateMessage();
        }

        public void SendClientName(string name)
        {
            var message = _netClient.CreateMessage();
            message.Write((byte)NetMessage.ClientName);
            message.Write(name);
            _netClient.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendMessage(NetOutgoingMessage message, NetDeliveryMethod deliveryMethod)
        {
            if (message != null)
            {
                _netClient.SendMessage(message, deliveryMethod);
            }
        }

        public NetIncomingMessage GetNetworkUpdate()
        {
            NetIncomingMessage msg;
            return (msg = _netClient.ReadMessage()) != null ? msg : null;
        }

        public string GetServerName()
        {
            return ServerName;
        }

        public string GetServerAddress()
        {
            return String.Format("{0}:{1}", _netClient.ServerConnection.RemoteEndpoint.Address, _netClient.Port);
        }

        public GameType GetGameType()
        {
            return _serverGameType;
        }
    }
}
