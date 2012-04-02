﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SS13_Shared;
using SS13_Shared.GO;
using Lidgren.Network;

namespace SGO
{
    public class EquipmentComponent : GameObjectComponent
    {
        public Dictionary<EquipmentSlot, Entity> equippedEntities = new Dictionary<EquipmentSlot,Entity>();
        protected List<EquipmentSlot> activeSlots = new List<EquipmentSlot>();

        public EquipmentComponent()
        {
            family = ComponentFamily.Equipment;
        }

        public override ComponentReplyMessage RecieveMessage(object sender, SS13_Shared.GO.ComponentMessageType type, params object[] list)
        {
            var reply = base.RecieveMessage(sender, type, list);

            if (sender == this)
                return ComponentReplyMessage.Empty;

            switch (type)
            {
                case ComponentMessageType.DisassociateEntity:
                    UnEquipEntity((Entity)list[0]);
                    break;
                case ComponentMessageType.EquipItemToPart: //Equip an entity straight up.
                    EquipEntityToPart((EquipmentSlot)list[0], (Entity)list[1]);
                    break;
                case ComponentMessageType.EquipItem:
                    EquipEntity((Entity)list[0]);
                    break;
                case ComponentMessageType.EquipItemInHand: //Move an entity from a hand to an equipment slot
                    EquipEntityInHand();
                    break;
                case ComponentMessageType.UnEquipItemToFloor: //remove an entity from a slot and drop it on the floor
                    UnEquipEntity((Entity)list[0]);
                    break;
                case ComponentMessageType.UnEquipItemToHand: //remove an entity from a slot and put it in the current hand slot.
                    if (!Owner.HasComponent(SS13_Shared.GO.ComponentFamily.Hands))
                        break; //TODO REAL ERROR MESSAGE OR SOME FUCK SHIT
                    UnEquipEntityToHand((Entity)list[0]);
                    break;
                case ComponentMessageType.UnEquipItemToSpecifiedHand: //remove an entity from a slot and put it in the current hand slot.
                    if (!Owner.HasComponent(SS13_Shared.GO.ComponentFamily.Hands))
                        break; //TODO REAL ERROR MESSAGE OR SOME FUCK SHIT
                    UnEquipEntityToHand((Entity)list[0], (Hand)list[1]);
                    break;
            }

            return reply;
        }

        public override void HandleNetworkMessage(IncomingEntityComponentMessage message, NetConnection client)
        {
            if (message.componentFamily == ComponentFamily.Equipment)
            {
                ComponentMessageType type = (ComponentMessageType)message.messageParameters[0];
                List<ComponentReplyMessage> replies = new List<ComponentReplyMessage>();
                switch(type) //Why does this send messages to itself THIS IS DUMB AND WILL BREAK THINGS. BZZZ
                {
                    case ComponentMessageType.EquipItem:
                        EquipEntity(EntityManager.Singleton.GetEntity((int)message.messageParameters[1]));
                        break;
                    case ComponentMessageType.EquipItemInHand:
                        EquipEntityInHand();
                        break;
                    case ComponentMessageType.EquipItemToPart:
                        EquipEntityToPart((EquipmentSlot)message.messageParameters[1], EntityManager.Singleton.GetEntity((int)message.messageParameters[2]));
                        break;
                    case ComponentMessageType.UnEquipItemToFloor:
                        UnEquipEntity(EntityManager.Singleton.GetEntity((int)message.messageParameters[1]));
                        break;
                    case ComponentMessageType.UnEquipItemToHand:
                        if (!Owner.HasComponent(SS13_Shared.GO.ComponentFamily.Hands))
                            return; //TODO REAL ERROR MESSAGE OR SOME FUCK SHIT
                        UnEquipEntityToHand(EntityManager.Singleton.GetEntity((int)message.messageParameters[1]));
                        break;
                    case ComponentMessageType.UnEquipItemToSpecifiedHand:
                        if (!Owner.HasComponent(SS13_Shared.GO.ComponentFamily.Hands))
                            return; //TODO REAL ERROR MESSAGE OR SOME FUCK SHIT
                        UnEquipEntityToHand(EntityManager.Singleton.GetEntity((int)message.messageParameters[1]), (Hand)message.messageParameters[2]);
                        break;
                }
            }
        }

        public override void HandleInstantiationMessage(Lidgren.Network.NetConnection netConnection)
        {
            foreach (EquipmentSlot p in equippedEntities.Keys)
            {
                if(!IsEmpty(p))
                {
                    Entity e = equippedEntities[p];
                    e.SendMessage(this, SS13_Shared.GO.ComponentMessageType.ItemEquipped, Owner);
                    Owner.SendComponentNetworkMessage(this, Lidgren.Network.NetDeliveryMethod.ReliableUnordered, netConnection, EquipmentComponentNetMessage.ItemEquipped, p, e.Uid);
                }
            }
        }

        // Equips Entity e to Part part
        private void EquipEntityToPart(EquipmentSlot part, Entity e)
        {
            if (equippedEntities.ContainsValue(e)) //Its already equipped? Unequip first. This shouldnt happen.
                UnEquipEntity(e);

            if (CanEquip(e)) //If the part is empty, the part exists on this mob, and the entity specified is not null
            {
                RemoveFromOtherComps(e);

                equippedEntities.Add(part, e);
                e.SendMessage(this, SS13_Shared.GO.ComponentMessageType.ItemEquipped, Owner);
                Owner.SendComponentNetworkMessage(this, Lidgren.Network.NetDeliveryMethod.ReliableUnordered, null, EquipmentComponentNetMessage.ItemEquipped, part, e.Uid);
            }
        }

        // Equips Entity e and automatically finds the appropriate part
        private void EquipEntity(Entity e)
        {
            if(equippedEntities.ContainsValue(e)) //Its already equipped? Unequip first. This shouldnt happen.
                UnEquipEntity(e);

            if (CanEquip(e))
            {
                var reply = e.SendMessage(this, ComponentFamily.Equippable, ComponentMessageType.GetWearLoc);
                if(reply.MessageType == ComponentMessageType.ReturnWearLoc)
                {
                    RemoveFromOtherComps(e);
                    EquipEntityToPart((EquipmentSlot)reply.ParamsList[0], e);
                }
            }
        }

        // Equips whatever we currently have in our active hand
        private void EquipEntityInHand()
        {
            if (!Owner.HasComponent(SS13_Shared.GO.ComponentFamily.Hands))
            {
                return; //TODO REAL ERROR MESSAGE OR SOME FUCK SHIT
            }
            //Get the item in the hand
            var reply = Owner.SendMessage(this, ComponentFamily.Hands, ComponentMessageType.GetActiveHandItem);
            if (reply.MessageType == ComponentMessageType.ReturnActiveHandItem && CanEquip((Entity)reply.ParamsList[0]))
            {
                RemoveFromOtherComps((Entity)reply.ParamsList[0]);
                //Equip
                EquipEntity((Entity)reply.ParamsList[0]);
            }
        }

        // Unequips the entity from Part part
        private void UnEquipEntity(EquipmentSlot part)
        {
            if (!IsEmpty(part)) //If the part is not empty
            {
                equippedEntities[part].SendMessage(this, SS13_Shared.GO.ComponentMessageType.ItemUnEquipped);
                Owner.SendComponentNetworkMessage(this, Lidgren.Network.NetDeliveryMethod.ReliableUnordered, null, EquipmentComponentNetMessage.ItemUnEquipped, part, equippedEntities[part].Uid);
                equippedEntities.Remove(part);
            }
        }

        private void UnEquipEntityToHand(Entity e)
        {
            UnEquipEntity(e);
            //HumanHandsComponent hh = (HumanHandsComponent)Owner.GetComponent(ComponentFamily.Hands);
            Owner.SendMessage(this, ComponentMessageType.PickUpItem, e);
        }

        private void UnEquipEntityToHand(Entity e, Hand h)
        {
            var hands = (HumanHandsComponent)Owner.GetComponent(ComponentFamily.Hands);
            var reply = Owner.SendMessage(this, ComponentFamily.Hands, ComponentMessageType.IsHandEmpty, h);
            if (reply.MessageType == ComponentMessageType.IsHandEmptyReply && (bool)reply.ParamsList[0])
            {
                UnEquipEntity(e);
                Owner.SendMessage(this, ComponentMessageType.PickUpItemToHand, e, h);
            }
        }

        // Unequips entity e 
        private void UnEquipEntity(Entity e)
        {
            EquipmentSlot key;
            foreach (var kvp in equippedEntities)
            {
                if(kvp.Value == e)
                {
                    key = kvp.Key;
                    UnEquipEntity(key);
                    break;
                }
            }
        }
        
        // Unequips all entites
        private void UnEquipAllEntities()
        {
            foreach (Entity e in equippedEntities.Values)
            {
                UnEquipEntity(e);
            }
        }

        private bool IsItem(Entity e)
        {
            if (e.HasComponent(SS13_Shared.GO.ComponentFamily.Item)) //We can only equip items derp
                return true;
            return false;
        }

        private Entity GetEntity(EquipmentSlot part)
        {
            if (!IsEmpty(part))
                return equippedEntities[part];
            else
                return null;
        }

        private bool IsEmpty(EquipmentSlot part)
        {
            if (equippedEntities.ContainsKey(part))
                return false;
            return true;
        }

        private void RemoveFromOtherComps(Entity entity)
        {
            Entity holder = null;
            if (entity.HasComponent(ComponentFamily.Item)) holder = ((BasicItemComponent)entity.GetComponent(ComponentFamily.Item)).currentHolder;
            if (holder == null && entity.HasComponent(ComponentFamily.Equippable)) holder = ((EquippableComponent)entity.GetComponent(ComponentFamily.Equippable)).currentWearer;
            if (holder != null) holder.SendMessage(this, ComponentMessageType.DisassociateEntity, entity);
            else Owner.SendMessage(this, ComponentMessageType.DisassociateEntity, entity);
        }

        private bool CanEquip(Entity e)
        {
            if(!e.HasComponent(ComponentFamily.Equippable))
                return false;

            var reply = e.SendMessage(this, ComponentFamily.Equippable, ComponentMessageType.GetWearLoc);
            if (reply.MessageType == ComponentMessageType.ReturnWearLoc)
            {
                if (IsItem(e) && IsEmpty((EquipmentSlot)reply.ParamsList[0]) && e != null && activeSlots.Contains((EquipmentSlot)reply.ParamsList[0]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
