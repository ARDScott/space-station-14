﻿using SS14.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS14.Client
{
    public class SceneTreeHolder : ISceneTreeHolder
    {
        public Godot.SceneTree SceneTree
        {
            get => sceneTree;
            set
            {
                if (sceneTree != null)
                {
                    throw new InvalidOperationException("Scene tree has already been set;");
                }

                sceneTree = value ?? throw new ArgumentNullException(nameof(value));

                WorldRoot = new Godot.Node2D();
                WorldRoot.SetName("WorldRoot");
                sceneTree.GetRoot().AddChild(WorldRoot);
            }
        }
        private Godot.SceneTree sceneTree;

        public Godot.Node2D WorldRoot { get; private set; }
    }
}
