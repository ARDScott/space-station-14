﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CGO
{
    public class RenderableComponent : GameObjectComponent, IGameObjectComponent, IRenderableComponent
    {
        private int m_drawDepth;
        public int DrawDepth
        {
            get { return m_drawDepth; }
            set { m_drawDepth = value; }
        }
        public RenderableComponent()
        {
            family = SS3D_shared.GO.ComponentFamily.Renderable; 
        }

        public virtual void Render()
        {

        }
    }
}
