using StardewModdingAPI;
using System.Collections.Generic;
using System;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Linq;
using StardewValley.Locations;
using System.CodeDom;
using System.IO;
using JsonAssets;

namespace Lockpicks.Framework;

public class ModConfig
{
    public int Price { get; set; } = 15;
    public int BuyPrice { get; set; } = 500;
    public string CanBuyFrom { get; set; } = "Blacksmith";

}