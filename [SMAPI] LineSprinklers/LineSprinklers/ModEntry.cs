﻿using System.Collections.Generic;
using System.Linq;
using LineSprinklers.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LineSprinklers
{
    /// <summary>The mod entry class.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The object names for the sprinklers added by the mod.</summary>
        private readonly string[] Names = {
            "Iridium Line Sprinkler (D)",
            "Iridium Line Sprinkler (L)",
            "Iridium Line Sprinkler (R)",
            "Iridium Line Sprinkler (U)",
            "Line Sprinkler (D)",
            "Line Sprinkler (L)",
            "Line Sprinkler (R)",
            "Line Sprinkler (U)",
            "Quality Line Sprinkler (D)",
            "Quality Line Sprinkler (L)",
            "Quality Line Sprinkler (R)",
            "Quality Line Sprinkler (U)"
        };

        /// <summary>The relative tile coverage by sprinkler ID.</summary>
        private readonly IDictionary<int, Vector2[]> Coverage = new Dictionary<int, Vector2[]>();

        /// <summary>The warning messages that have already been displayed.</summary>
        private readonly HashSet<string> SuppressWarnings = new HashSet<string>();


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        /// <summary>Get an API that other mods can access. This is always called after <see cref="M:StardewModdingAPI.Mod.Entry(StardewModdingAPI.IModHelper)" />.</summary>
        public override object GetApi()
        {
            return new LineSprinklersApi(this.Coverage);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // reset sprinkler coverage
            this.Coverage.Clear();
            foreach (var pair in this.GetSprinklerCoverage())
                this.Coverage[pair.Key] = pair.Value;

            // apply
            foreach (GameLocation location in this.GetLocations())
            {
                foreach (SObject obj in location.Objects.Values)
                {
                    if (this.Names.Contains(obj.Name) && this.Coverage.TryGetValue(obj.ParentSheetIndex, out Vector2[] coverage))
                    {
                        foreach (Vector2 offset in coverage)
                        {
                            Vector2 tile = obj.TileLocation + offset;
                            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrainFeature) && terrainFeature is HoeDirt hoeDirt)
                                hoeDirt.state.Value = HoeDirt.watered;
                        }
                    }
                }
            }
        }

        /// <summary>Get all in-game locations.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            foreach (GameLocation location in Game1.locations)
            {
                yield return location;
                if (location is BuildableGameLocation buildableLocation)
                {
                    foreach (Building building in buildableLocation.buildings)
                    {
                        if (building.indoors.Value != null)
                            yield return building.indoors.Value;
                    }
                }
            }
        }

        /// <summary>Get the relative tile coverage by supported sprinkler ID. Note that sprinkler IDs may change after a save is loaded due to Json Assets reallocating IDs.</summary>
        private IDictionary<int, Vector2[]> GetSprinklerCoverage()
        {
            // get Json Assets API
            IJsonAssetsApi jsonAssets = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (jsonAssets == null)
            {
                this.LogOnce("Can't access the Json Assets API. Is the mod installed correctly?", LogLevel.Error);
                return new Dictionary<int, Vector2[]>();
            }

            // get sprinkler coverage
            IDictionary<int, Vector2[]> coverage = new Dictionary<int, Vector2[]>();
            foreach (string name in this.Names)
            {
                // get sprinkler ID
                int id = jsonAssets.GetBigCraftableId(name);
                if (id == -1)
                {
                    this.LogOnce($"Can't find {name} in the Json Assets data. Is the mod installed correctly?", LogLevel.Warn);
                    continue;
                }

                // get coverage
                coverage[id] = this.GetSprinklerCoverage(name).ToArray();
            }

            return coverage;
        }

        /// <summary>Get the relative tile coverage by supported sprinkler ID. Note that sprinkler IDs may change after a save is loaded due to Json Assets reallocating IDs.</summary>
        /// <param name="name">The sprinkler name.</param>
        private IEnumerable<Vector2> GetSprinklerCoverage(string name)
        {
            switch (name)
            {
                case "Line Sprinkler (D)":
                    for (int y = 0; y < 4; y++)
                        yield return new Vector2(0, y);
                    break;

                case "Line Sprinkler (U)":
                    for (int y = 0; y < 4; y++)
                        yield return new Vector2(0, -y);
                    break;

                case "Line Sprinkler (L)":
                    for (int x = 0; x < 4; x++)
                        yield return new Vector2(-x, 0);
                    break;

                case "Line Sprinkler (R)":
                    for (int x = 0; x < 4; x++)
                        yield return new Vector2(x, 0);
                    break;

                case "Quality Line Sprinkler (D)":
                    for (int y = 0; y < 8; y++)
                        yield return new Vector2(0, y);
                    break;

                case "Quality Line Sprinkler (U)":
                    for (int y = 0; y < 8; y++)
                        yield return new Vector2(0, -y);
                    break;

                case "Quality Line Sprinkler (L)":
                    for (int x = 0; x < 8; x++)
                        yield return new Vector2(-x, 0);
                    break;

                case "Quality Line Sprinkler (R)":
                    for (int x = 0; x < 8; x++)
                        yield return new Vector2(x, 0);
                    break;

                case "Iridium Line Sprinkler (D)":
                    for (int y = 0; y < 24; y++)
                        yield return new Vector2(0, y);
                    break;

                case "Iridium Line Sprinkler (U)":
                    for (int y = 0; y < 24; y++)
                        yield return new Vector2(0, -y);
                    break;

                case "Iridium Line Sprinkler (L)":
                    for (int x = 0; x < 24; x++)
                        yield return new Vector2(-x, 0);
                    break;

                case "Iridium Line Sprinkler (R)":
                    for (int x = 0; x < 24; x++)
                        yield return new Vector2(x, 0);
                    break;
            }
        }

        /// <summary>Log a message only once.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The log level.</param>
        private void LogOnce(string message, LogLevel level)
        {
            if (this.SuppressWarnings.Add(message))
                this.Monitor.Log(message, level);
        }
    }
}
