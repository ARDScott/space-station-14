﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SS13.IoC;
using SS13_Shared;
using SS13_Shared.GO;
using ServerInterfaces.Chat;
using ServerInterfaces.GameObject;
using ServerServices;
using ServerServices.Map;
using ServerServices.Tiles;
using ServerInterfaces.Map;

namespace SGO
{
    public class BasicDoorComponent : BasicLargeObjectComponent
    {
        private bool Open;
        private bool autoclose = true;
        private string closedSprite = "";
        private float openLength = 5000;
        private string openSprite = "";
        private bool openonbump;
        private float timeOpen;
        private bool disabled = false;

        public BasicDoorComponent()
        {
            family = ComponentFamily.LargeObject;

            RegisterSVar("OpenOnBump", typeof(bool));
        }

        public override ComponentReplyMessage RecieveMessage(object sender, ComponentMessageType type,
                                                             params object[] list)
        {
            ComponentReplyMessage reply = base.RecieveMessage(sender, type, list);

            if (sender == this)
                return ComponentReplyMessage.Empty;

            switch (type)
            {
                case ComponentMessageType.Bumped:
                    if (openonbump)
                        OpenDoor();
                    break;
            }

            return reply;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (disabled) return;

            if (Open && autoclose)
            {
                timeOpen += frameTime;
                if (timeOpen >= openLength)
                    CloseDoor();
            }
        }

        public override void OnAdd(IEntity owner)
        {
            base.OnAdd(owner);
            Owner.OnMove += OnMove;
        }

        public override void OnRemove()
        {
            Owner.OnMove -= OnMove;
            base.OnRemove();
        }

        private void OnMove(Vector2 newPosition, Vector2 oldPosition)
        {
            SetPermeable(oldPosition);
            SetImpermeable(newPosition);
        }

        protected override void RecieveItemInteraction(Entity actor, Entity item,
                                                       Lookup<ItemCapabilityType, ItemCapabilityVerb> verbs)
        {
            base.RecieveItemInteraction(actor, item, verbs);

            if (verbs[ItemCapabilityType.Tool].Contains(ItemCapabilityVerb.Pry))
            {
                ToggleDoor(true);
            }
            else if (verbs[ItemCapabilityType.Tool].Contains(ItemCapabilityVerb.Hit))
            {
                var cm = IoCManager.Resolve<IChatManager>();
                cm.SendChatMessage(ChatChannel.Default,
                                   actor.Name + " hit the " + Owner.Name + " with a " + item.Name + ".", null, item.Uid);
            }
            else if (verbs[ItemCapabilityType.Tool].Contains(ItemCapabilityVerb.Emag))
            {
                OpenDoor();
                disabled = true;
            }
        }

        /// <summary>
        /// Entry point for interactions between an empty hand and this object
        /// Basically, actor "uses" this object
        /// </summary>
        /// <param name="actor">The actor entity</param>
        protected override void HandleEmptyHandToLargeObjectInteraction(Entity actor)
        {
            ToggleDoor();
        }

        private void ToggleDoor(bool forceToggle = false)
        {
            //Apply actions
            if (Open)
            {
                CloseDoor(forceToggle);
            }
            else
            {
                OpenDoor(forceToggle);
            }
        }

        private void OpenDoor(bool force = false)
        {
            if (disabled && !force) return;

            var map = IoCManager.Resolve<IMapManager>();
            Point occupiedTilePos = map.GetTileArrayPositionFromWorldPosition(Owner.Position);
            Tile occupiedTile = map.GetTileAt(occupiedTilePos.X, occupiedTilePos.Y) as Tile;
            Open = true;
            Owner.SendMessage(this, ComponentMessageType.DisableCollision);
            Owner.SendMessage(this, ComponentMessageType.SetSpriteByKey, openSprite);
            occupiedTile.gasPermeable = true;
            occupiedTile.gasCell.blocking = false;
        }

        private void CloseDoor(bool force = false)
        {
            if (disabled && !force) return;

            var map = IoCManager.Resolve<IMapManager>();
            Point occupiedTilePos = map.GetTileArrayPositionFromWorldPosition(Owner.Position);
            Tile occupiedTile = map.GetTileAt(occupiedTilePos.X, occupiedTilePos.Y) as Tile;
            Open = false;
            timeOpen = 0;
            Owner.SendMessage(this, ComponentMessageType.EnableCollision);
            Owner.SendMessage(this, ComponentMessageType.SetSpriteByKey, closedSprite);
            occupiedTile.gasPermeable = false;
            occupiedTile.gasCell.blocking = true;
        }

        private void SetImpermeable()
        {
            var map = IoCManager.Resolve<IMapManager>();
            Point occupiedTilePos = map.GetTileArrayPositionFromWorldPosition(Owner.Position);
            Tile occupiedTile = map.GetTileAt(occupiedTilePos.X, occupiedTilePos.Y) as Tile;
            occupiedTile.gasPermeable = false;
            occupiedTile.gasCell.blocking = true;
        }

        private void SetImpermeable(Vector2 position)
        {
            var map = IoCManager.Resolve<IMapManager>();
            Point occupiedTilePos = map.GetTileArrayPositionFromWorldPosition(position);
            Tile occupiedTile = map.GetTileAt(occupiedTilePos.X, occupiedTilePos.Y) as Tile;
            occupiedTile.gasPermeable = false;
            occupiedTile.gasCell.blocking = true;
        }

        private void SetPermeable(Vector2 position)
        {
            var map = IoCManager.Resolve<IMapManager>();
            Point occupiedTilePos = map.GetTileArrayPositionFromWorldPosition(position);
            Tile occupiedTile = map.GetTileAt(occupiedTilePos.X, occupiedTilePos.Y) as Tile;
            occupiedTile.gasPermeable = true;
            occupiedTile.gasCell.blocking = false;
        }

        public override void SetParameter(ComponentParameter parameter)
        {
            base.SetParameter(parameter);

            switch (parameter.MemberName)
            {
                case "OpenSprite":
                    openSprite = parameter.GetValue<string>();
                    break;
                case "ClosedSprite":
                    closedSprite = parameter.GetValue<string>();
                    break;
                case "OpenOnBump":
                    openonbump = parameter.GetValue<bool>();
                    break;
                case "AutoCloseInterval":
                    var autocloseinterval = parameter.GetValue<int>();
                    if (autocloseinterval == 0)
                        autoclose = false;
                    else
                    {
                        autoclose = true;
                        openLength = autocloseinterval;
                    }
                    break;
                default:
                    base.SetParameter(parameter);
                    break;
            }
        }

        public override List<ComponentParameter> GetParameters()
        {
            var cparams = base.GetParameters();
            cparams.Add(new ComponentParameter("OpenOnBump", openonbump));
            return cparams;
        }
    }
}