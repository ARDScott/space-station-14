﻿using CGO;
using GameObject;
using GameObject.System;
using EntityManager = GameObject.EntityManager;

namespace SGO.EntitySystems
{
    public class TestSystem : EntitySystem
    {
        public TestSystem(EntityManager em)
            : base(em)
        {
            EntityQuery = new EntityQuery();
            EntityQuery.OneSet.Add(typeof (BasicItemComponent));
        }

        public override void Update(float frametime)
        {
        }
    }
}