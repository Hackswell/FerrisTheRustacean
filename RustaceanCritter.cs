using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Menus;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FerrisTheRustacean;

public class RustaceanCritter : CrabCritter
{
	private const string ferrisTextureName = "Mods/hackswell.ferristherustacean/assets";
	private const int spriteSize = 24;

	// Bounding box in the tile of the sprites we'll use
	new protected Rectangle _baseSourceRectangle = new Rectangle(0, 0, spriteSize, spriteSize);

    public RustaceanCritter()
    {
	    sprite = new AnimatedSprite(ferrisTextureName, 0, spriteSize, spriteSize);
	    sprite.SourceRect = _baseSourceRectangle;
        sprite.ignoreSourceRectUpdates = true;
        _crabVariant = 1;			// Bottom row of crabs
        UpdateSpriteRectangle();
    }


    public RustaceanCritter(Vector2 start_position) : this()
    {
        position = start_position;
        float movementRectangleWidth = 256f;		// Move laterally +-128 px.  Vertically 0.
        movementBounds = new Rectangle((int)(start_position.X - movementRectangleWidth / 2f), (int)start_position.Y,
	        (int)movementRectangleWidth, 0);
    }

    public override void UpdateSpriteRectangle()
    {
	    Rectangle rectangle = _baseSourceRectangle;
	    rectangle.Y += _crabVariant * spriteSize;
	    int drawnFrame = _currentFrame;
	    if (drawnFrame == 4)
	    {
		    drawnFrame = 1;
	    }
	    rectangle.X += drawnFrame * spriteSize;
	    sprite.SourceRect = rectangle;
    }

	// Hackswell:  Overriding this, too. =sigh= Only because Ferris isn't as Skittish as a CrabCritter.
	public override bool update(GameTime time, GameLocation environment)
	{
		nextFrameChange -= (float)time.ElapsedGameTime.TotalSeconds;
		if (skittering)
		{
			skitterTime -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (nextFrameChange <= 0f && (moving || skittering))
		{
			_currentFrame++;
			if (_currentFrame >= 4)
			{
				_currentFrame = 0;
			}
			if (skittering)
			{
				nextFrameChange = Utility.RandomFloat(0.025f, 0.05f, null);
			}
			else
			{
				nextFrameChange = Utility.RandomFloat(0.05f, 0.15f, null);
			}
		}

		if (skittering)
		{
			if (yJumpOffset >= 0f)
			{
				if (!diving)
				{
					gravityAffectedDY = Game1.random.Next(0, 4) == 0 ? -4f : -2f;
				}
				else
				{
					if (environment.isWaterTile((int)position.X / 64, (int)position.Y / 64))
					{
						environment.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 50f, 2, 1, position, false, false));
						Game1.playSound("dropItemInWater");
						return true;
					}
					gravityAffectedDY = -4f;
				}
			}
		}
		else
		{
			nextCharacterCheck -= (float)time.ElapsedGameTime.TotalSeconds;
			if (nextCharacterCheck <= 0f)
			{
				Character character = Utility.isThereAFarmerOrCharacterWithinDistance(
					this.position / 64f,
					Mod.Conf.FerrisSkitterDistance,
					environment);

				if (character != null)
				{
					_crabVariant = 0;
					skittering = true;
					movementDirection.X = character.position.X > position.X ? -3f : 3f;
				}
				nextCharacterCheck = 0.25f;
			}
			if (!skittering)
			{
				if (moving && yJumpOffset >= 0f)
				{
					gravityAffectedDY = -1f;
				}
				nextMovementChange -= (float)time.ElapsedGameTime.TotalSeconds;
				if (nextMovementChange <= 0f)
				{
					moving = !moving;
					if (moving)
					{
						movementDirection.X = Game1.random.NextBool() ? -1f : 1f;
						nextMovementChange = Utility.RandomFloat(0.15f, 0.5f, null);
					}
					else
					{
						movementDirection = Vector2.Zero;
						nextMovementChange = Utility.RandomFloat(0.2f, 1f, null);
					}
				} // end nextMovement
			} // end if not skittering
		}

		position += movementDirection;
		if (!diving && !environment.isTilePassable(new Location((int)(position.X / 64f), (int)(position.Y / 64f)), Game1.viewport))
		{
			position -= movementDirection;
			movementDirection *= -1f;
		}
		if (!skittering)
		{

			if (position.X < movementBounds.Left)
			{
				position.X = movementBounds.Left;
				movementDirection *= -1f;
			}
			if (position.X > movementBounds.Right)
			{
				position.X = movementBounds.Right;
				movementDirection *= -1f;
			}
		}
		else if (!diving && environment.isWaterTile((int)(position.X / 64f + (float)Math.Sign(movementDirection.X)), (int)position.Y / 64))
		{
			if (yJumpOffset >= 0f)
			{
				gravityAffectedDY = -7f;
			}
			diving = true;
		}

		UpdateSpriteRectangle();

		if (skitterTime <= 0f)
		{
			return true;
		}
		return base.update(time, environment);
	}

}
