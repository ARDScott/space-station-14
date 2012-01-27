﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SS3D_shared.GO;

namespace CGO
{
    public class EquippableComponent : GameObjectComponent
    {
        public EquippableComponent()
        {
            family = SS3D_shared.GO.ComponentFamily.Equippable;
        }

        public override void HandleNetworkMessage(IncomingEntityComponentMessage message)
        {
            //base.HandleNetworkMessage(message);

            switch((EquippableComponentNetMessage)message.messageParameters[0])
            {
                case EquippableComponentNetMessage.Equipped:
                    EquippedBy((int)message.messageParameters[1], (EquipmentSlot)message.messageParameters[2]);
                    break;
                case EquippableComponentNetMessage.UnEquipped:
                    UnEquipped();
                    break;
            }
        }

        private void EquippedBy(int uid, EquipmentSlot wearloc)
        {
            Owner.SendMessage(this, ComponentMessageType.ItemEquipped, null);
            Owner.AddComponent(ComponentFamily.Mover, ComponentFactory.Singleton.GetComponent("SlaveMoverComponent"));
            Owner.SendMessage(this, ComponentMessageType.SlaveAttach, null, uid);
            switch(wearloc)
            {
                case EquipmentSlot.Back:
                    SendDrawDepth(DrawDepth.MobOverAccessoryLayer);
                    break;
                case EquipmentSlot.Belt:
                    SendDrawDepth(DrawDepth.MobUnderAccessoryLayer);
                    break;
                case EquipmentSlot.Ears:
                    SendDrawDepth(DrawDepth.MobUnderAccessoryLayer);
                    break;
                case EquipmentSlot.Eyes:
                    SendDrawDepth(DrawDepth.MobUnderAccessoryLayer);
                    break;
                case EquipmentSlot.Feet:
                    SendDrawDepth(DrawDepth.MobUnderClothingLayer);
                    break;
                case EquipmentSlot.Hands:
                    SendDrawDepth(DrawDepth.MobOverAccessoryLayer);
                    break;
                case EquipmentSlot.Head:
                    SendDrawDepth(DrawDepth.MobOverClothingLayer);
                    break;
                case EquipmentSlot.Inner:
                    SendDrawDepth(DrawDepth.MobUnderClothingLayer);
                    break;
                case EquipmentSlot.Mask:
                    SendDrawDepth(DrawDepth.MobUnderAccessoryLayer);
                    break;
                case EquipmentSlot.Outer:
                    SendDrawDepth(DrawDepth.MobOverClothingLayer);
                    break;
            }
        }

        private void SendDrawDepth(SS3D_shared.GO.DrawDepth dd)
        {
            Owner.SendMessage(this, ComponentMessageType.SetWornDrawDepth, null, dd);
        }

        private void UnEquipped()
        {
            Owner.SendMessage(this, ComponentMessageType.ItemUnEquipped, null);
            Owner.AddComponent(ComponentFamily.Mover, ComponentFactory.Singleton.GetComponent("NetworkMoverComponent"));
        }
    }
}
