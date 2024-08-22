using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace MusicPlayer.Services
{
    public class MusicService : IMusicService
    {
        private readonly List<MusicFile> _musicFiles = new List<MusicFile>();

        public IEnumerable<MusicFile> GetAllMusicFiles()
        {
            return _musicFiles;
        }

        public void LoadMusicFiles(string directoryPath)
        {
            _musicFiles.Clear();
            var supportedExtensions = new[] { ".mp3", ".wav" };

            var files = Directory.GetFiles(directoryPath)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()));

            foreach (var file in files)
            {
                try
                {
                    using var reader = new AudioFileReader(file);
                    var musicFile = new MusicFile
                    {
                        Title = Path.GetFileNameWithoutExtension(file),
                        Artist = "Unknown", // Можно добавить извлечение метаданных здесь
                        FilePath = file,
                        Duration = reader.TotalTime
                    };
                    _musicFiles.Add(musicFile);
                }
                catch (Exception)
                {
                    // Логирование ошибки, если файл не может быть прочитан
                }
            }
        }

        public MusicFile? GetMusicFileByPath(string filePath)
        {
            return _musicFiles.FirstOrDefault(m => m.FilePath == filePath);
        }
    }

    public class MusicFile
    {
        public required string Title { get; set; }
        public required string Artist { get; set; }
        public required string FilePath { get; set; }
        public TimeSpan Duration { get; set; }
    }
}