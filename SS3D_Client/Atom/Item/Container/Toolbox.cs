﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CGO;

namespace SS3D.Atom.Item.Container
{
    public class Toolbox : Item
    {
        public Toolbox()
            : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            AddComponent(SS3D_shared.GO.ComponentFamily.Renderable, ComponentFactory.Singleton.GetComponent("ItemSpriteComponent"));
            IGameObjectComponent c = (IGameObjectComponent)GetComponent(SS3D_shared.GO.ComponentFamily.Renderable);
            c.SetParameter(new ComponentParameter("basename", typeof(string), "toolbox"));
        }
    }
}
