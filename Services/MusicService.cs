using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace MusicPlayer.Services
{
    public class MusicService
    {
        public List<Track> GetTracksFromFolder(string folderPath)
        {
            var tracks = new List<Track>();
            var supportedExtensions = new[] { ".mp3", ".wav", ".ogg" };

            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()));

            foreach (var file in files)
            {
                var track = new Track
                {
                    Title = Path.GetFileNameWithoutExtension(file),
                    Artist = "Unknown", // ¬ будущем можно добавить чтение метаданных
                    FilePath = file,
                    Duration = GetAudioFileDuration(file)
                };
                tracks.Add(track);
            }

            return tracks;
        }

        private TimeSpan GetAudioFileDuration(string filePath)
        {
            using (var reader = new AudioFileReader(filePath))
            {
                return reader.TotalTime;
            }
        }
    }
}