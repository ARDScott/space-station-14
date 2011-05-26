﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace SS3D_shared
{
    public class Floor : BaseTile
    {
        public Floor(SceneManager sceneManager, Vector3 position, int tileSpacing)
            : base(tileSpacing)
        {
            TileType = TileType.Floor;
            string entityName;
            name = "floor";
            meshName = "floorMesh";
            entityName = "0" + position.x + "0" + position.z;
            if (sceneManager.HasEntity(entityName))
            {
                sceneManager.DestroyEntity(entityName);
            }
            if (sceneManager.HasSceneNode(entityName))
            {
                sceneManager.DestroySceneNode(entityName);
            }
            Node = sceneManager.RootSceneNode.CreateChildSceneNode(entityName);
            Entity = sceneManager.CreateEntity(entityName, meshName);
            Entity.QueryFlags = HelperClasses.QueryFlags.ENTITY_FLOOR;
            Node.Position = position;
            Node.AttachObject(Entity);
            Entity.UserObject = (AtomBaseClass)this;
            SetGeoPos();
        }

        public Floor()
            : base()
        {
            TileType = TileType.Floor;
            name = "floor";
        }
    }
}
