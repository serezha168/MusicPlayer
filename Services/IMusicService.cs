using System.Collections.Generic;

namespace MusicPlayer.Services
{
    public interface IMusicService
    {
        IEnumerable<MusicFile> GetAllMusicFiles();
        void LoadMusicFiles(string directoryPath);
        MusicFile? GetMusicFileByPath(string filePath);
    }
}