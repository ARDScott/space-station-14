﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SS3D_shared;
using SS3D_shared.HelperClasses;
using Lidgren.Network;
using SS3D_Server.Modules;
using System.Reflection;
using ServerServices;
using System.Drawing;
using ServerServices.Tiles;
using SGO;

namespace SS3D_Server.Modules
{
    class PlacementManager
    {
        //TO-DO: Expand for multiple permission per mob?
        //       Add support for multi-use placeables (tiles etc.).

        private Boolean editMode = false;               //If true, clients may freely request and place objects.

        public List<PlacementInformation> BuildPermissions = new List<PlacementInformation>(); //Holds build permissions for all mobs. A list of mobs and the objects they're allowed to request and how. One permission per mob.

        #region Singleton
        private static PlacementManager singleton;

        private PlacementManager() { }

        public static PlacementManager Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new PlacementManager();
                }
                return singleton;
            }
        } 
        #endregion

        /// <summary>
        ///  Handles placement related client messages.
        /// </summary>
        public void HandleNetMessage(NetIncomingMessage msg)
        {
            PlacementManagerMessage messageType = (PlacementManagerMessage)msg.ReadByte();

            switch (messageType)
            {
                case PlacementManagerMessage.StartPlacement:
                    break;
                case PlacementManagerMessage.CancelPlacement:
                    break;
                case PlacementManagerMessage.RequestPlacement:
                    HandlePlacementRequest(msg);
                    break;
                case PlacementManagerMessage.EDITMODE_ToggleEditMode: //THIS REALLY NEED ADMINCHECKS OR SOMETHING.
                    editMode = !editMode;
                    SS3D_Server.SS3DServer.Singleton.chatManager.SendChatMessage(ChatChannel.Server, "Edit Mode : " + (editMode ? "On" : "Off"), "", 0);
                    break;
                case PlacementManagerMessage.EDITMODE_GetObject:
                    if (editMode) HandleEditRequest(msg);
                    break;
            }
        }

        private PlacementInformation GetPermission(int uid, PlacementOption alignOpt)
        {
            var permission = from p in BuildPermissions
                             where p.mobUid == uid && p.placementOption == alignOpt
                             select p;

            if (permission.Any()) return permission.First();
            else return null;
        }

        public void HandleEditRequest(NetIncomingMessage msg)
        {
            //TODO RE-ENABLE
            /*string objectType = msg.ReadString();
            AlignmentOptions align = (AlignmentOptions)msg.ReadByte();
            Type fullType = SS3DServer.Singleton.atomManager.GetAtomType(objectType);
            if (fullType != null) StartBuilding(SS3DServer.Singleton.playerManager.GetSessionByConnection(msg.SenderConnection).attachedAtom, 120, objectType, align, editMode);
            else LogManager.Log("Invalid Object Requested : " + "SS3D_Server." + objectType);*/ 
        }

        public void HandlePlacementRequest(NetIncomingMessage msg)
        {
            PlacementOption alignRcv = (PlacementOption)msg.ReadByte();

            Boolean isTile = msg.ReadBoolean();

            TileType tileType = TileType.None;
            string entityTemplateName = "";

            if (isTile) tileType = (TileType)msg.ReadInt32();
            else entityTemplateName = msg.ReadString();

            float xRcv = msg.ReadFloat();
            float yRcv = msg.ReadFloat();
            float rotRcv = msg.ReadFloat();

            PlacementInformation permission = GetPermission(SS3DServer.Singleton.playerManager.GetSessionByConnection(msg.SenderConnection).attachedAtom.Uid, alignRcv);
            Boolean isAdmin = SS3DServer.Singleton.playerManager.GetSessionByConnection(msg.SenderConnection).adminPermissions.isAdmin;

            if (permission != null || true) //isAdmin)
            {
                if (!isTile)
                {
                    Entity created = EntityManager.Singleton.SpawnEntity(entityTemplateName, new Vector2(xRcv, yRcv));
                    created.Translate(new Vector2(xRcv, yRcv), rotRcv);
                }
                else
                {
                    //Tile here
                }
            }

            //if (GetPermission(SS3DServer.Singleton.playerManager.GetSessionByConnection(msg.SenderConnection).attachedAtom.Uid, alignRcv) != null)
            //{
            //    //DO PLACEMENT CHECKS. Are they allowed to place this here?
            //    PlacementInformation permission = GetPermission(SS3DServer.Singleton.playerManager.GetSessionByConnection(msg.SenderConnection).attachedAtom.Uid, alignRcv);

            //    if (!editMode)
            //    {
            //        BuildPermissions.Remove(permission);
            //        SendPlacementCancel(SS3DServer.Singleton.playerManager.GetSessionByConnection(msg.SenderConnection).attachedAtom);
            //    }
            //     //TODO RE-ENABLE
            //    /*Type objectType = SS3DServer.Singleton.atomManager.GetAtomType(permission.type);

            //    if (objectType.IsSubclassOf(typeof(Tile)))
            //    {
            //        Point arrayPos = SS3D_Server.SS3DServer.Singleton.map.GetTileArrayPositionFromWorldPosition(new Vector2(xRcv, yRcv));
            //        SS3D_Server.SS3DServer.Singleton.map.ChangeTile(arrayPos.X, arrayPos.Y, objectType);
            //        SS3D_Server.SS3DServer.Singleton.map.NetworkUpdateTile(arrayPos.X, arrayPos.Y);
            //    }
            //    else
            //    { //TODO RE-ENABLE
            //        //SS3D_Server.SS3DServer.Singleton.atomManager.SpawnAtom(permission.type, new Vector2(xRcv, yRcv), rotRcv);
            //    }
            //    */
            //}
            //else //They are not allowed to request this. Send 'PlacementFailed'. TBA
            //{
            //    LogManager.Log("Invalid placement request: " 
            //        + SS3DServer.Singleton.playerManager.GetSessionByConnection(msg.SenderConnection).name +
            //        " - " + SS3DServer.Singleton.playerManager.GetSessionByConnection(msg.SenderConnection).attachedAtom.Uid.ToString() +
            //        " - " + alignRcv.ToString());

            //    SendPlacementCancel(SS3DServer.Singleton.playerManager.GetSessionByConnection(msg.SenderConnection).attachedAtom);
            //}
        }

        /// <summary>
        ///  Places mob in entity placement mode with given settings.
        /// </summary>
        public void SendPlacementBegin(Entity mob, ushort range, string objectType, PlacementOption alignOption)
        {
            NetOutgoingMessage message = SS3DNetServer.Singleton.CreateMessage();
            message.Write((byte)NetMessage.PlacementManagerMessage);
            message.Write((byte)PlacementManagerMessage.StartPlacement);
            message.Write(range);
            message.Write(false);//Not a tile
            message.Write(objectType);
            message.Write((byte)alignOption);

            List<ComponentReplyMessage> replies = new List<ComponentReplyMessage>();
            mob.SendMessage(this, SS3D_shared.GO.ComponentMessageType.GetActorConnection, replies);
            if (replies.Count > 0 && replies[0].messageType == SS3D_shared.GO.ComponentMessageType.ReturnActorConnection)
                SS3DNetServer.Singleton.SendMessage(message, (NetConnection)replies[0].paramsList[0], NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        ///  Places mob in entity placement mode with given settings.
        /// </summary>
        public void SendPlacementBegin(Entity mob, ushort range, TileType tileType, PlacementOption alignOption)
        {
            NetOutgoingMessage message = SS3DNetServer.Singleton.CreateMessage();
            message.Write((byte)NetMessage.PlacementManagerMessage);
            message.Write((byte)PlacementManagerMessage.StartPlacement);
            message.Write(range);
            message.Write(true);//Is a tile.
            message.Write((int)tileType);
            message.Write((byte)alignOption);

            List<ComponentReplyMessage> replies = new List<ComponentReplyMessage>();
            mob.SendMessage(this, SS3D_shared.GO.ComponentMessageType.GetActorConnection, replies);
            if (replies.Count > 0 && replies[0].messageType == SS3D_shared.GO.ComponentMessageType.ReturnActorConnection)
                SS3DNetServer.Singleton.SendMessage(message, (NetConnection)replies[0].paramsList[0], NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        ///  Cancels object placement mode for given mob.
        /// </summary>
        public void SendPlacementCancel(Entity mob)
        {
            NetOutgoingMessage message = SS3DNetServer.Singleton.CreateMessage();
            message.Write((byte)NetMessage.PlacementManagerMessage);
            message.Write((byte)PlacementManagerMessage.CancelPlacement);
            List<ComponentReplyMessage> replies = new List<ComponentReplyMessage>();
            mob.SendMessage(this, SS3D_shared.GO.ComponentMessageType.GetActorConnection, replies);
            if(replies.Count > 0 && replies[0].messageType == SS3D_shared.GO.ComponentMessageType.ReturnActorConnection)
                SS3DNetServer.Singleton.SendMessage(message, (NetConnection)replies[0].paramsList[0], NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        ///  Gives Mob permission to place entity and places it in object placement mode.
        /// </summary>
        public void StartBuilding(Entity mob, ushort range, string objectType, PlacementOption alignOption)
        {
            AssignBuildPermission(mob, range, objectType, alignOption);
            SendPlacementBegin(mob, range, objectType, alignOption);
        }

        /// <summary>
        ///  Gives Mob permission to place tile and places it in object placement mode.
        /// </summary>
        public void StartBuilding(Entity mob, ushort range, TileType tileType, PlacementOption alignOption)
        {
            AssignBuildPermission(mob, range, tileType, alignOption);
            SendPlacementBegin(mob, range, tileType, alignOption);
        }

        /// <summary>
        ///  Revokes open placement Permission and cancels object placement mode.
        /// </summary>
        public void CancelBuilding(Entity mob)
        {
            RevokeAllBuildPermissions(mob);
            SendPlacementCancel(mob);
        }

        /// <summary>
        ///  Gives a mob a permission to place a given Entity.
        /// </summary>
        public void AssignBuildPermission(Entity mob, ushort range, string objectType, PlacementOption alignOption)
        {
            PlacementInformation newPermission = new PlacementInformation();
            newPermission.mobUid = mob.Uid;
            newPermission.range = range;
            newPermission.isTile = false;
            newPermission.entityType = objectType;
            newPermission.placementOption = alignOption;

            var mobPermissions = from PlacementInformation permission in BuildPermissions
                                 where permission.mobUid == mob.Uid
                                 select permission;

            if (mobPermissions.Any()) //Already has one? Revoke the old one and add this one.
            {
                RevokeAllBuildPermissions(mob);
                BuildPermissions.Add(newPermission);
            }
            else
            {
                BuildPermissions.Add(newPermission);
            }
        }

        /// <summary>
        ///  Gives a mob a permission to place a given Tile.
        /// </summary>
        public void AssignBuildPermission(Entity mob, ushort range, TileType tileType, PlacementOption alignOption)
        {
            PlacementInformation newPermission = new PlacementInformation();
            newPermission.mobUid = mob.Uid;
            newPermission.range = range;
            newPermission.isTile = true;
            newPermission.tileType = tileType;
            newPermission.placementOption = alignOption;

            var mobPermissions = from PlacementInformation permission in BuildPermissions
                                 where permission.mobUid == mob.Uid
                                 select permission;

            if (mobPermissions.Any()) //Already has one? Revoke the old one and add this one.
            {
                RevokeAllBuildPermissions(mob);
                BuildPermissions.Add(newPermission);
            }
            else
            {
                BuildPermissions.Add(newPermission);
            }
        }

        /// <summary>
        ///  Removes all building Permissions for given mob.
        /// </summary>
        public void RevokeAllBuildPermissions(Entity mob)
        {
            var mobPermissions = from PlacementInformation permission in BuildPermissions
                                 where permission.mobUid == mob.Uid
                                 select permission;

            if (mobPermissions.Any())
                BuildPermissions.RemoveAll(x => mobPermissions.Contains(x));
        }
    }
}
