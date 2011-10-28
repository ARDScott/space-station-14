﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SS3D_shared.HelperClasses
{
    public class PlacementInformation
    {
        public int mobUid;                                                //UID of mob this permission is for.
        public ushort range = 0;                                          //Valid range from mob.

        public Boolean isTile = false;

        public string entityType = "";                                    //Object name / type. If not tile.
        public TileType tileType = TileType.None;                         //Tile Type if tile.

        public PlacementOption placementOption = PlacementOption.AlignTile; //Alignment type. See enum declaration for infos.
    }
}
