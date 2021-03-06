﻿using BeatSaberPlaylistsLib.Blist.Converters;
using BeatSaberPlaylistsLib.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

// Schema: https://github.com/raftario/blist/blob/master/playlist.schema.json
namespace BeatSaberPlaylistsLib.Blist
{
    /// <summary>
    /// An <see cref="IPlaylistSong"/> that can be serialized in a <see cref="BlistPlaylist"/>.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class BlistPlaylistSong : PlaylistSong
    {
        private Dictionary<string, object>? _customData;

        /// <summary>
        /// Creates an empty <see cref="BlistPlaylistSong"/>.
        /// </summary>
        public BlistPlaylistSong()
        { }

        /// <summary>
        /// Creates a new <see cref="BlistPlaylistSong"/> from the given <paramref name="song"/>.
        /// </summary>
        /// <param name="song"></param>
        public BlistPlaylistSong(ISong song)
            : this()
        {
            this.Populate(song);
        }

        /// <summary>
        /// Sets custom data for a <see cref="BlistPlaylistSong"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetCustomData(string key, object value)
        {
            if (_customData == null) _customData = new Dictionary<string, object>();
            if (_customData.ContainsKey(key))
                _customData[key] = value;
            else
                _customData.Add(key, value);
        }

        /// <summary>
        /// Custom data not included in the schema. Returns null if there are no entries.
        /// Use <see cref="SetCustomData(string, object)"/> to add entries.
        /// </summary>
        [JsonProperty("customData", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object>? CustomData
        {
            get
            {
                if (_customData == null) return _customData;
                if (_customData.Count == 0) return null;
                return _customData;
            }
            set
            {
                _customData = value;
            }
        }

        /// <summary>
        /// The optional RFC3339 date and time the beatmap was added to the playlist
        /// </summary>
        [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
        private DateTime? _serializedDate
        {
            get => DateAdded;
            set => DateAdded = value;
        }

        /// <summary>
        /// The SHA1 hash of the beatmap
        /// </summary>
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        private string? _serializedHash
        {
            get => Hash;
            set => Hash = value;
        }

        /// <summary>
        /// The BeatSaver key of the beatmap
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        private string? _serializedKey
        {
            get => Key;
            set => Key = value;
        }

        /// <summary>
        /// The level ID of the BeatMap
        /// </summary>
        [JsonProperty("levelID", NullValueHandling = NullValueHandling.Ignore)]
        private string? _serializedLevelId
        {
            get => LevelId;
            set => LevelId = value;
        }

        /// <summary>
        /// The optional recommended difficulties for the beatmap
        /// </summary>
        [JsonProperty("difficulties", NullValueHandling = NullValueHandling.Ignore)]
        public List<Difficulty>? _serializedDifficulties
        {
            get => Difficulties;
            set => Difficulties = value;
        }

        /// <summary>
        /// The entry type defining how the beatmap is identified in the playlist
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(PlaylistTypeConverter))]
        public BlistPlaylistType Type
        {
            get
            {
                if (Identifiers.HasFlag(Identifier.Hash)) return BlistPlaylistType.Hash;
                if (Identifiers.HasFlag(Identifier.LevelId)) return BlistPlaylistType.LevelId;
                if (Identifiers.HasFlag(Identifier.Key)) return BlistPlaylistType.Key;
                return BlistPlaylistType.None;
            }
            set
            {
                // TODO: Ignore this or not?
            }
        }

        ///<inheritdoc/>
        [JsonIgnore]
        public override string? Name
        {
            get
            {
                if (CustomData == null) return null;
                if (CustomData.TryGetValue(SongNameKey, out object value))
                {
                    return value.ToString();
                }
                return null;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    if (CustomData == null) return;
                    if (!CustomData.ContainsKey(SongNameKey)) return;
                    CustomData.Remove(SongNameKey);
                    return;
                }
                SetCustomData(SongNameKey, value);
            }
        }
        ///<inheritdoc/>
        [JsonIgnore]
        public override string? LevelAuthorName
        {
            get
            {
                if (CustomData == null) return null;
                if (CustomData.TryGetValue(LevelAuthorNameKey, out object value))
                {
                    return value.ToString();
                }
                return null;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    if (CustomData == null) return;
                    if (!CustomData.ContainsKey(LevelAuthorNameKey)) return;
                    CustomData.Remove(LevelAuthorNameKey);
                    return;
                }
                SetCustomData(LevelAuthorNameKey, value);
            }
        }
        /// <summary>
        /// Key for the song name custom data entry.
        /// </summary>
        public static readonly string SongNameKey = "songName";
        /// <summary>
        /// Key for the level author name custom data entry.
        /// </summary>
        public static readonly string LevelAuthorNameKey = "levelAuthorName";


        ///<inheritdoc/>
        public override bool Equals(IPlaylistSong other)
        {
            if (other == null)
                return false;
            return Hash == other?.Hash;
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            string? keyPart = string.IsNullOrEmpty(Key) ? string.Empty : $"({Key}) ";
            return $"{keyPart}{Name} by {LevelAuthorName}";
        }
    }
}
