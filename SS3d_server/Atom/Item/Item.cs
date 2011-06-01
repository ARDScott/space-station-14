﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SS3D_shared.HelperClasses;
using Lidgren.Network;
using SS3d_server.Atom.Mob;
using SS3d_server.Atom.Mob.HelperClasses;

namespace SS3d_server.Atom.Item
{
    public class Item : Atom
    {
        public Appendage holdingAppendage = null;

        public Item()
            : base()
        {

        }

        protected override void HandleExtendedMessage(NetIncomingMessage message)
        {
            ItemMessage i = (ItemMessage)message.ReadByte();
            switch (i)
            {
                default:
                    break;
            }
        }

        protected override void HandleClick(NetIncomingMessage message)
        {
            //base.HandleClick(message);
            //Who clicked us?
            Mob.Mob clicker = (Mob.Mob)atomManager.netServer.playerManager.GetSessionByConnection(message.SenderConnection).attachedAtom;
            if (clicker == null)
                return;

            /// If the selected hand is empty, this item should go into that hand.
            /// Otherwise, the item that is in that hand is used on this item.
            if (clicker.selectedAppendage.heldItem == null)
                PickedUpBy(clicker);
            else
                clicker.selectedAppendage.heldItem.UsedOn(this);

        }

        /// <summary>
        /// This is called when a mob picks up an item.
        /// </summary>
        /// <param name="uid">an atom uid that has just picked up this item</param>
        public virtual void PickedUpBy(Mob.Mob newHolder)
        {
            holdingAppendage = newHolder.selectedAppendage;

            /// Ok, this will send a message to all clients saying that 
            /// this item is now attached to a certain appendage on the mob with id uid.
            NetOutgoingMessage outmessage = CreateAtomMessage();
            outmessage.Write((byte)ItemMessage.AttachTo);
            outmessage.Write(newHolder.uid);
            outmessage.Write(holdingAppendage.appendageName);
            atomManager.netServer.SendMessageToAll(outmessage);

            
        }

        /// <summary>
        /// Called when a mob drops an item
        /// </summary>
        /// <param name="uid">mob that dropped the item.</param>
        public virtual void Dropped(ushort uid)
        {

        }

        public virtual void UsedOn(Item i)
        {

        }

        public override void Push()
        {
            base.Push();

        }
    }
}
