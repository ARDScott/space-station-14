﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGO.Component.Health.Organs.Internal
{
    public class Heart : Internal
    {
        private float lastBeat = 0;
        private float beatTime = 1000; // Beat (update) roughly once per second
        public Heart()
            : base()
        {
            name = "Heart";
            
        }

        public override void SetUp(HealthComponent _owner)
        {
            max_blood = 0;
            masterConnectionType = typeof(Organs.External.Torso);
            base.SetUp(_owner);
        }

        public override void Process(float frametime)
        {
            lastBeat += frametime;
            if (lastBeat < beatTime)
                return;
            lastBeat = 0;
            masterConnection.HeartBeat();
            base.Process(frametime);
        }
    }
}
