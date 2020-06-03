﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeatSaberPlaylistsLib.Types
{
    /// <summary>
    /// Base class for a Playlist.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Playlist<T> : IPlaylist<T>
        where T : IPlaylistSong, new()
    {
        /// <summary>
        /// Internal collection of songs in the playlist.
        /// </summary>
        protected abstract IList<T> Songs { get; set; }

        /// <inheritdoc/>
        public IPlaylistSong this[int index]
        {
            get => Songs[index];

            set
            {
                Songs[index] = CreateFrom(value);
            }
        }

        /// <inheritdoc/>
        public event EventHandler? PlaylistChanged;


        /// <inheritdoc/>
        public abstract string Title { get; set; }
        /// <inheritdoc/>
        public abstract string? Author { get; set; }
        /// <inheritdoc/>
        public abstract string? Description { get; set; }
        /// <inheritdoc/>
        public abstract string Filename { get; set; }
        /// <inheritdoc/>
        public string? SuggestedExtension { get; set; }
        /// <inheritdoc/>
        public bool AllowDuplicates { get; set; }
        /// <inheritdoc/>
        public int Count => Songs.Count;
        /// <inheritdoc/>
        public virtual bool IsReadOnly => false;

        /// <summary>
        /// Creates a new <see cref="IPlaylistSong"/> of type <typeparamref name="T"/> from the given <paramref name="song"/>.
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        protected abstract T CreateFrom(ISong song);

        /// <inheritdoc/>
        public virtual IPlaylistSong? Add(ISong song)
        {
            if (AllowDuplicates || !Songs.Any(s => s.Hash == song.Hash || (song.Key != null && s.Key == song.Key)))
            {
                T playlistSong = CreateFrom(song);
                Songs.Add(playlistSong);
                return playlistSong;
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual IPlaylistSong? Add(string songHash, string? songName, string? songKey, string? mapper) =>
            Add(new T()
            {
                Hash = songHash,
                Name = songName,
                Key = songKey,
                LevelAuthorName = mapper
            });

        /// <inheritdoc/>
        public virtual IPlaylistSong? Add(IPlaylistSong item) => Add((ISong)item);

        /// <inheritdoc/>
        public virtual void Clear()
        {
            Songs.Clear();
        }

        /// <inheritdoc/>
        public virtual void Sort()
        {
            Songs = Songs.OrderByDescending(s => s.DateAdded).ToList();
        }

        /// <inheritdoc/>
        public virtual bool Contains(IPlaylistSong item)
        {
            return Songs.Any(s => s.Equals(item));
        }

        /// <inheritdoc/>
        public void CopyTo(IPlaylistSong[] array, int arrayIndex)
        {
            int index = arrayIndex;
            foreach (T song in Songs)
            {
                array[index] = song;
                index++;
            }
        }

        /// <inheritdoc/>
        public abstract Stream GetCoverStream();

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that iterates through the playlist's songs.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Songs.GetEnumerator();
        }

        /// <inheritdoc/>
        public int IndexOf(IPlaylistSong item)
        {
            if (item is T song)
                return Songs.IndexOf(song);
            else
                return -1;
        }

        /// <inheritdoc/>
        public void Insert(int index, IPlaylistSong item)
        {
            Songs.Insert(index, CreateFrom(item));
        }

        /// <inheritdoc/>
        public void RaisePlaylistChanged()
        {
            EventHandler? handler = PlaylistChanged;
            handler?.Invoke(this, null);
        }

        /// <inheritdoc/>
        public bool Remove(IPlaylistSong item)
        {
            bool songRemoved = false;
            if (item is T matchedType)
                songRemoved = Songs.Remove(matchedType);
            else
            {
                T song = Songs.FirstOrDefault(s => s.Equals(item));
                if (song != null)
                    songRemoved = Songs.Remove(song);
            }
            return songRemoved;
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            Songs.RemoveAt(index);
        }

        /// <inheritdoc/>
        public int RemoveAll(Func<IPlaylistSong, bool> match)
        {
            int songsRemoved = 0;
            T[]? toRemove = Songs.Where(s => match(s)).ToArray();
            foreach (T song in toRemove)
            {
                if (Songs.Remove(song))
                    songsRemoved++;
            }
            return songsRemoved;
        }

        /// <inheritdoc/>
        public abstract int RemoveAll(Func<T, bool> match);

        /// <inheritdoc/>
        public void RemoveDuplicates()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public abstract void SetCover(byte[] coverImage);

        /// <inheritdoc/>
        public abstract void SetCover(string? coverImageStr);

        /// <inheritdoc/>
        public abstract void SetCover(Stream stream);

        /// <inheritdoc/>
        public bool TryRemoveByHash(string songHash)
        {
            songHash = songHash.ToUpper();
            return RemoveAll((IPlaylistSong s) => s.Hash == songHash) > 0;
        }

        /// <inheritdoc/>
        public bool TryRemoveByKey(string songKey)
        {
            songKey = songKey.ToLower();
            return RemoveAll((IPlaylistSong s) => s.Key == songKey) > 0;
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Songs.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator<IPlaylistSong> IEnumerable<IPlaylistSong>.GetEnumerator()
        {
            IList<IPlaylistSong>? thing = (IList<IPlaylistSong>)Songs;
            return thing.GetEnumerator();
        }

        /// <inheritdoc/>
        void ICollection<IPlaylistSong>.Add(IPlaylistSong item)
        {
            Add(item);
        }
    }
}