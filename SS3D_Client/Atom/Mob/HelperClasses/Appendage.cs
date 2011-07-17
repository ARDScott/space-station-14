﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SS3D.Atom.Mob.HelperClasses
{
    public class Appendage
    {
        public string bone;
        public string appendageName;
        public Mob owner;
        public Item.Item attachedItem;

        public Appendage(string _bone, string _appendageName, Mob _owner)
        {
            bone = _bone;
            appendageName = _appendageName;
            owner = _owner;
        }
    }
}
