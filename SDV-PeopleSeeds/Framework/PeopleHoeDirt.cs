using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace PeopleSeeds;

internal class PeopleHoeDirt : HoeDirt
{
	public PeopleHoeDirt(int state, Crop crop)
		: base(state, crop)
	{
	}

	public virtual bool performUseAction(Vector2 tileLocation, GameLocation location)
	{
		if (!(base.crop is PeopleCrop))
		{
			return true;
		}
		if (base.crop is PeopleCrop peopleCrop)
		{
			bool result = peopleCrop.currentPhase.Value >= peopleCrop.phaseDays.Count - 1 && (!peopleCrop.fullyGrown.Value || peopleCrop.dayOfCurrentPhase.Value <= 0);
			if (((Crop)peopleCrop).harvestMethod.Value == 0 && peopleCrop.harvest((int)tileLocation.X, (int)tileLocation.Y, this))
			{
				base.destroyCrop(tileLocation, false, location);
				return true;
			}
			if (((Crop)peopleCrop).harvestMethod.Value == 1 && base.readyForHarvest())
			{
				if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon && (Game1.player.CurrentTool as MeleeWeapon).InitialParentTileIndex == 47)
				{
					Game1.player.CanMove = false;
					Game1.player.UsingTool = true;
					Game1.player.canReleaseTool = true;
					Game1.player.Halt();
					try
					{
						Game1.player.CurrentTool.beginUsing(Game1.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, Game1.player);
					}
					catch (Exception)
					{
					}
					((MeleeWeapon)Game1.player.CurrentTool).setFarmerAnimating(Game1.player);
				}
				else
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13915"));
				}
			}
			return result;
		}
		return false;
	}

	public virtual void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
	{
		if (base.state.Value != 2)
		{
			int num = 1;
			int num2 = 0;
			int num3 = 0;
			Vector2 key = tileLocation;
			key.X += 1f;
			if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key] is HoeDirt)
			{
				num2 += 100;
				if (((HoeDirt)Game1.currentLocation.terrainFeatures[key]).state == base.state)
				{
					num3 += 100;
				}
			}
			key.X -= 2f;
			if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key] is HoeDirt)
			{
				num2 += 10;
				if (((HoeDirt)Game1.currentLocation.terrainFeatures[key]).state == base.state)
				{
					num3 += 10;
				}
			}
			key.X += 1f;
			key.Y += 1f;
			if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key] is HoeDirt)
			{
				num2 += 500;
				if (((HoeDirt)Game1.currentLocation.terrainFeatures[key]).state == base.state)
				{
					num3 += 500;
				}
			}
			key.Y -= 2f;
			if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key] is HoeDirt)
			{
				num2 += 1000;
				if (((HoeDirt)Game1.currentLocation.terrainFeatures[key]).state == base.state)
				{
					num3 += 1000;
				}
			}
			num = HoeDirt.drawGuide[num2];
			int num4 = HoeDirt.drawGuide[num3];
			Texture2D texture2D = ((Game1.currentLocation.Name.Equals("Mountain") || Game1.currentLocation.Name.Equals("Mine") || (Game1.currentLocation is MineShaft && Game1.mine.getMineArea() != 121)) ? HoeDirt.darkTexture : HoeDirt.lightTexture);
			if ((Game1.currentSeason.Equals("winter") && !(Game1.currentLocation is Desert) && !Game1.currentLocation.IsGreenhouse && !(Game1.currentLocation is MineShaft)) || (Game1.currentLocation is MineShaft && Game1.mine.getMineArea() == 40 && !Game1.mine.isLevelSlimeArea()))
			{
				texture2D = HoeDirt.snowTexture;
			}
			spriteBatch.Draw(texture2D, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(num % 4 * 16, num / 4 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
			if (base.state.Value == 1)
			{
				spriteBatch.Draw(texture2D, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(num4 % 4 * 16 + 64, num4 / 4 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.2E-08f);
			}
			if (base.fertilizer.Value != 0)
			{
				int num5 = 0;
				switch (base.fertilizer.Value)
				{
				case 369:
					num5 = 1;
					break;
				case 370:
					num5 = 2;
					break;
				case 371:
					num5 = 3;
					break;
				case 465:
					num5 = 4;
					break;
				case 466:
					num5 = 5;
					break;
				}
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(173 + num5 / 2 * 16, 466 + num5 % 2 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.9E-08f);
			}
		}
		if (base.crop != null)
		{
			(base.crop as PeopleCrop).draw(spriteBatch, tileLocation, (base.state.Value == 1 && base.crop.currentPhase.Value == 0 && !base.crop.raisedSeeds.Value) ? (new Color(180, 100, 200) * 1f) : Color.White, 0f);
		}
	}
}
