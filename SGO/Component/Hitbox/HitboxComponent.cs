﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GameObject;
using SS13_Shared.GO;
using SS13_Shared.GO.Component.Hitbox;

namespace SGO
{
    public class HitboxComponent : Component
    {
        public RectangleF AABB { get; set; }

        public HitboxComponent()
        {
            Family = ComponentFamily.Hitbox;
            AABB = new RectangleF();
        }

        public override ComponentState GetComponentState()
        {
            return new HitboxComponentState(AABB);
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
                case "SizeX":
                    var width = parameter.GetValue<float>();
                    AABB = new RectangleF(AABB.Left + (AABB.Width - width) / 2f, AABB.Top, width, AABB.Height);
                    break;
                case "SizeY":
                    var height = parameter.GetValue<float>();
                    AABB = new RectangleF(AABB.Left, AABB.Top + (AABB.Height - height) / 2f, AABB.Width, height);
                    break;
                case "OffsetX":
                    var x = parameter.GetValue<float>();
                    AABB = new RectangleF(x - AABB.Width / 2f, AABB.Top, AABB.Width, AABB.Height);
                    break;
                case "OffsetY":
                    var y = parameter.GetValue<float>();
                    AABB = new RectangleF(AABB.Left, y - AABB.Height / 2f, AABB.Width, AABB.Height);
                    break;
            }
        }
    }
}
