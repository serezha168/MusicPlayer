// Services/MusicService.cs
using System.Collections.Generic;
using System.IO;
using TagLib;
using MusicPlayer.Models;

namespace MusicPlayer.Services
{
    public class MusicService
    {
        // ћетод дл€ загрузки треков из указанной директории
        public List<Track> LoadTracks(string directoryPath)
        {
            var tracks = new List<Track>();
            var files = Directory.GetFiles(directoryPath, "*.mp3");

            foreach (var file in files)
            {
                var tagFile = TagLib.File.Create(file);
                tracks.Add(new Track
                {
                    Title = tagFile.Tag.Title ?? "Unknown Title",
                    Artist = tagFile.Tag.FirstPerformer ?? "Unknown Artist",
                    Duration = tagFile.Properties.Duration,
                    FilePath = file
                });
            }

            return tracks;
        }
    }
}
