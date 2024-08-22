using System;
using NAudio.Wave;

namespace MusicPlayer.Services
{
    public class AudioPlayerService
    {
        private IWavePlayer _waveOutDevice;
        private AudioFileReader _audioFileReader;

        public void Play(string filePath)
        {
            Stop();
            _audioFileReader = new AudioFileReader(filePath);
            _waveOutDevice = new WaveOutEvent();
            _waveOutDevice.Init(_audioFileReader);
            _waveOutDevice.Play();
        }

        public void Pause()
        {
            _waveOutDevice?.Pause();
        }

        public void Stop()
        {
            _waveOutDevice?.Stop();
            _waveOutDevice?.Dispose();
            _waveOutDevice = null;
            _audioFileReader?.Dispose();
            _audioFileReader = null;
        }
    }
}