﻿using SS13_Shared;
using SS13_Shared.GO;
using Lidgren.Network;
using SS13_Shared.GO.Server;
using System.Collections.Generic;
using GO = GameObject;

namespace ServerInterfaces.GameObject
{
    public delegate void ShutdownEvent(IEntity e);
    
    public interface IEntity : GO.IEntity
    {
        void Translate(Vector2 toPosition);
        Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
        Direction Direction { get; set; }
        int Uid { get; set; }

        void SendMessage(object sender, ComponentMessageType type, params object[] parameters);
        void SendMessage(object sender, ComponentMessageType type, List<ComponentReplyMessage> replies, params object[] args);
        ComponentReplyMessage SendMessage(object sender, ComponentFamily family, ComponentMessageType type, params object[] args);

        void FireNetworkedSpawn();
        void FireNetworkedJoinSpawn(NetConnection client);
        GO.EntityTemplate Template { get; set; }
        event EntityMoveEvent OnMove; 
        event ShutdownEvent OnShutdown;
        void SendComponentNetworkMessage(IGameObjectComponent component, NetDeliveryMethod method, NetConnection recipient, params object[] messageParams);

        void Initialize(bool loaded = false);
        void Moved(Vector2 oldPosition);
        void HandleNetworkMessage(ServerIncomingEntityMessage message);
        EntityState GetEntityState();
        bool Match(IEntityQuery query);
    }
}
