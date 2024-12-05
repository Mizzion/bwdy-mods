using StardewModdingAPI;
using System.Collections.Generic;
using System;
using StardewValley;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Linq;
using StardewValley.Locations;
using Lockpicks.Framework;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using GenericModConfigMenu;
using xTile;

namespace Lockpicks
{
    public class ModEntry : Mod
    {
        internal static Random RNG = new Random(Guid.NewGuid().GetHashCode());
        internal static HashSet<string> LockCache = new HashSet<string>();
         
        private string GenerateCacheKey(string objectType, GameLocation location, float x, float y) { return objectType + "^" + location.Name + "^" + ((int)x).ToString() + "^" + ((int)y).ToString(); }

        private ModConfig _config;
        private IGenericModConfigMenuApi _cfg;
        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<ModConfig>();
            
            //Set up translations
            I18n.Init(helper.Translation);
            
            //Events
            Helper.Events.GameLoop.DayStarted += (s,e) => { LockCache.Clear(); };
            Helper.Events.Input.ButtonPressed += (s,e) => { OnInput(e); };
            Helper.Events.Multiplayer.ModMessageReceived += OnMultiplayerPacket;
            Helper.Events.GameLoop.GameLaunched += GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.GameLoop.DayEnding += OnDayEnding;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            SetUpRooms();
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            var curRecipes = Game1.player.craftingRecipes.Pairs.ToDictionary(r => r.Key, r => r.Value);
            var player = Game1.player;
            
            foreach (var cr in curRecipes.Where(cr => cr.Key.Contains(Lockpick.TranslatedName)))
            {
                player.craftingRecipes.Remove(cr.Key);
                Monitor.Log($"Removed recipe for {Lockpick.TranslatedName} before the day ended.");
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            var player = Game1.player;

            if (!player.craftingRecipes.ContainsKey(Lockpick.TranslatedName))
            {
                player.craftingRecipes.Add(Lockpick.TranslatedName, 0);
                Monitor.Log($"Added Recipe for {Lockpick.TranslatedName}");
            }
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            //Set up the lock pick
            Lockpick.TranslatedName = I18n.Tool();
            Lockpick.TranslatedDiscription = I18n.Tooltip();
            Lockpick.Texture = Helper.ModContent.GetInternalAssetName("assets/lockpick.png").ToString()
                ?.Replace("/", "\\");
            Lockpick.Price = _config.Price;
            

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, ObjectData>();
                    var lockPick = new ObjectData()
                    {
                        Name = Lockpick.ItemId,
                        DisplayName = Lockpick.TranslatedName,
                        Price = 0,
                        Description = Lockpick.TranslatedDiscription,
                        SpriteIndex = Lockpick.ParentSheetIndex,
                        Texture = Lockpick.Texture,
                        Type = Lockpick.Type,
                        Category = Lockpick.Category,
                        CanBeGivenAsGift = false,
                        Edibility = Lockpick.Edibility
                    };

                    var newItem = new Dictionary<string, ObjectData>()
                    {
                        { Lockpick.ItemId, lockPick }
                    };
                    
                    //Now we add the new item
                    foreach (var ni in newItem.Where(ni => !data.Data.Contains(ni)))
                    {
                        data.Data.Add(ni);
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>();
                    var isEng = asset.Locale != "en" ? $"/{Lockpick.TranslatedName}" : "";
                    var ingredientsOut = $"335 2/Home/{Lockpick.ItemId}/false/null{isEng}";

                    if (data.Data.ContainsKey(Lockpick.TranslatedName))
                        data.Data[Lockpick.TranslatedName] = ingredientsOut;
                    
                    if (!data.Data.ContainsKey(Lockpick.TranslatedName))
                    {
                        try
                        {
                            data.Data.Add(Lockpick.TranslatedName, ingredientsOut);
                            Monitor.Log($"Added Lockpick Recipe {Lockpick.TranslatedName}: {ingredientsOut}");
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log($"There was an error: {ex}", LogLevel.Error);
                        }
                    }

                });
            } 
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, ShopData>();
                    //var canBuyFrom = _config.CanBuyFrom.Replace(" ","").Split(',');
                    //var d = data;
                    foreach (var shop in data.Data)
                    {
                        if (shop.Key.Contains("Blacksmith"))
                        {
                            shop.Value.Items.Add(new ShopItemData()
                            {
                                Price = _config.BuyPrice,
                                ItemId = Lockpick.ItemId,
                                IgnoreShopPriceModifiers = true
                            });
                            Monitor.Log($"Added lock pick to shop: {shop.Key}");
                            /*if (canBuyFrom.Contains(shop.Key))
                            {
                                shop.Value.Items.Add(new ShopItemData()
                                {
                                    Price = _config.BuyPrice,
                                    ItemId = Lockpick.ItemId,
                                    IgnoreShopPriceModifiers = true
                                });
                                Monitor.Log($"Added lock pick to shop: {shop.Key}");
                            }*/
                        }
                    }
                });
            }
        }
        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _cfg = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (_cfg is null) return;
            
            _cfg.Register(
                mod: ModManifest,
                reset: () => _config = new ModConfig(),
                save: () => Helper.WriteConfig(_config)
                );
            
            _cfg.AddSectionTitle(
                mod: ModManifest,
                text: () => "Lockpick Settings",
                tooltip: null
                );
            _cfg.AddNumberOption(
                mod: ModManifest,
                name: I18n.Price,
                tooltip: I18n.PriceDescription,
                getValue: () => _config.Price,
                setValue: value => _config.Price = value
                );
            _cfg.AddNumberOption(
                mod: ModManifest,
                name: I18n.BuyPrice,
                tooltip: I18n.BuyPriceDescription,
                getValue: () => _config.BuyPrice,
                setValue: value => _config.BuyPrice = value
            );
            /*
            _cfg.AddSectionTitle(
                mod: ModManifest,
                text: I18n.CanBuyFrom,
                tooltip:null
                );
            _cfg.AddParagraph(
                mod: ModManifest,
                text: () => _config.CanBuyFrom
                );*/
        }
        private void OnInput(ButtonPressedEventArgs e)
        {
            if (e.IsSuppressed() || (!e.Button.IsActionButton() && !e.Button.IsUseToolButton()) || Game1.eventUp || !Context.IsPlayerFree) return;
            Vector2 vector = Utils.GetTargetedTile();
            string v = Utils.GetAction(Game1.currentLocation, vector);
            if(v != null && OnTileAction(vector, v)) { Helper.Input.Suppress(e.Button); }
            else
            {
                vector = Utility.getTranslatedVector2(vector, Game1.player.FacingDirection, 0f);
                vector.Y += 1;
                v = Utils.GetAction(Game1.currentLocation, vector);
                if(v != null && OnTileAction(vector, v)) { Helper.Input.Suppress(e.Button); }
                else
                {
                    vector = Game1.player.Tile;
                    v = Utils.GetAction(Game1.currentLocation, vector);
                    if (v != null && OnTileAction(vector, v)) { Helper.Input.Suppress(e.Button); }
                }
            }
        }

        #region "Custom Methods"

        private void SetUpRooms()
        {
            // add darkroom
            if (Game1.getLocationFromName("Darkroom") == null)
            {
                //Helper.GameContent.Load<Map>("Maps\\Darkroom");
                Game1.content.Load<xTile.Map>("Maps\\Darkroom");
                Game1.locations.Add(new GameLocation("Maps\\Darkroom", "Darkroom"));
                var darkroom = Game1.getLocationFromName("Darkroom");
                darkroom.resetForPlayerEntry();
                darkroom.warps.Add(new Warp(3, 8, "HaleyHouse", 4, 4, false));
            }
            //add marniebarn
            if (Game1.getLocationFromName("MarnieBarn") == null)
            {
                Game1.content.Load<xTile.Map>("Maps\\MarnieBarn");
                Game1.locations.Add(new GameLocation("Maps\\MarnieBarn", "MarnieBarn"));
                var marniebarn = Game1.getLocationFromName("MarnieBarn");
                marniebarn.resetForPlayerEntry();
                //this map has a bugged warp in it that needs to be replaced
                var buggedwarp = marniebarn.warps.First();
                buggedwarp.TargetName = "Forest";
                buggedwarp.TargetX = 97;
                buggedwarp.TargetY = 16;
                //and lets add a warp from the yard back inside
                var forest = Game1.getLocationFromName("Forest") as Forest;
                forest?.warps.Add(new StardewValley.Warp(96, 15, "MarnieBarn", 11, 13, false));
                forest?.warps.Add(new StardewValley.Warp(97, 15, "MarnieBarn", 11, 13, false));
                forest?.warps.Add(new StardewValley.Warp(98, 15, "MarnieBarn", 11, 13, false));
                forest?.warps.Add(new StardewValley.Warp(99, 15, "MarnieBarn", 11, 13, false));
                marniebarn.warps.Add(new Warp(3, 9, "AnimalShop", 30, 14, false));
            }
        }
        private bool OnTileAction(Vector2 vector, string action)
        {
            Monitor.Log(action, LogLevel.Alert);
            var parameters = action.Split(' ');
            if (parameters.Length < 1) return false;
            bool lockFound = false;
            switch (parameters[0])
            {
                case "LockedDoorWarp":
                    if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.season) && Utility.getStartTimeOfFestival() < 1900)
                    {
                        lockFound = true;
                        break;
                    }
                    if (parameters[3] == "WizardHouse" && !Utils.IsWizardHouseUnlocked()) {
                        lockFound = true;
                        break;
                    }
                    if(parameters[3] == "SeedShop" && Game1.Date.DayOfWeek == DayOfWeek.Wednesday && !Game1.MasterPlayer.eventsSeen.Contains("191393"))
                    {
                        lockFound = true;
                        break;
                    }
                    if (Game1.timeOfDay < int.Parse(parameters[4]) || Game1.timeOfDay >= int.Parse(parameters[5])) lockFound = true;
                    else if(parameters.Length > 6 && Game1.player.getFriendshipLevelForNPC(parameters[6]) < int.Parse(parameters[7])) lockFound = true;
                    break;
                case "Door":
                    if (parameters.Length > 1 && Game1.player.getFriendshipLevelForNPC(parameters[1]) < 500) lockFound = true;
                    break;
                case "SkullDoor":
                    if (!Game1.MasterPlayer.hasUnlockedSkullDoor && !Game1.MasterPlayer.hasSkullKey) lockFound = true;
                    break;
                case "WarpCommunityCenter":
                    if (!(Game1.MasterPlayer.mailReceived.Contains("ccDoorUnlock") || Game1.MasterPlayer.mailReceived.Contains("JojaMember"))) lockFound = true;
                    break;
                case "WarpWomensLocker":
                    if (Game1.player.IsMale) lockFound = true;
                    break;
                case "WarpMensLocker":
                    if (!Game1.player.IsMale) lockFound = true;
                    break;
                case "WizardHatch":
                    if ((!Game1.player.friendshipData.ContainsKey("Wizard") || Game1.player.friendshipData["Wizard"].Points < 1000)) lockFound = true;
                    break;
                case "SewerGrate":
                case "EnterSewer":
                    if (!Game1.MasterPlayer.hasRustyKey && !Game1.MasterPlayer.mailReceived.Contains("OpenedSewer")) lockFound = true;
                    break;
                case "Warp_Sunroom_Door":
                    if (Game1.player.getFriendshipHeartLevelForNPC("Caroline") < 2) lockFound = true;
                    break;
                case "Theater_Entrance":
                    if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater")) break;
                    if (Game1.player.team.movieMutex.IsLocked() || Game1.isFestival() || Game1.timeOfDay > 2100 || Game1.timeOfDay < 900) lockFound = true;
                    else if (!Game1.player.Items.ContainsId("809")) lockFound = true;
                    else if (Game1.player.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks) lockFound = true;
                    break;
                case "Message":
                    if (parameters.Length < 2) break;
                    if (parameters[1] == "\"HaleyHouse.1\"") lockFound = true;
                    else if (parameters[1] == "\"AnimalShop.17\"") lockFound = true;
                    break;
            }

            bool cached = LockCache.Contains(GenerateCacheKey(parameters[0], Game1.currentLocation, vector.X, vector.Y));
            if (!cached && !Game1.player.Items.ContainsId(Lockpick.ItemId)) return false; //we're done here

            if (lockFound)
            {
                if (cached)
                {
                    OpenLock(action.Split(' '), (int)vector.X, (int)vector.Y);
                }
                else
                {
                    string key = string.Join("^", new[] { "l", ((int)vector.X).ToString(), ((int)vector.Y).ToString(), action });
                    Game1.currentLocation.lastQuestionKey = "lockpick";
                    Response[] responses = new[] { new Response(key, I18n.Yes()), new Response("No", I18n.No()) };
                    Game1.currentLocation.createQuestionDialogue(I18n.Use(), responses, (f, a) => {
                        if (a == "No" || !a.StartsWith("l^")) return;
                        var p = a.Substring(2).Split('^');
                        if (p.Length != 3) return;
                        var rng = RNG.Next(25);
                        Monitor.Log($"RNG Was : {rng} of 25");
                        if (rng == 0) //chance of breaking
                        {
                            Game1.player.removeFirstOfThisItemFromInventory(Lockpick.ItemId);
                            Game1.showRedMessage(I18n.Broke());
                            Game1.playSound("clank");
                        }
                        else OpenLock(p[2].Split(' '), int.Parse(p[0]), int.Parse(p[1]), true);
                    });
                }
                return true;
            }
            return false;
        }

        private void OpenLock(string[] Lock, int tileX, int tileY, bool picked = false)
        {
            if (picked)
            {
                Game1.playSound("stoneCrack"); Game1.playSound("axchop");
                string key = GenerateCacheKey(Lock[0], Game1.currentLocation, tileX, tileY);
                LockCache.Add(key);
                if (Game1.IsMultiplayer) Helper.Multiplayer.SendMessage<string>(key, "lockpickEvent");
            }
            switch (Lock[0])
            {
                case "LockedDoorWarp":
                    Warp(picked, "doorClose", Lock[3], int.Parse(Lock[1]), int.Parse(Lock[2]));
                    break;
                case "Door":
                    Game1.currentLocation.openDoor(new xTile.Dimensions.Location(tileX, tileY), playSound: !picked);
                    Game1.currentLocation.map.GetLayer("Back").Tiles[tileX, tileY].Properties.Remove("TouchAction");
                    break;
                case "SkullDoor":
                    Game1.showRedMessage(Helper.Translation.Get("complex"));
                    break;
                case "WarpCommunityCenter":
                    Warp(picked, "doorClose", "CommunityCenter", 32, 23);
                    break;
                case "WarpWomensLocker":
                case "WarpMensLocker":
                    Warp(picked || Lock.Length >= 5, "doorClose", Lock[3], Convert.ToInt32(Lock[1]), Convert.ToInt32(Lock[2]));
                    break;
                case "WizardHatch":
                    Warp(picked, "doorClose", "WizardHouseBasement", 4, 4);
                    break;
                case "SewerGrate":
                    Warp(picked, "openChest", "Sewer", 3, 48);
                    break;
                case "EnterSewer":
                    Warp(picked, "stairsdown", "Sewer", 16, 11);
                    break;
                case "Warp_Sunroom_Door":
                    Warp(picked, "doorClose", "Sunroom", 5, 13);
                    break;
                case "Theater_Entrance":
                    Warp(picked, "doorClose", "MovieTheater", 13, 15);
                    break;
                case "Message":
                    if (Lock[1] == "\"HaleyHouse.1\"")
                    {
                        if (!picked) Game1.playSound("doorClose");
                        Warp(picked, "doorClose", "Darkroom", 192 / Game1.tileSize, 384 / Game1.tileSize);
                    }
                    else if (Lock[1] == "\"AnimalShop.17\"")
                    {
                        if (!picked) Game1.playSound("doorClose");
                        Warp(picked, "doorClose", "MarnieBarn", 192 / Game1.tileSize, 448 / Game1.tileSize);
                    }
                    break;
            }
        }

        private void OnMultiplayerPacket(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != Helper.Multiplayer.ModID) return;
            if (!Context.IsWorldReady) return; //don't pick locks on the title screen
            string key = e.ReadAs<string>();
            if (!LockCache.Contains(key))
            {
                string[] split = key.Split('^');
                string lockType = split[0];
                string location = split[1];
                int x = int.Parse(split[2]);
                int y = int.Parse(split[3]);
                if(location == Game1.currentLocation.NameOrUniqueName) Game1.playSound("stoneCrack"); Game1.playSound("axchop");
                if (lockType != "SkullDoor") LockCache.Add(key);
                if (lockType == "Door")
                {
                    Game1.getLocationFromName(location).map.GetLayer("Back").Tiles[x, y].Properties.Remove("TouchAction");
                }
            }
        }

        private void Warp(bool picked, string sound, string destination, int x, int y)
        {
            if (!picked) Game1.playSound(sound);
            Utils.WarpFarmer(destination, x, y);
        }
        
        #endregion
    }
}