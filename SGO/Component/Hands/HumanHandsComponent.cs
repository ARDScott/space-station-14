﻿using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using SS13_Shared;
using SS13_Shared.GO;

namespace SGO
{
    public class HumanHandsComponent : GameObjectComponent
    {
        private readonly Dictionary<Hand, Entity> handslots;
        private Hand currentHand = Hand.Left;

        public HumanHandsComponent()
        {
            family = ComponentFamily.Hands;
            handslots = new Dictionary<Hand, Entity>();
        }

        /// <summary>
        /// Recieve a component message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="type"></param>
        /// <param name="replies"></param>
        /// <param name="list"></param>
        public override ComponentReplyMessage RecieveMessage(object sender, ComponentMessageType type,
                                                             params object[] list)
        {
            ComponentReplyMessage reply = base.RecieveMessage(sender, type, list);

            if (sender == this)
                return ComponentReplyMessage.Empty;

            switch (type)
            {
                case ComponentMessageType.DisassociateEntity:
                    var entDrop = (Entity) list[0];
                    Drop(entDrop);
                    break;
                case ComponentMessageType.ActiveHandChanged:
                    SwitchHandsTo((Hand) list[0]);
                    break;
                case ComponentMessageType.IsCurrentHandEmpty:
                    reply = new ComponentReplyMessage(ComponentMessageType.IsCurrentHandEmpty, IsEmpty(currentHand));
                    break;
                case ComponentMessageType.IsHandEmpty:
                    reply = new ComponentReplyMessage(ComponentMessageType.IsHandEmptyReply, IsEmpty((Hand) list[0]));
                    break;
                case ComponentMessageType.PickUpItem:
                    Pickup((Entity) list[0]);
                    break;
                case ComponentMessageType.PickUpItemToHand:
                    Pickup((Entity) list[0], (Hand) list[1]);
                    break;
                case ComponentMessageType.DropItemInCurrentHand:
                    Drop(currentHand);
                    break;
                case ComponentMessageType.DropItemInHand:
                    var hand = (Hand) list[0];
                    Drop(hand);
                    break;
                case ComponentMessageType.DropEntityInHand:
                    var ent = (Entity) list[0];
                    Drop(ent);
                    break;
                case ComponentMessageType.BoundKeyChange:
                    if ((BoundKeyFunctions) list[0] == BoundKeyFunctions.Drop &&
                        (BoundKeyState) list[1] == BoundKeyState.Up)
                        Drop();
                    if ((BoundKeyFunctions) list[0] == BoundKeyFunctions.SwitchHands &&
                        (BoundKeyState) list[1] == BoundKeyState.Up)
                    {
                        SwitchHands();
                    }
                    if ((BoundKeyFunctions)list[0] == BoundKeyFunctions.ActivateItemInHand &&
                        (BoundKeyState)list[1] == BoundKeyState.Up)
                        ActivateItemInHand();
                        
                    break;
                case ComponentMessageType.GetActiveHandItem:
                    if (!IsEmpty(currentHand))
                        reply = new ComponentReplyMessage(ComponentMessageType.ReturnActiveHandItem,
                                                          handslots[currentHand]);
                    break;
                case ComponentMessageType.Die:
                    DropAll();
                    break;
            }

            return reply;
        }

        public override void HandleNetworkMessage(IncomingEntityComponentMessage message, NetConnection client)
        {
            if (message.ComponentFamily == ComponentFamily.Hands)
            {
                var type = (ComponentMessageType) message.MessageParameters[0];
                var replies = new List<ComponentReplyMessage>();
                switch (type)
                {
                    case ComponentMessageType.ActiveHandChanged:
                        SwitchHandsTo((Hand)message.MessageParameters[1]);
                        break;
                    case ComponentMessageType.DropEntityInHand:
                        Drop((Entity)EntityManager.Singleton.GetEntity((int) message.MessageParameters[1]));
                        break;
                    case ComponentMessageType.DropItemInHand:
                        Drop((Hand)message.MessageParameters[1]);
                        break;
                }
            }
        }


        /// <summary>
        /// Change the currently selected hand
        /// </summary>
        private void SwitchHands()
        {
            if (currentHand == Hand.Left)
                SwitchHandsTo(Hand.Right);
            else
                SwitchHandsTo(Hand.Left);
        }

        private void SwitchHandsTo(Hand hand)
        {
            currentHand = hand;
            Owner.SendComponentNetworkMessage(this, NetDeliveryMethod.ReliableOrdered, null,
                                              ComponentMessageType.ActiveHandChanged, hand);
        }

        /// <summary>
        /// Get the entity in the specified hand
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        private Entity GetEntity(Hand hand)
        {
            if (!IsEmpty(hand))
                return handslots[hand];
            else
                return null;
        }

        /// <summary>
        /// Get the currently selected hand
        /// </summary>
        /// <returns></returns>
        private Hand GetCurrentHand()
        {
            return currentHand;
        }

        private void ActivateItemInHand()
        {
            var h = GetCurrentHand();
            if (!IsEmpty(h))
            {
                var e = GetEntity(h);
                if (e != null)
                {
                    e.SendMessage(this, ComponentFamily.Item, ComponentMessageType.Activate);
                }
            }

        }

        /// <summary>
        /// Set the entity in the specified hand
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="entity"></param>
        private void SetEntity(Hand hand, Entity entity)
        {
            if (entity != null && IsEmpty(hand))
            {
                handslots.Add(hand, entity);
                //Owner.SendComponentNetworkMessage(this, Lidgren.Network.NetDeliveryMethod.ReliableOrdered, null, ComponentMessageType.EntityChanged, entity.Uid, hand); Maybe for later use?
            }
        }

        /// <summary>
        /// Put the specified entity in the currently selected hand
        /// </summary>
        /// <param name="entity"></param>
        private void Pickup(Entity entity)
        {
            if (entity != null && IsEmpty(currentHand))
            {
                RemoveFromOtherComps(entity);

                SetEntity(currentHand, entity);
                Owner.SendComponentNetworkMessage(this, NetDeliveryMethod.ReliableOrdered, null,
                                                  ComponentMessageType.HandsPickedUpItem, entity.Uid, currentHand);
                entity.SendMessage(this, ComponentMessageType.PickedUp, Owner, currentHand);
            }
        }

        /// <summary>
        /// Put the specified entity in the specified hand
        /// </summary>
        /// <param name="entity"></param>
        private void Pickup(Entity entity, Hand hand)
        {
            if (entity != null && IsEmpty(hand))
            {
                RemoveFromOtherComps(entity);

                SetEntity(hand, entity);
                Owner.SendComponentNetworkMessage(this, NetDeliveryMethod.ReliableOrdered, null,
                                                  ComponentMessageType.HandsPickedUpItem, entity.Uid, hand);
                entity.SendMessage(this, ComponentMessageType.PickedUp, Owner, hand);
            }
        }

        /// <summary>
        /// Drop the item in the currently selected hand
        /// </summary>
        private void Drop()
        {
            Drop(currentHand);
        }

        private void RemoveFromOtherComps(Entity entity)
        {
            Entity holder = null;
            if (entity.HasComponent(ComponentFamily.Item))
                holder = ((BasicItemComponent) entity.GetComponent(ComponentFamily.Item)).currentHolder;
            if (holder == null && entity.HasComponent(ComponentFamily.Equippable))
                holder = ((EquippableComponent) entity.GetComponent(ComponentFamily.Equippable)).currentWearer;
            if (holder != null) holder.SendMessage(this, ComponentMessageType.DisassociateEntity, entity);
            else Owner.SendMessage(this, ComponentMessageType.DisassociateEntity, entity);
        }

        /// <summary>
        /// Drop an item from a hand.
        /// </summary>
        /// <param name="hand"></param>
        private void Drop(Hand hand)
        {
            if (!IsEmpty(hand))
            {
                GetEntity(hand).SendMessage(this, ComponentMessageType.Dropped);
                Owner.SendComponentNetworkMessage(this, NetDeliveryMethod.ReliableOrdered, null,
                                                  ComponentMessageType.HandsDroppedItem, GetEntity(hand).Uid, hand);
                handslots.Remove(hand);
            }
        }

        /// <summary>
        /// Drop an entity.
        /// </summary>
        /// <param name="hand"></param>
        private void Drop(Entity ent)
        {
            if (handslots.ContainsValue(ent))
            {
                Hand holding = handslots.First(x => x.Value == ent).Key;
                Drop(holding);
            }
        }


        private void DropAll()
        {
            Drop(Hand.Left);
            Drop(Hand.Right);
        }

        /// <summary>
        /// Check if the specified hand is empty
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        private bool IsEmpty(Hand hand)
        {
            if (handslots.ContainsKey(hand))
                return false;
            return true;
        }
    }
}