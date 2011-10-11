﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SS3D_shared;
using SS3D_shared.GO;
using GorgonLibrary.Graphics;

namespace CGO
{
    public class ItemSpriteComponent : SpriteComponent
    {
        string basename = "";
        private bool IsInHand = false;
        public ItemSpriteComponent()
            : base()
        {
            SetDrawDepth(2);
        }

        public override void RecieveMessage(object sender, ComponentMessageType type, List<ComponentReplyMessage> replies, params object[] list)
        {
            base.RecieveMessage(sender, type, replies, list);

            switch (type)
            {
                case ComponentMessageType.MoveDirection:
                    if (!IsInHand)
                        break;
                    switch ((Constants.MoveDirs)list[0])
                    {
                        case Constants.MoveDirs.north:
                            SetSpriteByKey(basename + "_inhand");
                            flip = true;
                            break;
                        case Constants.MoveDirs.south:
                            SetSpriteByKey(basename + "_inhand");
                            break;
                        case Constants.MoveDirs.east:
                            SetSpriteByKey(basename + "_inhand_side");
                            flip = true;
                            break;
                        case Constants.MoveDirs.west:
                            SetSpriteByKey(basename + "_inhand_side");
                            flip = false;
                            break;
                        case Constants.MoveDirs.northeast:
                            SetSpriteByKey(basename + "_inhand");
                            break;
                        case Constants.MoveDirs.northwest:
                            SetSpriteByKey(basename + "_inhand");
                            break;
                        case Constants.MoveDirs.southeast:
                            SetSpriteByKey(basename + "_inhand");
                            break;
                        case Constants.MoveDirs.southwest:
                            SetSpriteByKey(basename + "_inhand");
                            break;
                    }
                    SetDrawDepth(4);
                    break;
                case ComponentMessageType.Dropped:
                    SetSpriteByKey(basename);
                    IsInHand = false;
                    SetDrawDepth(2);
                    break;
                case ComponentMessageType.PickedUp:
                    IsInHand = true;
                    break;
            }

        }

        /// <summary>
        /// Set parameters :)
        /// </summary>
        /// <param name="parameter"></param>
        public override void SetParameter(ComponentParameter parameter)
        {
            //base.SetParameter(parameter);
            switch (parameter.MemberName)
            {
                case "basename":
                    basename = (string)parameter.Parameter;
                    LoadSprites();
                    break;
            }
        }

        protected override Sprite GetBaseSprite()
        {
            return sprites[basename];
        }

        /// <summary>
        /// Load the mob sprites given the base name of the sprites.
        /// </summary>
        public void LoadSprites()
        {
            AddSprite(basename);
            AddSprite(basename + "_inhand");
            AddSprite(basename + "_inhand_side");

            SetSpriteByKey(basename);
        }
    }
}
