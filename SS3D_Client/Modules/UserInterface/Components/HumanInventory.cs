﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using GorgonLibrary;
using GorgonLibrary.Framework;
using GorgonLibrary.GUI;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Utilities;
using GorgonLibrary.InputDevices;
using ClientResourceManager;
using SS3D.Modules;

using CGO;

namespace SS3D.UserInterface
{
    public class HumanInventory : GuiComponent
    {
        private Dictionary<GUIBodyPart, ItemSlot> inventorySlots;
        private HumanHandsGui handsGUI;
        private WindowComponent window;
        private Entity heldEntity;
        private GUIBodyPart lastSlot = GUIBodyPart.None;
        private Vector2D mousePos;
        private Sprite outline;
        private int slotWidth;

        public HumanInventory(PlayerController _playerController)
            : base(_playerController)
        {
            componentClass = SS3D_shared.GuiComponentType.HumanInventory;
            inventorySlots = new Dictionary<GUIBodyPart, ItemSlot>();
            outline = ResMgr.Singleton.GetSprite("outline");
            slotWidth = (int)UIDesktop.Singleton.Skin.Elements["Window.InventorySlot"].Dimensions.Width;
            int width = 48 + slotWidth + (int)outline.Width + slotWidth;
            int height = 64 + (int)(outline.Height);
            clientArea = new Rectangle(Gorgon.Screen.Width - 25 - width, 600, width, height);
            mousePos = Vector2D.Zero;
            window = new WindowComponent(_playerController, clientArea.X, clientArea.Y, clientArea.Width, clientArea.Height);

            SetVisible(false);
        }

        // Set up all the slots for the body
        private void SetUpSlots()
        {
            inventorySlots.Clear();
            if (!playerController.controlledAtom.HasComponent(SS3D_shared.GO.ComponentFamily.Equipment))
                return;

            Entity m = (Entity)playerController.controlledAtom;
            EquipmentComponent ec = (EquipmentComponent)m.GetComponent(SS3D_shared.GO.ComponentFamily.Equipment);
            foreach (GUIBodyPart part in ec.activeSlots)
            {
                inventorySlots.Add(part, new ItemSlot(playerController, part));
            }
            int i = 0;
            bool second = false;
            foreach (ItemSlot slot in inventorySlots.Values)
            {
                if (i >= inventorySlots.Count / 2)
                {
                    second = true;
                    i = 0;
                }
                if (!second)
                    slot.Position = new Point(clientArea.X + slot.Position.X + 12, clientArea.Y + slot.Position.Y + (i * 56));
                else
                    slot.Position = new Point(clientArea.X + clientArea.Width - 12 - slotWidth, clientArea.Y + slot.Position.Y + (i * 56));
                slot.SetOutlinePosition(new Vector2D(clientArea.X + (int)(clientArea.Width / 2) - (int)(outline.Width / 2), clientArea.Y + (clientArea.Height / 2) - (outline.Height / 2)));
                i++;
            }
        }

        // Set up the handsgui reference so we can interact with it
        public void SetHandsGUI(HumanHandsGui _handsGUI)
        {
            handsGUI = _handsGUI;
        }

        public override void HandleNetworkMessage(Lidgren.Network.NetIncomingMessage message)
        {
            base.HandleNetworkMessage(message);
        }

        public override bool KeyDown(GorgonLibrary.InputDevices.KeyboardInputEventArgs e)
        {
            if (e.Key == KeyboardKeys.I)
            {
                ToggleVisible();
                return true;
            }
            return false;
        }

        [Obsolete("TODO: Change to new system")]
        public override bool MouseDown(GorgonLibrary.InputDevices.MouseInputEventArgs e)
        {
            Entity m = (Entity)playerController.controlledAtom;
            EquipmentComponent ec = (EquipmentComponent)m.GetComponent(SS3D_shared.GO.ComponentFamily.Equipment);
            HumanHandsComponent hh = (HumanHandsComponent)m.GetComponent(SS3D_shared.GO.ComponentFamily.Hands);
            //// Check which slot we clicked (if any) and get the atom from in there
            if (heldEntity == null)
            {
                heldEntity = handsGUI.GetActiveHandItem();
                lastSlot = GUIBodyPart.None;
            }
            foreach (ItemSlot slot in inventorySlots.Values)
            {
                if (slot.MouseDown(e))
                {
                    if (!AttemptEquipInSlot(m, slot))
                    {
                        if (ec.equippedEntities.ContainsKey(slot.GetBodyPart()))
                        {
                            heldEntity = ec.equippedEntities[slot.GetBodyPart()];
                            lastSlot = slot.GetBodyPart();
                        }
                    }
                    return true;
                }
            }

            if (handsGUI != null) // Otherwise see if we clicked on one of our hands and get that atom if so
            {
                if (handsGUI.MouseDown(e))
                {
                    if (hh.HandSlots.ContainsKey(hh.currentHand))
                    {
                        heldEntity = hh.HandSlots[hh.currentHand];
                        lastSlot = GUIBodyPart.None;
                        return true;
                    }
                }
            }
            return false;
        }

        [Obsolete("TODO: Change to new system")]
        public override bool MouseUp(GorgonLibrary.InputDevices.MouseInputEventArgs e)
        {
            if (heldEntity == null)
                return false;
            Entity m = (Entity)playerController.controlledAtom;
            EquipmentComponent ec = (EquipmentComponent)m.GetComponent(SS3D_shared.GO.ComponentFamily.Equipment);

            // Check which slot we released the mouse on, and equip the item there
            // (remembering to unequip it from wherever it came from)
            foreach (ItemSlot slot in inventorySlots.Values)
            {
                if (slot.MouseUp(e))
                {
                    if (lastSlot == GUIBodyPart.None)
                        ec.DispatchEquipFromHand();
                    else
                        ec.DispatchEquip(heldEntity.Uid);
                }
            }

            // If we dropped it on a hand we call Click which will equip it
            if (handsGUI != null)
            {
                if (handsGUI.MouseDown(e))
                {
                    if (lastSlot != GUIBodyPart.None) // It came from the inventory
                    {
                        ec.DispatchUnEquipToHand(heldEntity.Uid);
                    }
                }
            }

            heldEntity = null;
            return false;
        }

        public bool AttemptEquipInSlot(Entity m, ItemSlot slot)
        {
            if (slot.CanAccept(heldEntity))
            {
                if (lastSlot != GUIBodyPart.None) // It came from the inventory
                {
                    lastSlot = GUIBodyPart.None;
                }
                heldEntity = null;
                return true;
            }
            return false;
        }

        public override void MouseMove(MouseInputEventArgs e)
        {
            mousePos = e.Position;
        }

        public override void Render()
        {
            if (!IsVisible())
                return;

            window.Render();

            if (inventorySlots.Count == 0 &&
                playerController.controlledAtom != null)
                SetUpSlots();

            outline.Position = new Vector2D(clientArea.X + (int)(clientArea.Width / 2) - (int)(outline.Width / 2), clientArea.Y + (clientArea.Height / 2) - (outline.Height / 2));
            outline.Draw();

            foreach (ItemSlot slot in inventorySlots.Values)
            {
                if (slot.CanAccept(heldEntity))
                    slot.Highlight();
                slot.Render();
            }

            if (heldEntity != null)
            {
                Sprite s = SS3D.HelperClasses.Utilities.GetSpriteComponentSprite(heldEntity);
                s.Position = mousePos;
                s.Draw();
            }
        }
    }

    
}
