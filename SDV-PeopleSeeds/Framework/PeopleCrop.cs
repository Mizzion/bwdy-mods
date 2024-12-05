using System;
using System.IO;
using bwdyworks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;

namespace PeopleSeeds;

internal class PeopleCrop : Crop
{
	internal int personId;

	internal Texture2D Sprite;

	public PeopleCrop(int personId, int tileX, int tileY)
		: base(0, tileX, tileY)
	{
		this.personId = personId;
		base.phaseDays.Add(1);
		base.phaseDays.Add(2);
		base.phaseDays.Add(3);
		base.phaseDays.Add(4);
		base.phaseDays.Add(5);
		base.phaseDays.Add(99999);
		base.seasonsToGrowIn.Add("spring");
		base.seasonsToGrowIn.Add("summer");
		base.seasonsToGrowIn.Add("fall");
		base.indexOfHarvest.Value = 0;
		base.regrowAfterHarvest.Value = -1;
		base.harvestMethod.Value = 0;
		base.minHarvest.Value = 1;
		base.maxHarvest.Value = 1;
		base.maxHarvestIncreasePerFarmingLevel.Value = 0;
		base.raisedSeeds.Value = false;
		base.flip.Value = Game1.random.NextDouble() < 0.5;
		base.netSeedIndex.Value = People.GetSeedItemId(personId);
		this.Sprite = ((Mod)Mod.Instance).Helper.Content.Load<Texture2D>(Path.Combine("Assets", People.GetPerson(personId) + "Crop.png"), (ContentSource)1);
	}

	public bool harvest(int xTile, int yTile, PeopleHoeDirt soil, JunimoHarvester junimoHarvester = null)
	{
		if (base.dead.Value)
		{
			return false;
		}
		if (junimoHarvester != null)
		{
			return true;
		}
		if (base.currentPhase.Value >= base.phaseDays.Count - 1 && (!base.fullyGrown.Value || base.dayOfCurrentPhase.Value <= 0))
		{
			PeopleFurniture peopleFurniture = new PeopleFurniture(this.personId, Vector2.Zero);
			((StardewValley.Object)peopleFurniture).Quality = 0;
			if (Modworks.RNG.Next(100) < 50)
			{
				((StardewValley.Object)peopleFurniture).Quality = ((StardewValley.Object)peopleFurniture).Quality + 1;
				if (Modworks.RNG.Next(100) < 50)
				{
					((StardewValley.Object)peopleFurniture).Quality = ((StardewValley.Object)peopleFurniture).Quality + 1;
					if (Modworks.RNG.Next(100) < 50)
					{
						((StardewValley.Object)peopleFurniture).Quality = ((StardewValley.Object)peopleFurniture).Quality + 1;
						if (Modworks.RNG.Next(100) < 50)
						{
							((StardewValley.Object)peopleFurniture).Quality = ((StardewValley.Object)peopleFurniture).Quality + 1;
						}
					}
				}
			}
			if (Game1.player.couldInventoryAcceptThisItem(peopleFurniture))
			{
				Game1.player.addItemToInventory(peopleFurniture);
				Vector2 key = new Vector2(xTile, yTile);
				int value = soil.state.Value;
				if (Game1.currentLocation.terrainFeatures.ContainsKey(key))
				{
					Game1.currentLocation.terrainFeatures.Remove(key);
					Game1.currentLocation.terrainFeatures[key] = new HoeDirt(value);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private new Rectangle getSourceRect(int number)
	{
		if (base.dead.Value)
		{
			return new Rectangle(192 + number % 4 * 16, 384, 16, 32);
		}
		return new Rectangle(Math.Min(240, ((!base.fullyGrown.Value) ? (((base.phaseToShow.Value != -1) ? base.phaseToShow.Value : base.currentPhase.Value) + ((((base.phaseToShow.Value != -1) ? base.phaseToShow.Value : base.currentPhase.Value) == 0 && number % 2 == 0) ? (-1) : 0) + 1) : ((base.dayOfCurrentPhase.Value <= 0) ? 6 : 7)) * 16), 0, 16, 32);
	}

	public new void draw(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
	{
		b.Draw(this.Sprite, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((base.currentPhase.Value >= base.phaseDays.Count - 1) ? 0f : ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f)) + 32f, tileLocation.Y * 64f + ((base.currentPhase.Value >= base.phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) + 32f)), this.getSourceRect((int)tileLocation.X * 7 + (int)tileLocation.Y * 11), toTint, rotation, new Vector2(8f, 24f), 4f, base.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y * 64f + 32f + ((base.currentPhase.Value >= base.phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f))) / 10000f / ((base.currentPhase.Value == 0) ? 2f : 1f));
	}

	public new void drawInMenu(SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth)
	{
		b.Draw(this.Sprite, screenPosition, this.getSourceRect(0), toTint, rotation, new Vector2(32f, 96f), scale, base.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
	}

	public new void drawWithOffset(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, Vector2 offset)
	{
		b.Draw(this.Sprite, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), this.getSourceRect((int)tileLocation.X * 7 + (int)tileLocation.Y * 11), toTint, rotation, new Vector2(8f, 24f), 4f, base.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
	}
}
