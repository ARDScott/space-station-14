﻿using SS13_Shared;
using SS13_Shared.GO;

namespace CGO
{
    public class BasicItemComponent : GameObjectComponent
    {
        public BasicItemComponent()
        {
            family = ComponentFamily.Item;
        }

        public override void HandleNetworkMessage(IncomingEntityComponentMessage message)
        {
            if (message.ComponentFamily != family)
                return;
            switch ((ItemComponentNetMessage)message.MessageParameters[0])
            {
                case ItemComponentNetMessage.PickedUp://I've been picked up -- says the server's item component
                    Owner.AddComponent(ComponentFamily.Mover, ComponentFactory.Singleton.GetComponent("SlaveMoverComponent"));
                    var e = EntityManager.Singleton.GetEntity((int)message.MessageParameters[1]);
                    var h = (Hand)message.MessageParameters[2];
                    Owner.SendMessage(this, ComponentMessageType.PickedUp, null, h); 
                    Owner.SendMessage(this, ComponentMessageType.SlaveAttach, null, e.Uid);
                    break;
                case ItemComponentNetMessage.Dropped: //I've been dropped -- says the server's item component
                    Owner.AddComponent(ComponentFamily.Mover, ComponentFactory.Singleton.GetComponent("NetworkMoverComponent"));
                    Owner.SendMessage(this, ComponentMessageType.Dropped, null);
                    break;
            }
        }
    }
}
