using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace Lockpicks;

public class Lockpick
{
    public static string ItemName { get; set; }
    public static string TranslatedName { get; set; }
    public static string TranslatedDiscription { get; set; }
    public static string Texture { get; set; }
    public static string ItemId = "bwdy.lockpicks";
    public static int ParentSheetIndex = 0;
    public static int Price { get; set; }
    public static int Edibility { get; set; } = -300;
    public static  int Category = Object.junkCategory;
    public static  string Type = "Crafting";

    public Dictionary<int, int> CraftingIngredients = new Dictionary<int, int>()
    {
        { 335, 2 }
    };
}