using System;
using NAudio.Wave;

namespace MusicPlayer.Services
{
    public class AudioPlayerService
    {
        private IWavePlayer _waveOutDevice;
        private AudioFileReader _audioFileReader;

        public bool IsPlaying => _waveOutDevice?.PlaybackState == PlaybackState.Playing;

        public float Volume
        {
            get => _audioFileReader?.Volume ?? 0f;
            set
            {
                if (_audioFileReader != null)
                {
                    _audioFileReader.Volume = Math.Clamp(value, 0f, 1f);
                }
            }
        }

        public void Play(string filePath)
        {
            Stop();

            _audioFileReader = new AudioFileReader(filePath);
            _waveOutDevice = new WaveOutEvent();
            _waveOutDevice.Init(_audioFileReader);
            _waveOutDevice.Play();
        }

        public void Stop()
        {
            _waveOutDevice?.Stop();
            _waveOutDevice?.Dispose();
            _waveOutDevice = null;
            _audioFileReader?.Dispose();
            _audioFileReader = null;
        }

        public void Pause()
        {
            _waveOutDevice?.Pause();
        }

        public void Resume()
        {
            if (_waveOutDevice?.PlaybackState == PlaybackState.Paused)
            {
                _waveOutDevice.Play();
            }
        }

        public TimeSpan GetPosition()
        {
            return _audioFileReader?.CurrentTime ?? TimeSpan.Zero;
        }

        public TimeSpan GetLength()
        {
            return _audioFileReader?.TotalTime ?? TimeSpan.Zero;
        }

        public void SetPosition(TimeSpan position)
        {
            if (_audioFileReader != null)
            {
                _audioFileReader.CurrentTime = position;
            }
        }
    }
}
