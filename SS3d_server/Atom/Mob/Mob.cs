﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using SS3D_Server.Atom.Mob.HelperClasses;

namespace SS3D_Server.Atom.Mob
{
    public class Mob : Atom
    {
        public float walkSpeed = 1.0f;
        public float runSpeed = 2.0f;
        public Item.Organs.BLOOD_TYPE blood_type = Item.Organs.BLOOD_TYPE.A; // Temporary
        public List<Item.Organs.Organ> organs = new List<Item.Organs.Organ>();

        public Dictionary<string, HelperClasses.Appendage> appendages;
        public Appendage selectedAppendage;

        public string animationState = "idle";

        public Mob()
            : base()
        {
            Console.Write("MOB!");
            initAppendages();
        }

        public override void Destruct()
        {
            base.Destruct();

            DropAllItems();
        }

        public override void SendState(NetConnection client)
        {
            base.SendState(client);

            if (IsDead())
                SendDeathMessage(client);
        }

        /// <summary>
        /// Initializes appendage dictionary
        /// </summary>
        protected virtual void initAppendages()
        {
            appendages = new Dictionary<string,Appendage>();
            appendages.Add("LeftHand", new HelperClasses.Appendage("LeftHand", this));
            appendages.Add("RightHand", new HelperClasses.Appendage("RightHand", this));
            appendages["LeftHand"].attackAnimation = "lattack";
            appendages["RightHand"].attackAnimation = "rattack";
            selectedAppendage = appendages["LeftHand"];
        }

        protected override void HandleExtendedMessage(Lidgren.Network.NetIncomingMessage message)
        {
            MobMessage mobMessageType = (MobMessage)message.ReadByte();
            switch (mobMessageType)
            {
                case MobMessage.AnimationState:
                    HandleAnimationState(message);
                    break;
                case MobMessage.DropItem:
                    HandleDropItem();
                    break;
                default: 
                    break;
            }
        }

        protected virtual void HandleAnimationState(NetIncomingMessage message)
        {
            string state = message.ReadString();
            NetOutgoingMessage outmessage = CreateAtomMessage();
            outmessage.Write((byte)AtomMessage.Extended);
            outmessage.Write((byte)MobMessage.AnimationState);
            outmessage.Write(state);
            SendMessageToAll(outmessage);
        }

        /// <summary>
        /// Selects an appendage from the dictionary
        /// </summary>
        /// <param name="appendageName">name of appendage to select</param>
        public virtual void SelectAppendage(string appendageName)
        {
            if (appendages.Keys.Contains(appendageName))
                selectedAppendage = appendages[appendageName];
            SendSelectAppendage();
        }

        /// <summary>
        /// Sends a message to all clients telling them the mob has selected an appendage.
        /// </summary>
        public virtual void SendSelectAppendage()
        {
            NetOutgoingMessage outmessage = CreateAtomMessage();
            outmessage.Write((byte)AtomMessage.Extended);
            outmessage.Write((byte)MobMessage.SelectAppendage);
            outmessage.Write(selectedAppendage.appendageName);
            SendMessageToAll(outmessage);
        }

        public virtual void HandleDropItem()
        {
            if (selectedAppendage.heldItem != null)
                selectedAppendage.heldItem.Dropped();
        }

        public virtual void DropAllItems()
        {
            foreach (Appendage a in appendages.Values)
            {
                if (a.heldItem != null)
                    a.heldItem.Dropped();
            }
        }

        public virtual void AnimateOnce(string animation)
        {
            NetOutgoingMessage outmessage = CreateAtomMessage();
            outmessage.Write((byte)AtomMessage.Extended);
            outmessage.Write((byte)MobMessage.AnimateOnce);
            outmessage.Write(animation);
            SendMessageToAll(outmessage);
        }

        public override void Die()
        {
            base.Die();

            DropAllItems();
            SendDeathMessage();
            //AnimateOnce("death");
        }

        public virtual void SendDeathMessage()
        {
            NetOutgoingMessage msg = CreateAtomMessage();
            msg.Write((byte)AtomMessage.Extended);
            msg.Write((byte)MobMessage.Death);
            SendMessageToAll(msg, NetDeliveryMethod.ReliableOrdered);
        }
        
        public virtual void SendDeathMessage(NetConnection client)
        {
            NetOutgoingMessage msg = CreateAtomMessage();
            msg.Write((byte)AtomMessage.Extended);
            msg.Write((byte)MobMessage.Death);
            SendMessageTo(msg, client, NetDeliveryMethod.ReliableOrdered);
        }

        public override void Damage(int amount)
        {
            base.Damage(amount);

            if (IsDead())
                Die();
        }
    }
}
