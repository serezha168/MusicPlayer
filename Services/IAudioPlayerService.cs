using System;

namespace MusicPlayer.Services
{
    public interface IAudioPlayerService : IDisposable
    {
        void LoadAudio(string filePath);
        void Play();
        void Pause();
        void Stop();
        void SetVolume(float volume);
        void SetPosition(long position);
        long GetPosition();
        long GetLength();
    }
}