﻿using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using ClientInterfaces.Collision;
using ClientInterfaces.GOC;
using GorgonLibrary;
using SS13.IoC;
using SS13_Shared;
using SS13_Shared.GO;

namespace CGO
{
    public class ColliderComponent : GameObjectComponent
    {
        public override ComponentFamily Family
        {
            get { return ComponentFamily.Collider; }
        }

        /// <summary>
        /// X - Top | Y - Right | Z - Bottom | W - Left
        /// </summary>
        private Vector4D tweakAABB = Vector4D.Zero;
        private Vector4D TweakAABB
        {
            get { return tweakAABB; }
            set { tweakAABB = value; }
        }
        
        private RectangleF currentAABB;
        public RectangleF OffsetAABB
        {
            get
            { // Return tweaked AABB
                if (currentAABB != null)
                    return new RectangleF(currentAABB.Left + Owner.Position.X - (currentAABB.Width / 2) + tweakAABB.W,
                                        currentAABB.Top + Owner.Position.Y - (currentAABB.Height / 2) + tweakAABB.X,
                                        currentAABB.Width - (tweakAABB.W - tweakAABB.Y),
                                        currentAABB.Height - (tweakAABB.X - tweakAABB.Z));
                else
                    return RectangleF.Empty;
            }
        }

        public override ComponentReplyMessage RecieveMessage(object sender, ComponentMessageType type, params object[] list)
        {
            ComponentReplyMessage reply = base.RecieveMessage(sender, type, list);

            if (sender == this) //Don't listen to our own messages!
                return ComponentReplyMessage.Empty;

            switch (type)
            {
                case ComponentMessageType.SpriteChanged:
                    GetAABB();
                    break;
                case ComponentMessageType.CheckCollision:
                    reply = list.Any() ? CheckCollision((bool) list[0]) : CheckCollision();
                    break;
            }

            return reply;
        }

        public override void SetParameter(ComponentParameter parameter)
        {
            base.SetParameter(parameter);

            switch (parameter.MemberName)
            {
                case "TweakAABB":
                        TweakAABB = parameter.GetValue<Vector4D>();
                    break;
            }
        }

        public override void OnAdd(IEntity owner)
        {
            base.OnAdd(owner);
            GetAABB();
        }

        /// <summary>
        /// Gets the current AABB from the sprite component.
        /// </summary>
        private void GetAABB()
        {
            var reply = Owner.SendMessage(this, ComponentFamily.Renderable, ComponentMessageType.GetAABB);
            if (reply.MessageType == ComponentMessageType.CurrentAABB)
            {
                currentAABB = (RectangleF)reply.ParamsList[0];
            }
            else
                return;
        }

        private ComponentReplyMessage CheckCollision(bool SuppressBump = false)
        {
            bool isColliding = false;
            var collisionManager = IoCManager.Resolve<ICollisionManager>();
            isColliding = collisionManager.TryCollide(Owner);
            return new ComponentReplyMessage(ComponentMessageType.CollisionStatus, isColliding);
        }


    }
}
