using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace PeopleSeeds;

public class PeopleFurniture : Furniture
{
	internal int personId = -1;

	internal int gquality = 1;

	internal bool living;

	public PeopleFurniture()
	{
	}

	public PeopleFurniture(int personId, Vector2 tp)
		: base(0, tp)
	{
		this.personId = personId;
		base.tileLocation.Value = tp;
		base.isOn.Value = false;
		base.ParentSheetIndex = 0;
		base.name = People.GetPerson(personId);
		((StardewValley.Object)this).DisplayName = base.Name;
		base.furniture_type.Value = 0;
		base.defaultSourceRect.Value = new Rectangle(0, 0, 16, 32);
		base.sourceRect.Value = new Rectangle(0, 0, 16, 32);
		base.drawHeldObjectLow.Value = false;
		base.defaultBoundingBox.Value = new Rectangle((int)base.tileLocation.X * 64, (int)base.tileLocation.Y * 64, 64, 64);
		base.boundingBox.Value = new Rectangle((int)base.tileLocation.X * 64, (int)base.tileLocation.Y * 64, 64, 64);
		base.updateDrawPosition();
		base.rotations.Value = 0;
		base.bigCraftable.Value = true;
		base.Category = -17;
		base.edibility.Value = 729;
		base.price.Value = 490;
	}

	public override int maximumStackSize()
	{
		return 99;
	}

	public override bool canBeGivenAsGift()
	{
		return true;
	}

	public virtual bool canBePlacedHere(GameLocation l, Vector2 tile)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (!(l is DecoratableLocation))
		{
			return false;
		}
		if (Utility.distance(tile.X, ((Character)Game1.player).getTileLocation().X, tile.Y, ((Character)Game1.player).getTileLocation().Y) > 3f)
		{
			return false;
		}
		for (int i = 0; i < base.boundingBox.Width / 64; i++)
		{
			for (int j = 0; j < base.boundingBox.Height / 64; j++)
			{
				Vector2 vector = tile * 64f + new Vector2(i, j) * 64f;
				vector.X += 32f;
				vector.Y += 32f;
				Enumerator<Furniture> enumerator = (l as DecoratableLocation).furniture.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Furniture current = enumerator.Current;
						if (current.furniture_type.Value == 11 && ((StardewValley.Object)current).getBoundingBox(current.TileLocation).Contains((int)vector.X, (int)vector.Y) && current.heldObject.Value == null && base.getTilesWide() == 1)
						{
							return true;
						}
						if ((current.furniture_type.Value != 12 || base.furniture_type.Value == 12) && ((StardewValley.Object)current).getBoundingBox(current.TileLocation).Contains((int)vector.X, (int)vector.Y))
						{
							return false;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
				}
				Vector2 key = tile + new Vector2(i, j);
				if (l.Objects.ContainsKey(key))
				{
					return false;
				}
			}
		}
		return base.canBePlacedHere(l, tile);
	}

	public PeopleFurniture CloneOne()
	{
		return new PeopleFurniture(this.personId, Vector2.Zero)
		{
			Quality = ((StardewValley.Object)this).Quality,
			gquality = this.gquality,
			Stack = 1
		};
	}

	public virtual int addToStack(int amount)
	{
		((StardewValley.Object)this).stack.Value += amount;
		if (((StardewValley.Object)this).stack.Value > 99)
		{
			int result = ((StardewValley.Object)this).stack.Value - 99;
			((StardewValley.Object)this).stack.Value = 99;
			return result;
		}
		return 0;
	}

	public override Color getCategoryColor()
	{
		return new Color(255, 80, 80);
	}

	public override string getCategoryName()
	{
		return "People";
	}

	public override string getDescription()
	{
		return "It's " + base.name + ", but they are still, as if frozen.";
	}

	private float GetScaleSize()
	{
		int num = base.sourceRect.Width / 16;
		int num2 = base.sourceRect.Height / 16;
		if (num >= 5)
		{
			return 0.75f;
		}
		if (num2 >= 3)
		{
			return 1f;
		}
		if (num <= 2)
		{
			return 2f;
		}
		if (num <= 4)
		{
			return 1f;
		}
		return 0.1f;
	}

	public override bool canBeShipped()
	{
		return true;
	}

	public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
	{
		if (Game1.player.itemToEat == this)
		{
			spriteBatch.Draw(People.CloneTemplates[People.GetPerson(this.personId)].Sprite.Texture, objectPosition, NetFieldBase<Rectangle, NetRectangle>.op_Implicit((NetFieldBase<Rectangle, NetRectangle>)base.defaultSourceRect), Color.White, 0f, new Vector2(0f, 3f), 4f, SpriteEffects.None, Math.Max(0f, (float)(((Character)f).getStandingY() + 2) / 10000f));
		}
	}

	public virtual void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
	{
		spriteBatch.Draw(People.CloneTemplates[People.GetPerson(this.personId)].Sprite.Texture, location + new Vector2(32f, 32f), NetFieldBase<Rectangle, NetRectangle>.op_Implicit((NetFieldBase<Rectangle, NetRectangle>)base.defaultSourceRect), color * transparency, 0f, new Vector2(base.defaultSourceRect.Width / 2, base.defaultSourceRect.Height / 2), 1f * this.GetScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
		if (drawStackNumber && (double)scaleSize > 0.3 && this.Stack > 1)
		{
			Utility.drawTinyDigits(NetFieldBase<int, NetInt>.op_Implicit((NetFieldBase<int, NetInt>)((StardewValley.Object)this).stack), spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(NetFieldBase<int, NetInt>.op_Implicit((NetFieldBase<int, NetInt>)((StardewValley.Object)this).stack), 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
		}
		if (drawStackNumber && ((StardewValley.Object)this).quality.Value > 0)
		{
			float num = ((((StardewValley.Object)this).quality.Value < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f));
			spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, 52f + num), (((StardewValley.Object)this).quality.Value < 4) ? new Rectangle(338 + (((StardewValley.Object)this).quality.Value - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8), color * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + num), SpriteEffects.None, layerDepth);
		}
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		if (x == -1)
		{
			spriteBatch.Draw(People.CloneTemplates[People.GetPerson(this.personId)].Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, NetFieldBase<Vector2, NetVector2>.op_Implicit((NetFieldBase<Vector2, NetVector2>)base.drawPosition)), NetFieldBase<Rectangle, NetRectangle>.op_Implicit((NetFieldBase<Rectangle, NetRectangle>)base.sourceRect), Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(base.boundingBox.Value.Bottom - 8) / 10000f);
		}
		else
		{
			spriteBatch.Draw(People.CloneTemplates[People.GetPerson(this.personId)].Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (base.sourceRect.Height * 4 - base.boundingBox.Height))), NetFieldBase<Rectangle, NetRectangle>.op_Implicit((NetFieldBase<Rectangle, NetRectangle>)base.sourceRect), Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(base.boundingBox.Value.Bottom - 8) / 10000f);
		}
		if (base.heldObject.Value != null)
		{
			spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.boundingBox.Center.X - 32, base.boundingBox.Center.Y - (base.drawHeldObjectLow.Value ? 17 : 70))), GameLocation.getSourceRectForObject(base.heldObject.Value.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipVertically, (float)(base.boundingBox.Bottom + 1) / 10000f);
		}
	}
}
