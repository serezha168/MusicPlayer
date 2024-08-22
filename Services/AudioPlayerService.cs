using NAudio.Wave;
using System;

namespace MusicPlayer.Services
{
    public class AudioPlayerService : IAudioPlayerService
    {
        private IWavePlayer? _waveOutDevice;
        private AudioFileReader? _audioFileReader;

        public AudioPlayerService()
        {
            _waveOutDevice = new WaveOutEvent();
        }

        public void LoadAudio(string filePath)
        {
            _audioFileReader?.Dispose();
            _audioFileReader = new AudioFileReader(filePath);
            _waveOutDevice?.Init(_audioFileReader);
        }

        public void Play()
        {
            _waveOutDevice?.Play();
        }

        public void Pause()
        {
            _waveOutDevice?.Pause();
        }

        public void Stop()
        {
            _waveOutDevice?.Stop();
        }

        public void SetVolume(float volume)
        {
            if (_waveOutDevice != null)
            {
                _waveOutDevice.Volume = volume;
            }
        }

        public void SetPosition(long position)
        {
            if (_audioFileReader != null)
            {
                _audioFileReader.Position = position;
            }
        }

        public long GetPosition()
        {
            return _audioFileReader?.Position ?? 0;
        }

        public long GetLength()
        {
            return _audioFileReader?.Length ?? 0;
        }

        public void Dispose()
        {
            _audioFileReader?.Dispose();
            _waveOutDevice?.Dispose();
        }
    }
}