﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using SS3D_shared.HelperClasses;
using System.Runtime.Serialization;

namespace SS3D_Server.Atom.Item.Misc
{
    [Serializable()]
    public class Flashlight : Item
    {

        public Light light;

        public Flashlight()
            : base()
        {
            name = "Flashlight";
            Random r = new Random(DateTime.Now.Millisecond);
            int milli = DateTime.Now.Millisecond;
            while (milli == DateTime.Now.Millisecond)
            {
            }
            light = new Light(new Color((byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255)), (Direction)r.Next(3));
            light.Normalize();
        }

        public override void SerializedInit()
        {
            base.SerializedInit();
            Random r = new Random(DateTime.Now.Millisecond);
            int milli = DateTime.Now.Millisecond;
            while (milli == DateTime.Now.Millisecond)
            {
            }
            light = new Light(new Color((byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255)), (Direction)r.Next(3));
            light.Normalize();
        }

        public override void SendState(Lidgren.Network.NetConnection client)
        {
            base.SendState(client);

            NetOutgoingMessage msg = CreateAtomMessage();
            msg.Write((byte)AtomMessage.Push);
            msg.Write(light.color.r);
            msg.Write(light.color.g);
            msg.Write(light.color.b);
            msg.Write((byte)light.direction);
            SendMessageTo(msg, client);
        }

        public override void SendState()
        {
            base.SendState();

            NetOutgoingMessage msg = CreateAtomMessage();
            msg.Write((byte)AtomMessage.Push);
            msg.Write(light.color.r);
            msg.Write(light.color.g);
            msg.Write(light.color.b);
            msg.Write((byte)light.direction);
            SendMessageToAll(msg);
        }



        public Flashlight(SerializationInfo info, StreamingContext ctxt)
        {
            SerializeBasicInfo(info, ctxt);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            base.GetObjectData(info, ctxt);
        }
    }
}
