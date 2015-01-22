﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GameObject;
using SS13_Shared.GO;
using SS13_Shared.GO.Component.Hitbox;
using ClientInterfaces.GOC;

namespace CGO {
    public class HitboxComponent : Component {

        public RectangleF AABB { get; set; }
        public SizeF Size {
            get {
                return AABB.Size;
            }
            set {
                AABB = new RectangleF(
                    AABB.Left + (AABB.Width - value.Width),
                    AABB.Top + (AABB.Height - value.Height),
                    value.Width,
                    value.Height
                    );
            }
        }
        public PointF Offset {
            get {
                return new PointF(AABB.Left + AABB.Width / 2f, AABB.Top + AABB.Height / 2f);
            }
            set {
                AABB = new RectangleF(
                    value.X - AABB.Width / 2f,
                    value.Y - AABB.Height / 2f,
                    AABB.Width,
                    AABB.Height
                    );
            }
        }


        public HitboxComponent() {
            Family = ComponentFamily.Hitbox;
            Size = new SizeF();
            Offset = new PointF();
        }

        public override Type StateType {
            get {
                return typeof(HitboxComponentState);
            }
        }

        public override void HandleComponentState(dynamic state) {
            AABB = state.AABB;
        }

    }
}
