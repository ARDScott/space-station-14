﻿using System.Collections.Generic;
using System.Linq;
using ClientInterfaces.UserInterface;
using GameObject;
using Lidgren.Network;
using SS13.IoC;
using SS13_Shared;
using SS13_Shared.GO;

namespace CGO
{
    public class HumanHandsComponent : Component
    {
        public HumanHandsComponent()
        {
            HandSlots = new Dictionary<Hand, Entity>();
            Family = ComponentFamily.Hands;
        }

        public Dictionary<Hand, Entity> HandSlots { get; private set; }
        public Hand CurrentHand { get; private set; }

        public override void HandleNetworkMessage(IncomingEntityComponentMessage message, NetConnection sender)
        {
            var type = (ComponentMessageType) message.MessageParameters[0];
            int entityUid;
            Hand usedHand;
            Entity item;

            switch (type)
            {
                case (ComponentMessageType.EntityChanged):
                    //This is not sent atm. Commented out serverside for later use.
                    entityUid = (int) message.MessageParameters[1];
                    usedHand = (Hand) message.MessageParameters[2];
                    item = Owner.EntityManager.GetEntity(entityUid);
                    if (HandSlots.Keys.Contains(usedHand))
                        HandSlots[usedHand] = item;
                    else
                        HandSlots.Add(usedHand, item);
                    break;
                case (ComponentMessageType.HandsDroppedItem):
                    //entityUid = (int)message.MessageParameters[1];
                    usedHand = (Hand) message.MessageParameters[2];
                    //item = EntityManager.Singleton.GetEntity(entityUid);
                    //item.SendMessage(this, ComponentMessageType.Dropped, null);
                    HandSlots.Remove(usedHand);
                    break;
                case (ComponentMessageType.HandsPickedUpItem):
                    entityUid = (int) message.MessageParameters[1];
                    usedHand = (Hand) message.MessageParameters[2];
                    item = Owner.EntityManager.GetEntity(entityUid);
                    //item.SendMessage(this, ComponentMessageType.PickedUp, null, usedHand);
                    HandSlots.Add(usedHand, item);
                    break;
                case ComponentMessageType.ActiveHandChanged:
                    SwitchHandTo((Hand) message.MessageParameters[1]);
                    break;
            }

            IoCManager.Resolve<IUserInterfaceManager>().ComponentUpdate(GuiComponentType.HandsUi);
        }

        public void SendSwitchHands(Hand hand)
        {
            Owner.SendComponentNetworkMessage(this, NetDeliveryMethod.ReliableOrdered,
                                              ComponentMessageType.ActiveHandChanged, hand);
        }

        public void SendDropEntity(Entity ent)
        {
            Owner.SendComponentNetworkMessage(this, NetDeliveryMethod.ReliableOrdered,
                                              ComponentMessageType.DropEntityInHand, ent.Uid);
        }

        public bool IsHandEmpty(Hand hand)
        {
            return !HandSlots.ContainsKey(hand);
        }

        public void SendDropFromHand(Hand hand)
        {
            Owner.SendComponentNetworkMessage(this, NetDeliveryMethod.ReliableOrdered,
                                              ComponentMessageType.DropItemInHand, hand);
        }

        private void SwitchHandTo(Hand hand)
        {
            CurrentHand = hand;
        }
    }
}