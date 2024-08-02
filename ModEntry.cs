// FerrisTheRustacean.ModEntry
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using SVObject = StardewValley.Object;

namespace FerrisTheRustacean
{
	internal class Mod : StardewModdingAPI.Mod
	{
		private const string ferrisTextureName = "Mods/hackswell.ferristherustacean/assets";
        private static IModHelper helperFerris;
        public static Texture2D textureFerris;
        public static IAssetName Ferris;
        public static ModConfig Conf;

        private static Dictionary<string, List<Vector2>> parsedMaps = new Dictionary<string, List<Vector2>>();

		public override void Entry(IModHelper helper)
		{
			Log.Monitor = this.Monitor;
			helperFerris = helper;
            Conf = LoadConfig();

			helper.Events.Content.AssetRequested += OnAssetRequested;
			helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
			helper.Events.Player.Warped += OnWarped;
		}

		private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
		{
		    if (e.NameWithoutLocale.IsEquivalentTo(ferrisTextureName))
			{
				e.LoadFromModFile<Texture2D>("assets/Ferris.png", AssetLoadPriority.Medium);
			}
		}

		private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			addRustaceans(Game1.currentLocation);
		}

		private void OnWarped(object sender, WarpedEventArgs e)
		{
			GameLocation newLocation = e.NewLocation;
			addRustaceans(newLocation);
		}

		private void addRustaceans(GameLocation location)
		{
			int maxWidth = location.Map.DisplayWidth / 64;
			int maxHeight = location.Map.DisplayHeight / 64;

			if (! Conf.AllowedLocations.Contains(location.Name))  return;


			Log.Info($"Warped to {location.Name}. Adding Rustaceans.");

			// If the dictionary doesn't have the list for our map, let's build it the first time.
			if (!parsedMaps.ContainsKey(location.Name))
			{
				List<Vector2> validTiles = new List<Vector2>();

				Log.Info($"addRustateans() Location: {location.Name} => mW: {maxWidth}\tmH: {maxHeight}");
				for (int x = 0; x < maxWidth; x++)
				{
					for (int y = 0; y < maxHeight; y++)
					{
						Vector2 testTile = new Vector2(x, y);

						// All the conditions we DO NOT want:
						if (!location.isTileOnMap(testTile)) continue;
						if (!location.CanItemBePlacedHere(testTile, false, CollisionMask.All,
							    CollisionMask.Buildings | CollisionMask.Characters | CollisionMask.Farmers |
							    CollisionMask.Flooring | CollisionMask.Furniture | CollisionMask.TerrainFeatures |
							    CollisionMask.LocationSpecific, false, false)) continue;
						if (location.isWaterTile((int)testTile.X, (int)testTile.Y)) continue;
						bool isDiggable = (location.doesTileHavePropertyNoNull((int)testTile.X, (int)testTile.Y, "Diggable", "Back") != "");
						string type = location.doesTileHavePropertyNoNull((int)testTile.X, (int)testTile.Y, "Type", "Back");
						if (isDiggable || (type != "Dirt" && type != "Wood")) continue;

						// If we haven't broken out by this time, we do have a "good" tile! Huzzah!  -Hackswell
						validTiles.Add(testTile);
					}
				} // end build validList for location

				Log.Info($"\tGenerated good tiles for {location.Name}.  Found {validTiles.Count} tiles!");
				parsedMaps[location.Name] = validTiles;
			}

			// Lets add some crabby critters!
			for (int groupNum = 0; groupNum < Conf.NumGroups; groupNum++)
			{
				List<Vector2> validList = parsedMaps[location.Name];
				if (validList is null)
				{
					Log.Error($"How in the world did we NOT generate a list of valid tiles for {location.Name}??");
					return;
				}

				Vector2 targetTile = validList[Game1.random.Next(validList.Count)];
				int numCrabs = Game1.random.Next(1, Conf.MaxNumCrabsPerGroup+1);
				foreach (Vector2 crabTile in Utility.getPositionsInClusterAroundThisTile(targetTile, numCrabs))
				{
					Log.Info($"\tAdding Rustacean to Group {groupNum}: **{crabTile.X}, {crabTile.Y}**!");
					location.addCritter(new RustaceanCritter((crabTile * 64f) + new Vector2(32f, 32f)));
				} // For each crab in the group
			} // For numGroups of 1-maxNumCrabsPerGroup crabs

			// Static critters, just so we know it's working
			if (location is Beach)
			{
				Log.Info($"Adding Static Rustacean at 19.0, 13.5"); // Upper Left
				location.addCritter(new RustaceanCritter(new Vector2(19.0f * 64f, 13.5f * 64f)));
				Log.Info($"Adding Static Crab at 90.0, 24.5"); // Lower Right
				location.addCritter(new CrabCritter(new Vector2(90.0f * 64f, 24.5f * 64f)));
				Log.Info($"Adding Static Crab at 38.0, 32.0"); // On the dock
				location.addCritter(new CrabCritter(new Vector2(38.0f * 64f, 32.0f * 64f)));
			}
		}

		protected static ModConfig LoadConfig()
        {
            return helperFerris != null ? Conf = helperFerris.ReadConfig<ModConfig>() : new ModConfig();
        }
	}
}
