﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.Core
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="GameVersion.SWSH"/> encounter area
    /// </summary>
    public sealed class EncounterArea8 : EncounterArea
    {
        /// <inheritdoc />
        public override bool IsMatchLocation(int location)
        {
            if (Location == location)
                return true;

            if (!PermitCrossover)
                return false;

            // Get all other areas that the Location can bleed encounters to
            if (!ConnectingArea8.TryGetValue(Location, out var others))
                return false;

            // Check if any of the other areas are the met location
            return others.Contains((byte)location);
        }

        public override IEnumerable<EncounterSlot> GetMatchingSlots(PKM pkm, IReadOnlyList<EvoCriteria> chain)
        {
            // wild area gets boosted up to level 60 post-game
            var met = pkm.Met_Level;
            bool isBoosted = met == 60 && (IsWildArea8(Location) || IsWildArea8Armor(Location));
            if (isBoosted)
                return GetBoostedMatches(chain);
            return GetUnboostedMatches(chain, met);
        }

        private IEnumerable<EncounterSlot> GetUnboostedMatches(IReadOnlyList<EvoCriteria> chain, int met)
        {
            foreach (var slot in Slots)
            {
                foreach (var evo in chain)
                {
                    if (slot.Species != evo.Species)
                        continue;

                    if (!slot.IsLevelWithinRange(met))
                        break;

                    if (slot.Form != evo.Form && !Legal.WildChangeFormAfter.Contains(evo.Species))
                        break;

                    yield return slot;
                    break;
                }
            }
        }

        private IEnumerable<EncounterSlot> GetBoostedMatches(IReadOnlyList<EvoCriteria> chain)
        {
            foreach (var slot in Slots)
            {
                foreach (var evo in chain)
                {
                    if (slot.Species != evo.Species)
                        continue;

                    // Ignore met level comparison; we already know it is permissible to boost to level 60.

                    if (slot.Form != evo.Form && !Legal.WildChangeFormAfter.Contains(evo.Species))
                        break;

                    yield return slot;
                    break;
                }
            }
        }

        public static bool IsWildArea8(int loc) => 122 <= loc && loc <= 154; // Rolling Fields -> Lake of Outrage
        public static bool IsWildArea8Armor(int loc) => 164 <= loc && loc <= 194; // Fields of Honor -> Honeycalm Island

        // Location, and areas that it can feed encounters to.
        public static readonly IReadOnlyDictionary<int, IReadOnlyList<byte>> ConnectingArea8 = new Dictionary<int, IReadOnlyList<byte>>
        {
            // Rolling Fields
            // Dappled Grove, East Lake Axewell, West Lake Axewell
            // Also connects to South Lake Miloch but too much of a stretch
            {122, new byte[] {124, 128, 130}},

            // Dappled Grove
            // Rolling Fields, Watchtower Ruins
            {124, new byte[] {122, 126}},

            // Watchtower Ruins
            // Dappled Grove, West Lake Axewell
            {126, new byte[] {124, 130}},

            // East Lake Axewell
            // Rolling Fields, West Lake Axewell, Axew's Eye, North Lake Miloch
            {128, new byte[] {122, 130, 132, 138}},

            // West Lake Axewell
            // Rolling Fields, Watchtower Ruins, East Lake Axewell, Axew's Eye
            {130, new byte[] {122, 126, 128, 132}},

            // Axew's Eye
            // East Lake Axewell, West Lake Axewell
            {132, new byte[] {128, 130}},

            // South Lake Miloch
            // Giant's Seat, North Lake Miloch
            {134, new byte[] {136, 138}},

            // Giant's Seat
            // South Lake Miloch, North Lake Miloch
            {136, new byte[] {134, 138}},

            // North Lake Miloch
            // East Lake Axewell, South Lake Miloch, Giant's Seat
            // Also connects to Motostoke Riverbank but too much of a stretch
            {138, new byte[] {134, 136}},

            // Motostoke Riverbank
            // Bridge Field
            {140, new byte[] {142}},

            // Bridge Field
            // Motostoke Riverbank, Stony Wilderness
            {142, new byte[] {140, 144}},

            // Stony Wilderness
            // Bridge Field, Dusty Bowl, Giant's Mirror, Giant's Cap
            {144, new byte[] {142, 146, 148, 152}},

            // Dusty Bowl
            // Stony Wilderness, Giant's Mirror, Hammerlocke Hills
            {146, new byte[] {144, 148, 150}},

            // Giant's Mirror
            // Stony Wilderness, Dusty Bowl, Hammerlocke Hills
            {148, new byte[] {144, 146, 148}},

            // Hammerlocke Hills
            // Dusty Bowl, Giant's Mirror, Giant's Cap
            {150, new byte[] {146, 148, 152}},

            // Giant's Cap
            // Stony Wilderness, Giant's Cap
            // Also connects to Lake of Outrage but too much of a stretch
            {152, new byte[] {144, 150}},

            // Lake of Outrage is just itself.

            // Challenge Beach
            // Soothing Wetlands, Courageous Cavern
            {170, new byte[] {166, 176}},

            // Challenge Road
            // Brawler's Cave
            {174, new byte[] {172}},

            // Courageous Cavern
            // Loop Lagoon
            {176, new byte[] {178}},

            // Warm-Up Tunnel
            // Training Lowlands, Potbottom Desert
            {182, new byte[] {180, 184}},

            // Workout Sea
            // Fields of Honor
            {186, new byte[] {164}},

            // Stepping-Stone Sea
            // Fields of Honor
            {188, new byte[] {170}},

            // Insular Sea
            // Honeycalm Sea
            {190, new byte[] {192}},

            // Honeycalm Sea
            // Honeycalm Island
            {192, new byte[] {194}},
        };

        /// <summary>
        /// Slots from this area can cross over to another area, resulting in a different met location.
        /// </summary>
        public bool PermitCrossover { get; internal set; }

        public static EncounterArea8[] GetAreas(byte[][] input, GameVersion game)
        {
            var result = new EncounterArea8[input.Length];
            for (int i = 0; i < input.Length; i++)
                result[i] = new EncounterArea8(input[i], game);
            return result;
        }

        private EncounterArea8(byte[] areaData, GameVersion game) : base(game)
        {
            Location = areaData[0];
            Slots = ReadSlots(areaData, areaData[1]);
        }

        private EncounterSlot[] ReadSlots(byte[] areaData, byte slotCount)
        {
            var slots = new EncounterSlot[slotCount];

            int ctr = 0;
            int ofs = 2;
            do
            {
                var flags = (AreaWeather8) BitConverter.ToUInt16(areaData, ofs);
                var min = areaData[ofs + 2];
                var max = areaData[ofs + 3];
                var count = areaData[ofs + 4];
                // ofs+5 reserved
                ofs += 6;
                for (int i = 0; i < count; i++, ctr++, ofs += 2)
                {
                    var specForm = BitConverter.ToUInt16(areaData, ofs);
                    slots[ctr] = new EncounterSlot8(this, specForm, min, max, flags);
                }
            } while (ctr != slots.Length);

            return slots;
        }
    }

    /// <summary>
    /// Encounter Conditions for <see cref="GameVersion.SWSH"/>
    /// </summary>
    /// <remarks>Values above <see cref="All"/> are for Shaking/Fishing hidden encounters only.</remarks>
    [Flags]
    public enum AreaWeather8
    {
        None,
        Normal = 1,
        Overcast = 1 << 1,
        Raining = 1 << 2,
        Thunderstorm = 1 << 3,
        Intense_Sun = 1 << 4,
        Snowing = 1 << 5,
        Snowstorm = 1 << 6,
        Sandstorm = 1 << 7,
        Heavy_Fog = 1 << 8,

        All = Normal | Overcast | Raining | Thunderstorm | Intense_Sun | Snowing | Snowstorm | Sandstorm | Heavy_Fog,

        Shaking_Trees = 1 << 9,
        Fishing = 1 << 10,

        NotWeather = Shaking_Trees | Fishing,
    }

    /// <summary>
    /// Encounter Slot found in <see cref="GameVersion.SWSH"/>
    /// </summary>
    public sealed class EncounterSlot8 : EncounterSlot
    {
        public readonly AreaWeather8 Weather;
        public override string LongName => Weather == AreaWeather8.All ? wild : $"{wild} - {Weather.ToString().Replace("_", string.Empty)}";
        public override int Generation => 8;

        public EncounterSlot8(EncounterArea8 area, int specForm, int min, int max, AreaWeather8 weather) : base(area)
        {
            Species = specForm & 0x7FF;
            Form = specForm >> 11;
            LevelMin = min;
            LevelMax = max;

            Weather = weather;
        }
    }
}
