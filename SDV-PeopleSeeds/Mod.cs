using System;
using System.Collections.Generic;
using System.Diagnostics;
using bwdyworks;
using bwdyworks.API;
using bwdyworks.Events;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace PeopleSeeds;

public class Mod : Mod
{
	internal static bool Debug;

	internal static string Module;

	internal static Mod Instance;

	[Conditional("DEBUG")]
	public void EntryDebug()
	{
		Mod.Debug = true;
	}

	public override void Entry(IModHelper helper)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		Mod.Instance = this;
		Mod.Module = ((IModLinked)helper.ModRegistry).ModID;
		if (Modworks.InstallModule(Mod.Module, Mod.Debug))
		{
			((Mod)this).Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
			Modworks.Events.NPCCheckAction += new NPCCheckActionHandler(Events_NPCCheckAction);
			Modworks.Items.AddItem(Mod.Module, new BasicItemEntry((Mod)(object)this, "wandred", 30, -300, "Basic", -20, "Wand of Transfiguration", "Convert between humans and §¤εØγ."));
			Modworks.Items.AddItem(Mod.Module, new BasicItemEntry((Mod)(object)this, "MysteryMeat", 8, 17, "Basic", -20, "Mystery Meat", "It was an accident."));
			People.OnStartup();
			Modworks.Events.ItemEaten += new ItemEatenHandler(Events_ItemEaten);
			((Mod)this).Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			Modworks.Events.TileCheckAction += new TileCheckActionHandler(Events_TileCheckAction);
			((Mod)this).Helper.Events.Player.Warped += Player_Warped;
			((Mod)this).Helper.Events.GameLoop.Saving += GameLoop_Saving;
			((Mod)this).Helper.Events.GameLoop.Saved += GameLoop_Saved;
		}
	}

	private void Player_Warped(object sender, WarpedEventArgs e)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (e.NewLocation is DecoratableLocation)
		{
			List<Furniture> list = new List<Furniture>();
			Enumerator<Furniture> enumerator = (e.NewLocation as DecoratableLocation).furniture.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Furniture current = enumerator.Current;
					if (current is PeopleFurniture && (current as PeopleFurniture).living)
					{
						PeopleFurniture peopleFurniture = current as PeopleFurniture;
						NPC nPC = People.ClonePerson(peopleFurniture.personId);
						e.NewLocation.characters.Add(nPC);
						nPC.currentLocation = e.NewLocation;
						nPC.setTileLocation(peopleFurniture.TileLocation);
						nPC.DefaultPosition = peopleFurniture.TileLocation;
						nPC.DefaultMap = e.NewLocation.Name;
						nPC.DefaultFacingDirection = Modworks.RNG.Next(4);
						nPC.FacingDirection = Modworks.RNG.Next(4);
						nPC.faceDirection(Modworks.RNG.Next(4));
						list.Add(current);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
			}
			foreach (Furniture item in list)
			{
				(e.NewLocation as DecoratableLocation).furniture.Remove(item);
			}
		}
		if (!(e.OldLocation is DecoratableLocation))
		{
			return;
		}
		int count = e.OldLocation.characters.Count;
		for (int i = 0; i < count; i++)
		{
			if (e.OldLocation.characters[0].id >= 100)
			{
				Vector2 tileLocation = ((Character)e.OldLocation.characters[0]).getTileLocation();
				PeopleFurniture peopleFurniture2 = Mod.Petrify(e.OldLocation.characters[0]);
				peopleFurniture2.TileLocation = tileLocation;
				peopleFurniture2.living = true;
				(e.OldLocation as DecoratableLocation).furniture.Add(peopleFurniture2);
			}
		}
	}

	private void Events_TileCheckAction(object sender, TileCheckActionEventArgs args)
	{
		if (args.Action == "Yoba" && Game1.player.ActiveObject != null && Game1.player.ActiveObject.ParentSheetIndex == 373)
		{
			Game1.playSound("getNewSpecialItem");
			Game1.playSound("yoba");
			StardewValley.Object item = Modworks.Items.CreateItemstack(Modworks.Items.GetModItemId(Mod.Module, "wandred").Value, 1);
			Game1.player.reduceActiveItemByOne();
			Game1.player.holdUpItemThenMessage(item);
			Game1.player.addItemsByMenuIfNecessary(new List<Item> { item }, (ItemGrabMenu.behaviorOnItemSelect)null);
			((CancelableEventArgs)args).Cancelled = true;
		}
		else if (args.Action == "Yoba" && Game1.player.ActiveObject != null && Game1.player.ActiveObject is PeopleFurniture)
		{
			string person = People.GetPerson((Game1.player.ActiveObject as PeopleFurniture).personId);
			if (People.Banished.Contains(person))
			{
				People.Banished.Remove(person);
				Game1.player.reduceActiveItemByOne();
				Game1.playSound("getNewSpecialItem");
				Game1.playSound("yoba");
				Game1.showGlobalMessage(person + " will take their rightful place at dawn.");
			}
			else
			{
				Game1.playSound("yoba");
				Game1.showGlobalMessage(person + " is already in their rightful place.");
			}
			((CancelableEventArgs)args).Cancelled = true;
		}
		else if (args.Action == "Yoba" && Game1.player.ActiveObject != null && Game1.player.ActiveObject.ParentSheetIndex == 74)
		{
			Game1.player.reduceActiveItemByOne();
			Game1.playSound("getNewSpecialItem");
			Game1.playSound("yoba");
			Game1.player.addItemsByMenuIfNecessary(new List<Item>
			{
				new StardewValley.Object(Vector2.Zero, People.GetSeedItemId(People.PersonNames[Modworks.RNG.Next(People.PersonNames.Length)]), 1),
				new StardewValley.Object(Vector2.Zero, People.GetSeedItemId(People.PersonNames[Modworks.RNG.Next(People.PersonNames.Length)]), 1)
			}, (ItemGrabMenu.behaviorOnItemSelect)null);
			((CancelableEventArgs)args).Cancelled = true;
		}
	}

	private void GameLoop_Saved(object sender, SavedEventArgs e)
	{
		foreach (string item in People.Banished)
		{
			Mod.HideNPC(Game1.getCharacterFromName(item, false));
		}
		Mod.UpcastItems();
	}

	private void GameLoop_Saving(object sender, SavingEventArgs e)
	{
		Mod.DowncastItems();
		((Mod)this).Helper.Data.WriteSaveData<HashSet<string>>("bwdy.wtf.PeopleSeeds.Banished", People.Banished);
	}

	private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
	{
		People.OnLoad();
		HashSet<string> hashSet = ((Mod)this).Helper.Data.ReadSaveData<HashSet<string>>("bwdy.wtf.PeopleSeeds.Banished");
		if (hashSet != null)
		{
			People.Banished = hashSet;
		}
		foreach (string item in People.Banished)
		{
			Mod.HideNPC(Game1.getCharacterFromName(item, false));
		}
		Mod.UpcastItems();
	}

	private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0980: Unknown result type (might be due to invalid IL or missing references)
		//IL_098d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b15: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0673: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0742: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0975: Unknown result type (might be due to invalid IL or missing references)
		//IL_095e: Unknown result type (might be due to invalid IL or missing references)
		//IL_087d: Unknown result type (might be due to invalid IL or missing references)
		if (e.IsSuppressed() || !Context.IsPlayerFree)
		{
			return;
		}
		if (SButtonExtensions.IsUseToolButton(e.Button) && Game1.player.ActiveObject is PeopleFurniture && Game1.player.currentLocation.objects.ContainsKey(e.Cursor.GrabTile))
		{
			StardewValley.Object @object = Game1.player.currentLocation.objects[e.Cursor.GrabTile];
			if (@object.Name.Equals("Seed Maker") && @object.heldObject.Value == null)
			{
				Response[] array = new Response[2]
				{
					new Response(e.Cursor.GrabTile.X + "|" + e.Cursor.GrabTile.Y, "Yes"),
					new Response("No", "No")
				};
				Modworks.Menus.AskQuestion("Put " + Game1.player.ActiveObject.Name + " in the Seed Maker?", array, (GameLocation.afterQuestionBehavior)CallbackSeedmaker);
				((Mod)this).Helper.Input.Suppress(e.Button);
			}
			else if (@object.Name.Equals("Furnace") && @object.heldObject.Value == null)
			{
				Response[] array2 = new Response[2]
				{
					new Response(e.Cursor.GrabTile.X + "|" + e.Cursor.GrabTile.Y, "Yes"),
					new Response("No", "No")
				};
				Modworks.Menus.AskQuestion("Put " + Game1.player.ActiveObject.Name + " in the Furnace?", array2, (GameLocation.afterQuestionBehavior)CallbackFurnace);
				((Mod)this).Helper.Input.Suppress(e.Button);
			}
			else if (@object.Name.Equals("Keg") && @object.heldObject.Value == null)
			{
				Response[] array3 = new Response[2]
				{
					new Response(e.Cursor.GrabTile.X + "|" + e.Cursor.GrabTile.Y, "Yes"),
					new Response("No", "No")
				};
				Modworks.Menus.AskQuestion("Put " + Game1.player.ActiveObject.Name + " in the Keg?", array3, (GameLocation.afterQuestionBehavior)CallbackKeg);
				((Mod)this).Helper.Input.Suppress(e.Button);
			}
			else if (@object.Name.Equals("Preserves Jar") && @object.heldObject.Value == null)
			{
				Response[] array4 = new Response[2]
				{
					new Response(e.Cursor.GrabTile.X + "|" + e.Cursor.GrabTile.Y, "Yes"),
					new Response("No", "No")
				};
				Modworks.Menus.AskQuestion("Put " + Game1.player.ActiveObject.Name + " in the Preserves Jar?", array4, (GameLocation.afterQuestionBehavior)CallbackPreservesJar);
				((Mod)this).Helper.Input.Suppress(e.Button);
			}
			return;
		}
		if (SButtonExtensions.IsActionButton(e.Button))
		{
			if (Game1.player.ActiveObject is PeopleFurniture)
			{
				if (Game1.player.currentLocation.objects.ContainsKey(e.Cursor.GrabTile))
				{
					StardewValley.Object object2 = Game1.player.currentLocation.objects[e.Cursor.GrabTile];
					if (object2.Name.Equals("Seed Maker") && object2.heldObject.Value == null)
					{
						Response[] array5 = new Response[2]
						{
							new Response(e.Cursor.GrabTile.X + "|" + e.Cursor.GrabTile.Y, "Yes"),
							new Response("No", "No")
						};
						Modworks.Menus.AskQuestion("Put " + Game1.player.ActiveObject.Name + " in the Seed Maker?", array5, (GameLocation.afterQuestionBehavior)CallbackSeedmaker);
						((Mod)this).Helper.Input.Suppress(e.Button);
					}
					else if (object2.Name.Equals("Furnace") && object2.heldObject.Value == null)
					{
						Response[] array6 = new Response[2]
						{
							new Response(e.Cursor.GrabTile.X + "|" + e.Cursor.GrabTile.Y, "Yes"),
							new Response("No", "No")
						};
						Modworks.Menus.AskQuestion("Put " + Game1.player.ActiveObject.Name + " in the Furnace?", array6, (GameLocation.afterQuestionBehavior)CallbackFurnace);
						((Mod)this).Helper.Input.Suppress(e.Button);
					}
					else if (object2.Name.Equals("Keg") && object2.heldObject.Value == null)
					{
						Response[] array7 = new Response[2]
						{
							new Response(e.Cursor.GrabTile.X + "|" + e.Cursor.GrabTile.Y, "Yes"),
							new Response("No", "No")
						};
						Modworks.Menus.AskQuestion("Put " + Game1.player.ActiveObject.Name + " in the Keg?", array7, (GameLocation.afterQuestionBehavior)CallbackKeg);
						((Mod)this).Helper.Input.Suppress(e.Button);
					}
					else if (object2.Name.Equals("Preserves Jar") && object2.heldObject.Value == null)
					{
						Response[] array8 = new Response[2]
						{
							new Response(e.Cursor.GrabTile.X + "|" + e.Cursor.GrabTile.Y, "Yes"),
							new Response("No", "No")
						};
						Modworks.Menus.AskQuestion("Put " + Game1.player.ActiveObject.Name + " in the Preserves Jar?", array8, (GameLocation.afterQuestionBehavior)CallbackPreservesJar);
						((Mod)this).Helper.Input.Suppress(e.Button);
					}
				}
				else
				{
					Modworks.Player.ForceOfferEatInedibleHeldItem();
					((Mod)this).Helper.Input.Suppress(e.Button);
				}
				return;
			}
			if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.Name == "Wand of Transfiguration" && Game1.player.currentLocation is DecoratableLocation)
			{
				DecoratableLocation decoratableLocation = Game1.player.currentLocation as DecoratableLocation;
				Furniture furniture = null;
				Enumerator<Furniture> enumerator = decoratableLocation.furniture.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Furniture current = enumerator.Current;
						if (current is PeopleFurniture && current.TileLocation == e.Cursor.GrabTile)
						{
							furniture = current;
							break;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
				}
				if (furniture != null)
				{
					decoratableLocation.furniture.Remove(furniture);
					PeopleFurniture peopleFurniture = furniture as PeopleFurniture;
					if (Game1.player.daysUntilHouseUpgrade.Value > 0 && People.GetPerson(peopleFurniture.personId) == "Robin")
					{
						Game1.showRedMessage("Robin still has work to do on your house.");
						((Mod)this).Helper.Input.Suppress(e.Button);
						return;
					}
					Modworks.Log.Trace("transfiguring item: " + People.GetPerson(peopleFurniture.personId) + " to NPC");
					NPC nPC = People.ClonePerson(peopleFurniture.personId);
					Modworks.NPCs.Warp(nPC, decoratableLocation.Name, Utility.Vector2ToPoint(furniture.TileLocation));
					nPC.DefaultPosition = peopleFurniture.TileLocation;
					nPC.DefaultMap = Game1.player.currentLocation.Name;
					nPC.DefaultFacingDirection = Modworks.RNG.Next(4);
					nPC.FacingDirection = Modworks.RNG.Next(4);
					nPC.faceDirection(Modworks.RNG.Next(4));
					Game1.player.currentLocation.playSound("slimeHit");
					Game1.playSound("yoba");
					((Mod)this).Helper.Input.Suppress(e.Button);
					return;
				}
				((Mod)this).Helper.Input.Suppress(e.Button);
			}
		}
		if ((SButtonExtensions.IsActionButton(e.Button) || SButtonExtensions.IsUseToolButton(e.Button)) && Game1.player.ActiveObject != null && People.IsSeed(Game1.player.ActiveObject.ParentSheetIndex))
		{
			Vector2 grabTile = e.Cursor.GrabTile;
			GameLocation currentLocation = Game1.currentLocation;
			if (currentLocation.isTileHoeDirt(grabTile) && !currentLocation.isCropAtTile((int)grabTile.X, (int)grabTile.Y) && currentLocation.terrainFeatures.ContainsKey(grabTile) && currentLocation.terrainFeatures[grabTile] is HoeDirt)
			{
				PeopleHoeDirt value = new PeopleHoeDirt((currentLocation.terrainFeatures[grabTile] as HoeDirt).state.Value, new PeopleCrop(People.GetPersonId(Game1.player.ActiveObject.ParentSheetIndex), (int)grabTile.X, (int)grabTile.Y));
				currentLocation.terrainFeatures[grabTile] = value;
				Game1.player.reduceActiveItemByOne();
				((Mod)this).Helper.Input.Suppress(e.Button);
				return;
			}
		}
		if (SButtonExtensions.IsUseToolButton(e.Button) && Game1.player.ActiveObject is PeopleFurniture && Mod.tryToPlaceItem(Game1.currentLocation, Game1.player.ActiveObject as PeopleFurniture, (int)e.Cursor.GrabTile.X * 64, (int)e.Cursor.GrabTile.Y * 64))
		{
			((Mod)this).Helper.Input.Suppress(e.Button);
		}
		else
		{
			if (Game1.player.ActiveObject == null || !SButtonExtensions.IsActionButton(e.Button))
			{
				return;
			}
			string text = Game1.player.ActiveObject.Name.ToLower();
			if (!text.Contains(" ") || (!text.EndsWith("panties") && !text.EndsWith("underwear")) || !text.Contains("'") || !(Game1.player.currentLocation is DecoratableLocation))
			{
				return;
			}
			DecoratableLocation obj = Game1.player.currentLocation as DecoratableLocation;
			Furniture furniture2 = null;
			Enumerator<Furniture> enumerator = obj.furniture.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Furniture current2 = enumerator.Current;
					if (current2 is PeopleFurniture && current2.TileLocation == e.Cursor.GrabTile)
					{
						furniture2 = current2;
						break;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
			}
			if (furniture2 != null && furniture2.heldObject.Value == null && text.Split('\'')[0] == furniture2.name.ToLower())
			{
				StardewValley.Object activeObject = Game1.player.ActiveObject;
				furniture2.heldObject.Value = new StardewValley.Object(activeObject.ParentSheetIndex, 1, false, activeObject.Price, Math.Min(activeObject.Quality + 1, 5));
				furniture2.MinutesUntilReady = 40;
				Game1.player.currentLocation.playSound("slimeHit");
				DelayedAction.playSoundAfterDelay("Ship", 100, (GameLocation)null);
				DelayedAction.playSoundAfterDelay("dirtyHit", 350, (GameLocation)null);
				this.WtfDialogue((furniture2 as PeopleFurniture).personId);
				Game1.player.reduceActiveItemByOne();
			}
		}
	}

	public static bool tryToPlaceItem(GameLocation location, PeopleFurniture item, int x, int y)
	{
		if (Utility.playerCanPlaceItemHere(location, (Item)item, x, y, Game1.player) && item.CloneOne().placementAction(location, x, y, Game1.player))
		{
			Game1.player.reduceActiveItemByOne();
			return true;
		}
		return false;
	}

	private void CallbackSeedmaker(Farmer who, string key)
	{
		if (!(key != "No"))
		{
			return;
		}
		string[] array = key.Split('|');
		Vector2 key2 = Utility.PointToVector2(new Point((int)float.Parse(array[0]), (int)float.Parse(array[1])));
		if (Game1.player.ActiveObject is PeopleFurniture && Game1.player.currentLocation.objects.ContainsKey(key2))
		{
			StardewValley.Object @object = Game1.player.currentLocation.objects[key2];
			if (@object.Name.Equals("Seed Maker"))
			{
				Modworks.Log.Trace("Making " + Game1.player.ActiveObject.Name + " seeds.");
				@object.heldObject.Value = new StardewValley.Object(Vector2.Zero, People.GetSeedItemId(Game1.player.ActiveObject.Name), Game1.player.ActiveObject.Quality + 1);
				@object.MinutesUntilReady = 20;
				who.currentLocation.playSound("slimeHit");
				DelayedAction.playSoundAfterDelay("Ship", 100, (GameLocation)null);
				DelayedAction.playSoundAfterDelay("dirtyHit", 350, (GameLocation)null);
				this.BadDialogue((Game1.player.ActiveObject as PeopleFurniture).personId);
				Game1.player.reduceActiveItemByOne();
			}
		}
	}

	private void CallbackKeg(Farmer who, string key)
	{
		if (!(key != "No"))
		{
			return;
		}
		string[] array = key.Split('|');
		Vector2 key2 = Utility.PointToVector2(new Point((int)float.Parse(array[0]), (int)float.Parse(array[1])));
		if (Game1.player.ActiveObject is PeopleFurniture && Game1.player.currentLocation.objects.ContainsKey(key2))
		{
			StardewValley.Object @object = Game1.player.currentLocation.objects[key2];
			if (@object.Name.Equals("Keg"))
			{
				StardewValley.Object object2 = new StardewValley.Object(Vector2.Zero, 348, 1);
				object2.name = Game1.player.ActiveObject.Name + " Wine";
				object2.Price = Game1.player.ActiveObject.Price * 2 + 500;
				@object.heldObject.Value = object2;
				@object.heldObject.Value.Quality = Modworks.RNG.Next(Game1.player.ActiveObject.Quality + 1);
				@object.MinutesUntilReady = 20;
				DelayedAction.playSoundAfterDelay("dirtyHit", 350, (GameLocation)null);
				this.BadDialogue((Game1.player.ActiveObject as PeopleFurniture).personId);
				Game1.player.reduceActiveItemByOne();
			}
		}
	}

	private void CallbackPreservesJar(Farmer who, string key)
	{
		if (!(key != "No"))
		{
			return;
		}
		string[] array = key.Split('|');
		Vector2 key2 = Utility.PointToVector2(new Point((int)float.Parse(array[0]), (int)float.Parse(array[1])));
		if (Game1.player.ActiveObject is PeopleFurniture && Game1.player.currentLocation.objects.ContainsKey(key2))
		{
			StardewValley.Object @object = Game1.player.currentLocation.objects[key2];
			if (@object.Name.Equals("Preserves Jar"))
			{
				StardewValley.Object object2 = new StardewValley.Object(Vector2.Zero, 344, 1);
				object2.name = Game1.player.ActiveObject.Name + " Jelly";
				object2.Price = Game1.player.ActiveObject.Price * 2 + 500;
				@object.heldObject.Value = object2;
				@object.heldObject.Value.Quality = Modworks.RNG.Next(Game1.player.ActiveObject.Quality + 1);
				@object.MinutesUntilReady = 20;
				DelayedAction.playSoundAfterDelay("slimeHit", 350, (GameLocation)null);
				this.BadDialogue((Game1.player.ActiveObject as PeopleFurniture).personId);
				Game1.player.reduceActiveItemByOne();
			}
		}
	}

	private void CallbackFurnace(Farmer who, string key)
	{
		if (!(key != "No"))
		{
			return;
		}
		string[] array = key.Split('|');
		Vector2 key2 = Utility.PointToVector2(new Point((int)float.Parse(array[0]), (int)float.Parse(array[1])));
		if (Game1.player.ActiveObject is PeopleFurniture && Game1.player.currentLocation.objects.ContainsKey(key2))
		{
			StardewValley.Object @object = Game1.player.currentLocation.objects[key2];
			if (@object.Name.Equals("Furnace"))
			{
				@object.heldObject.Value = Modworks.Items.CreateItemstack(Modworks.Items.GetModItemId(Mod.Module, "MysteryMeat").Value, 1);
				@object.heldObject.Value.Quality = Game1.player.ActiveObject.Quality;
				@object.MinutesUntilReady = 20;
				DelayedAction.playSoundAfterDelay("dirtyHit", 350, (GameLocation)null);
				this.BadDialogue((Game1.player.ActiveObject as PeopleFurniture).personId);
				Game1.player.reduceActiveItemByOne();
			}
		}
	}

	private void BadDialogue(int personId)
	{
		switch (Modworks.RNG.Next(7))
		{
		case 2:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "WHA-NONONONononono..." + People.GetScaredFace(personId));
			break;
		case 3:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "WHY WOU- [muffled]" + People.GetAngryFace(personId));
			break;
		case 4:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "No..." + People.GetScaredFace(personId));
			break;
		case 5:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "You're..." + People.GetAngryFace(personId));
			break;
		case 6:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "AAAGGHHHH!!!!" + People.GetScaredFace(personId));
			break;
		}
	}

	private void WtfDialogue(int personId)
	{
		switch (Modworks.RNG.Next(5))
		{
		case 0:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "Uwuwuwuwuwu...." + People.GetStrangeFace(personId));
			break;
		case 1:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "Squish" + People.GetStrangeFace(personId));
			break;
		case 2:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "OoOo..." + People.GetStrangeFace(personId));
			break;
		case 3:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "vuvuvuvuvuvuvu" + People.GetStrangeFace(personId));
			break;
		case 4:
			Game1.drawDialogue(People.CloneTemplates[People.GetPerson(personId)], "meeeeeehp" + People.GetStrangeFace(personId));
			break;
		}
	}

	private void Events_ItemEaten(object sender, ItemEatenEventArgs args)
	{
		if (args.Item is PeopleFurniture)
		{
			_ = args.Item.Name;
			this.BadDialogue((args.Item as PeopleFurniture).personId);
		}
	}

	private void Events_NPCCheckAction(object sender, NPCCheckActionEventArgs args)
	{
		if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.DisplayName == "Wand of Transfiguration")
		{
			Game1.player.addItemToInventory(Mod.Petrify(args.NPC));
			((CancelableEventArgs)args).Cancelled = true;
		}
	}

	private static PeopleFurniture Petrify(NPC npc)
	{
		if (People.GetPersonId(npc.Name) >= 0)
		{
			Game1.playSound("cowboy_monsterhit");
			Game1.playSound("woodWhack");
			Modworks.Log.Trace("transfiguring NPC: " + npc.Name + " to item");
			if (npc.id < 100)
			{
				People.Banished.Add(npc.Name);
			}
			Mod.HideNPC(npc);
			return new PeopleFurniture(People.GetPersonId(npc), ((Character)npc).getTileLocation());
		}
		return null;
	}

	private static void HideNPC(NPC npc)
	{
		GameLocation locationFromName = Game1.getLocationFromName("BugLand");
		npc.currentLocation.characters.Remove(npc);
		npc.currentLocation = null;
		locationFromName.characters.Add(npc);
		npc.currentLocation = locationFromName;
		npc.setTileLocation(new Vector2(-1f, -1f));
	}

	public static PeopleFurniture UpcastItem(Furniture f)
	{
		string[] array = f.name.Split(' ');
		PeopleFurniture peopleFurniture = new PeopleFurniture(int.Parse(array[array.Length - 1]), f.TileLocation)
		{
			Stack = f.Stack,
			Price = f.Price,
			Quality = ((StardewValley.Object)f).Quality,
			living = f.name.Contains("|")
		};
		peopleFurniture.Stack = int.Parse(f.name.Split(' ')[1]);
		peopleFurniture.TileLocation = f.TileLocation;
		peopleFurniture.updateDrawPosition();
		return peopleFurniture;
	}

	public static StardewValley.Object DowncastItem(PeopleFurniture pf)
	{
		Furniture furniture = new Furniture(0, pf.TileLocation)
		{
			Stack = pf.Stack,
			Price = pf.Price,
			Quality = ((StardewValley.Object)pf).Quality
		};
		furniture.TileLocation = pf.TileLocation;
		string displayName = (furniture.name = "§¤εØγ" + (pf.living ? "|" : "") + " " + pf.Stack + " " + pf.personId);
		((Item)furniture).DisplayName = displayName;
		return furniture;
	}

	public static void UpcastItems()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		for (int num = Game1.player.Items.Count - 1; num >= 0; num--)
		{
			if (Game1.player.Items[num] is Furniture && Game1.player.Items[num].Name.StartsWith("§¤εØγ"))
			{
				Game1.player.Items[num] = Mod.UpcastItem(Game1.player.Items[num] as Furniture);
			}
		}
		foreach (GameLocation location in Game1.locations)
		{
			PairsCollection pairs = location.objects.Pairs;
			Enumerator enumerator2 = ((PairsCollection)(ref pairs)).GetEnumerator();
			try
			{
				while (((Enumerator)(ref enumerator2)).MoveNext())
				{
					KeyValuePair<Vector2, StardewValley.Object> current2 = ((Enumerator)(ref enumerator2)).Current;
					if (current2.Value == null)
					{
						continue;
					}
					if (current2.Value is Furniture && current2.Value.Name.StartsWith("§¤εØγ"))
					{
						location.objects[current2.Key] = Mod.UpcastItem(current2.Value as Furniture);
					}
					if (current2.Value is Chest)
					{
						Chest chest = current2.Value as Chest;
						for (int i = 0; i < chest.items.Count; i++)
						{
							if (chest.items[i] is Furniture && chest.items[i].Name.StartsWith("§¤εØγ"))
							{
								chest.items[i] = Mod.UpcastItem(chest.items[i] as Furniture);
							}
						}
					}
					if (current2.Value.heldObject.Value != null && current2.Value.heldObject.Value is Furniture && current2.Value.heldObject.Value.Name.StartsWith("§¤εØγ"))
					{
						current2.Value.heldObject.Value = Mod.UpcastItem(current2.Value.heldObject.Value as Furniture);
					}
				}
			}
			finally
			{
				((IDisposable)(Enumerator)(ref enumerator2)/*cast due to .constrained prefix*/).Dispose();
			}
			if (location is Farm)
			{
				Enumerator<Building> enumerator3 = ((BuildableGameLocation)(location as Farm)).buildings.GetEnumerator();
				try
				{
					while (enumerator3.MoveNext())
					{
						Building current3 = enumerator3.Current;
						if (current3.indoors.Value == null)
						{
							continue;
						}
						pairs = current3.indoors.Value.objects.Pairs;
						enumerator2 = ((PairsCollection)(ref pairs)).GetEnumerator();
						try
						{
							while (((Enumerator)(ref enumerator2)).MoveNext())
							{
								KeyValuePair<Vector2, StardewValley.Object> current4 = ((Enumerator)(ref enumerator2)).Current;
								if (current4.Value == null)
								{
									continue;
								}
								if (current4.Value is Furniture && current4.Value.Name.StartsWith("§¤εØγ"))
								{
									current3.indoors.Value.objects[current4.Key] = Mod.UpcastItem(current4.Value as Furniture);
								}
								if (!(current4.Value is Chest))
								{
									continue;
								}
								Chest chest2 = current4.Value as Chest;
								for (int j = 0; j < chest2.items.Count; j++)
								{
									if (chest2.items[j] is Furniture && chest2.items[j].Name.StartsWith("§¤εØγ"))
									{
										chest2.items[j] = Mod.UpcastItem(chest2.items[j] as Furniture);
									}
								}
							}
						}
						finally
						{
							((IDisposable)(Enumerator)(ref enumerator2)/*cast due to .constrained prefix*/).Dispose();
						}
						if (!(current3.indoors.Value is DecoratableLocation))
						{
							continue;
						}
						for (int k = 0; k < (current3.indoors.Value as DecoratableLocation).furniture.Count; k++)
						{
							if ((current3.indoors.Value as DecoratableLocation).furniture[k] != null && (current3.indoors.Value as DecoratableLocation).furniture[k].Name.StartsWith("§¤εØγ"))
							{
								(current3.indoors.Value as DecoratableLocation).furniture[k] = Mod.UpcastItem((current3.indoors.Value as DecoratableLocation).furniture[k]);
							}
						}
					}
				}
				finally
				{
					((IDisposable)enumerator3/*cast due to .constrained prefix*/).Dispose();
				}
			}
			else if (location is FarmHouse)
			{
				for (int l = 0; l < (location as FarmHouse).fridge.Value.items.Count; l++)
				{
					if ((location as FarmHouse).fridge.Value.items[l] is Furniture && (location as FarmHouse).fridge.Value.items[l].Name.StartsWith("§¤εØγ"))
					{
						(location as FarmHouse).fridge.Value.items[l] = Mod.UpcastItem((location as FarmHouse).fridge.Value.items[l] as Furniture);
					}
				}
			}
			if (location is DecoratableLocation)
			{
				for (int m = 0; m < (location as DecoratableLocation).furniture.Count; m++)
				{
					if ((location as DecoratableLocation).furniture[m] != null && (location as DecoratableLocation).furniture[m].Name.StartsWith("§¤εØγ"))
					{
						(location as DecoratableLocation).furniture[m] = Mod.UpcastItem((location as DecoratableLocation).furniture[m]);
					}
				}
				if (location is DecoratableLocation)
				{
					List<Furniture> list = new List<Furniture>();
					Enumerator<Furniture> enumerator4 = (location as DecoratableLocation).furniture.GetEnumerator();
					try
					{
						while (enumerator4.MoveNext())
						{
							Furniture current5 = enumerator4.Current;
							if (current5 is PeopleFurniture && (current5 as PeopleFurniture).living)
							{
								PeopleFurniture peopleFurniture = current5 as PeopleFurniture;
								NPC nPC = People.ClonePerson(peopleFurniture.personId);
								location.characters.Add(nPC);
								nPC.currentLocation = location;
								nPC.setTileLocation(peopleFurniture.TileLocation);
								nPC.DefaultPosition = peopleFurniture.TileLocation;
								nPC.DefaultMap = location.Name;
								nPC.DefaultFacingDirection = Modworks.RNG.Next(4);
								nPC.FacingDirection = Modworks.RNG.Next(4);
								nPC.faceDirection(Modworks.RNG.Next(4));
								list.Add(current5);
							}
						}
					}
					finally
					{
						((IDisposable)enumerator4/*cast due to .constrained prefix*/).Dispose();
					}
					foreach (Furniture item in list)
					{
						(location as DecoratableLocation).furniture.Remove(item);
					}
				}
			}
			List<Vector2> list2 = new List<Vector2>();
			foreach (SerializableDictionary<Vector2, TerrainFeature> terrainFeature in location.terrainFeatures)
			{
				foreach (KeyValuePair<Vector2, TerrainFeature> item2 in terrainFeature)
				{
					if (item2.Value is HoeDirt)
					{
						HoeDirt hoeDirt = item2.Value as HoeDirt;
						if (hoeDirt.crop != null && hoeDirt.crop.whichForageCrop.Value >= 100)
						{
							list2.Add(item2.Key);
						}
					}
				}
			}
			foreach (Vector2 item3 in list2)
			{
				HoeDirt hoeDirt2 = location.terrainFeatures[item3] as HoeDirt;
				location.terrainFeatures[item3] = new PeopleHoeDirt(hoeDirt2.state.Value, new PeopleCrop(hoeDirt2.crop.whichForageCrop.Value - 100, (int)item3.X, (int)item3.Y));
				(location.terrainFeatures[item3] as PeopleHoeDirt).crop.currentPhase.Value = hoeDirt2.crop.currentPhase.Value;
				(location.terrainFeatures[item3] as PeopleHoeDirt).crop.dayOfCurrentPhase.Value = hoeDirt2.crop.dayOfCurrentPhase.Value;
			}
		}
	}

	public static void DowncastItems()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		for (int num = Game1.player.Items.Count - 1; num >= 0; num--)
		{
			if (Game1.player.Items[num] is PeopleFurniture)
			{
				Game1.player.Items[num] = Mod.DowncastItem(Game1.player.Items[num] as PeopleFurniture);
			}
		}
		foreach (GameLocation location in Game1.locations)
		{
			PairsCollection pairs = location.objects.Pairs;
			Enumerator enumerator2 = ((PairsCollection)(ref pairs)).GetEnumerator();
			try
			{
				while (((Enumerator)(ref enumerator2)).MoveNext())
				{
					KeyValuePair<Vector2, StardewValley.Object> current2 = ((Enumerator)(ref enumerator2)).Current;
					if (current2.Value == null)
					{
						continue;
					}
					if (current2.Value is PeopleFurniture)
					{
						location.objects[current2.Key] = Mod.DowncastItem(current2.Value as PeopleFurniture);
					}
					if (current2.Value is Chest)
					{
						Chest chest = current2.Value as Chest;
						for (int i = 0; i < chest.items.Count; i++)
						{
							if (chest.items[i] != null && chest.items[i] is PeopleFurniture)
							{
								chest.items[i] = Mod.DowncastItem(chest.items[i] as PeopleFurniture);
							}
						}
					}
					if (current2.Value.heldObject.Value != null && current2.Value.heldObject.Value is PeopleFurniture)
					{
						current2.Value.heldObject.Value = Mod.DowncastItem(current2.Value.heldObject.Value as PeopleFurniture);
					}
				}
			}
			finally
			{
				((IDisposable)(Enumerator)(ref enumerator2)/*cast due to .constrained prefix*/).Dispose();
			}
			if (location is Farm)
			{
				Enumerator<Building> enumerator3 = ((BuildableGameLocation)(location as Farm)).buildings.GetEnumerator();
				try
				{
					while (enumerator3.MoveNext())
					{
						Building current3 = enumerator3.Current;
						if (current3.indoors.Value == null)
						{
							continue;
						}
						pairs = current3.indoors.Value.objects.Pairs;
						enumerator2 = ((PairsCollection)(ref pairs)).GetEnumerator();
						try
						{
							while (((Enumerator)(ref enumerator2)).MoveNext())
							{
								KeyValuePair<Vector2, StardewValley.Object> current4 = ((Enumerator)(ref enumerator2)).Current;
								if (current4.Value == null)
								{
									continue;
								}
								if (current4.Value is PeopleFurniture)
								{
									current3.indoors.Value.objects[current4.Key] = Mod.DowncastItem(current4.Value as PeopleFurniture);
								}
								if (!(current4.Value is Chest))
								{
									continue;
								}
								Chest chest2 = current4.Value as Chest;
								for (int j = 0; j < chest2.items.Count; j++)
								{
									if (chest2.items[j] != null && chest2.items[j] is PeopleFurniture)
									{
										chest2.items[j] = Mod.DowncastItem(chest2.items[j] as PeopleFurniture);
									}
								}
							}
						}
						finally
						{
							((IDisposable)(Enumerator)(ref enumerator2)/*cast due to .constrained prefix*/).Dispose();
						}
						if (!(current3.indoors.Value is DecoratableLocation))
						{
							continue;
						}
						for (int k = 0; k < (current3.indoors.Value as DecoratableLocation).furniture.Count; k++)
						{
							if ((current3.indoors.Value as DecoratableLocation).furniture[k] is PeopleFurniture)
							{
								(current3.indoors.Value as DecoratableLocation).furniture[k] = (Furniture)Mod.DowncastItem((current3.indoors.Value as DecoratableLocation).furniture[k] as PeopleFurniture);
							}
							Dictionary<Vector2, NPC> dictionary = new Dictionary<Vector2, NPC>();
							Enumerator<NPC> enumerator4 = current3.indoors.Value.characters.GetEnumerator();
							try
							{
								while (enumerator4.MoveNext())
								{
									NPC current5 = enumerator4.Current;
									if (current5.id >= 100)
									{
										dictionary[current5.Position] = current5;
									}
								}
							}
							finally
							{
								((IDisposable)enumerator4/*cast due to .constrained prefix*/).Dispose();
							}
							foreach (KeyValuePair<Vector2, NPC> item in dictionary)
							{
								PeopleFurniture peopleFurniture = Mod.Petrify(item.Value);
								peopleFurniture.living = true;
								Furniture furniture = Mod.DowncastItem(peopleFurniture) as Furniture;
								furniture.TileLocation = item.Key;
								(current3.indoors.Value as DecoratableLocation).furniture.Add(furniture);
							}
						}
					}
				}
				finally
				{
					((IDisposable)enumerator3/*cast due to .constrained prefix*/).Dispose();
				}
			}
			else if (location is FarmHouse)
			{
				for (int l = 0; l < (location as FarmHouse).fridge.Value.items.Count; l++)
				{
					if ((location as FarmHouse).fridge.Value.items[l] is PeopleFurniture)
					{
						(location as FarmHouse).fridge.Value.items[l] = Mod.DowncastItem((location as FarmHouse).fridge.Value.items[l] as PeopleFurniture);
					}
				}
			}
			if (location is DecoratableLocation)
			{
				for (int m = 0; m < (location as DecoratableLocation).furniture.Count; m++)
				{
					if ((location as DecoratableLocation).furniture[m] is PeopleFurniture)
					{
						(location as DecoratableLocation).furniture[m] = (Furniture)Mod.DowncastItem((location as DecoratableLocation).furniture[m] as PeopleFurniture);
					}
				}
				Dictionary<Vector2, NPC> dictionary2 = new Dictionary<Vector2, NPC>();
				Enumerator<NPC> enumerator4 = location.characters.GetEnumerator();
				try
				{
					while (enumerator4.MoveNext())
					{
						NPC current7 = enumerator4.Current;
						if (current7.id >= 100)
						{
							dictionary2[current7.Position] = current7;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator4/*cast due to .constrained prefix*/).Dispose();
				}
				foreach (KeyValuePair<Vector2, NPC> item2 in dictionary2)
				{
					PeopleFurniture peopleFurniture2 = Mod.Petrify(item2.Value);
					peopleFurniture2.living = true;
					Furniture furniture2 = Mod.DowncastItem(peopleFurniture2) as Furniture;
					furniture2.TileLocation = item2.Key;
					(location as DecoratableLocation).furniture.Add(furniture2);
				}
			}
			List<Vector2> list = new List<Vector2>();
			foreach (SerializableDictionary<Vector2, TerrainFeature> terrainFeature in location.terrainFeatures)
			{
				foreach (KeyValuePair<Vector2, TerrainFeature> item3 in terrainFeature)
				{
					if (item3.Value is PeopleHoeDirt)
					{
						list.Add(item3.Key);
					}
				}
			}
			foreach (Vector2 item4 in list)
			{
				if ((location.terrainFeatures[item4] as PeopleHoeDirt).crop == null)
				{
					location.terrainFeatures[item4] = new HoeDirt((location.terrainFeatures[item4] as PeopleHoeDirt).state.Value);
					continue;
				}
				PeopleHoeDirt peopleHoeDirt = location.terrainFeatures[item4] as PeopleHoeDirt;
				location.terrainFeatures[item4] = new HoeDirt(peopleHoeDirt.state.Value, new Crop(0, (int)item4.X, (int)item4.Y));
				(location.terrainFeatures[item4] as HoeDirt).crop.whichForageCrop.Value = 100 + (peopleHoeDirt.crop as PeopleCrop).personId;
				(location.terrainFeatures[item4] as HoeDirt).crop.dayOfCurrentPhase.Value = (peopleHoeDirt.crop as PeopleCrop).dayOfCurrentPhase.Value;
				(location.terrainFeatures[item4] as HoeDirt).crop.currentPhase.Value = (peopleHoeDirt.crop as PeopleCrop).currentPhase.Value;
			}
		}
	}
}
