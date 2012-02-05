﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ClientServices.Resources;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using GorgonLibrary.GUI;
using SS13.UserInterface;
using Lidgren.Network;
using SS13_Shared;
using SS13_Shared.GO;
using CGO;
using SS13.Modules;

namespace SS13.UserInterface
{
    class EquipmentSlotUi : GuiComponent
    {
        public delegate void InventorySlotUiDropHandler(EquipmentSlotUi sender, Entity dropped);
        public event InventorySlotUiDropHandler Dropped;

        public EquipmentSlot assignedSlot { get; private set; }
        public Entity currentEntity { get; private set; }

        private Color color = Color.White;

        private Sprite buttonSprite;
        private Sprite currentEntSprite;

        private TextSprite text;

        private PlayerController playerControler;

        public EquipmentSlotUi(EquipmentSlot slot, PlayerController controler)
            : base(controler)
        {
            assignedSlot = slot;
            playerControler = controler;
            buttonSprite = ResourceManager.GetSprite("slot");
            text = new TextSprite(slot.ToString() + "UIElementSlot", slot.ToString(), ResourceManager.GetFont("CALIBRI"));
            text.ShadowColor = Color.Black;
            text.ShadowOffset = new Vector2D(1, 1);
            text.Shadowed = true;
            text.Color = Color.White;
            Update();
        }

        public override void Update()
        {
            buttonSprite.Position = Position;
            clientArea = new Rectangle(Position, new Size((int)buttonSprite.AABB.Width, (int)buttonSprite.AABB.Height));

            text.Position = position;

            if (playerController.ControlledEntity == null)
                return;

            var entity = (Entity)playerController.ControlledEntity;
            EquipmentComponent equipment = (EquipmentComponent)entity.GetComponent(ComponentFamily.Equipment);

            if (equipment.equippedEntities.ContainsKey(assignedSlot))
            {
                currentEntity = equipment.equippedEntities[assignedSlot];
                currentEntSprite = Utilities.GetSpriteComponentSprite(currentEntity);
            }
            else
            {
                currentEntity = null;
                currentEntSprite = null;
            } 
        }

        public override void Render()
        {
            buttonSprite.Color = color;
            buttonSprite.Position = Position;
            buttonSprite.Draw();
            buttonSprite.Color = Color.White;

            if (currentEntSprite != null && currentEntity != null)
                currentEntSprite.Draw(new Rectangle((int)(position.X + buttonSprite.AABB.Width / 2f - currentEntSprite.AABB.Width / 2f), (int)(position.Y + buttonSprite.AABB.Height / 2f - currentEntSprite.AABB.Height / 2f), (int)currentEntSprite.Width, (int)currentEntSprite.Height));

            text.Draw();
        }

        public override void Dispose()
        {
            buttonSprite = null;
            Dropped = null;
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override bool MouseDown(MouseInputEventArgs e)
        {
            if (clientArea.Contains(new Point((int)e.Position.X, (int)e.Position.Y)))
            {
                if (playerController.ControlledEntity == null)
                    return false;

                var entity = (Entity)playerController.ControlledEntity;
                EquipmentComponent equipment = (EquipmentComponent)entity.GetComponent(ComponentFamily.Equipment);

                if (equipment.equippedEntities.ContainsKey(assignedSlot))
                    UiManager.dragInfo.StartDrag(equipment.equippedEntities[assignedSlot]);

                return true;
            }
            return false;
        }

        public override bool MouseUp(MouseInputEventArgs e)
        {
            if (clientArea.Contains(new Point((int)e.Position.X, (int)e.Position.Y)))
            {
                if (playerController.ControlledEntity == null)
                    return false;

                var entity = (Entity)playerController.ControlledEntity;
                EquipmentComponent equipment = (EquipmentComponent)entity.GetComponent(ComponentFamily.Equipment);
                HumanHandsComponent hands = (HumanHandsComponent)entity.GetComponent(ComponentFamily.Hands);

                if (currentEntity != null && currentEntity == UiManager.dragInfo.dragEntity && hands.IsHandEmpty(hands.currentHand)) //Dropped from us to us. (Try to) unequip it to active hand.
                {
                    UiManager.dragInfo.Reset();
                    equipment.DispatchUnEquipToHand(currentEntity.Uid);
                    return true;
                }
                else
                {
                    if (currentEntity == null && UiManager.dragInfo.isEntity && UiManager.dragInfo.dragEntity != null)
                    {
                        if (Dropped != null) Dropped(this, UiManager.dragInfo.dragEntity);
                        return true;
                    }
                }
            }
            return false;
        }

        public override void MouseMove(MouseInputEventArgs e)
        {
            if (clientArea.Contains(new Point((int)e.Position.X, (int)e.Position.Y)))
                color = Color.LightSteelBlue;
            else
                color = Color.White;
        }

        private bool IsEmpty()
        {
            if (playerController.ControlledEntity == null)
                return false;

            var entity = (Entity)playerController.ControlledEntity;
            EquipmentComponent equipment = (EquipmentComponent)entity.GetComponent(ComponentFamily.Equipment);

            if (equipment.IsEmpty(assignedSlot)) return true;
            else return false;
        }
    }
}
