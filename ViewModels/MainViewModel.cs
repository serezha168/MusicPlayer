using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using NAudio.Wave;

namespace MusicPlayer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private WaveOutEvent? outputDevice;
        private AudioFileReader? audioFile;
        private bool isPlaying;
        private double currentPosition;
        private string? currentTime;
        private string? totalTime;
        private double volume = 1.0;
        private bool isUpdatingPosition;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;
                    OnPropertyChanged();
                }
            }
        }

        public double CurrentPosition
        {
            get => currentPosition;
            set
            {
                if (!isUpdatingPosition && Math.Abs(currentPosition - value) > 0.001)
                {
                    currentPosition = value;
                    OnPropertyChanged();
                    if (audioFile != null)
                    {
                        SetPosition(value);
                    }
                }
            }
        }

        public string? CurrentTime
        {
            get => currentTime;
            set
            {
                if (currentTime != value)
                {
                    currentTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? TotalTime
        {
            get => totalTime;
            set
            {
                if (totalTime != value)
                {
                    totalTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Volume
        {
            get => volume;
            set
            {
                if (Math.Abs(volume - value) > 0.001)
                {
                    volume = value;
                    OnPropertyChanged();
                    if (outputDevice != null)
                    {
                        outputDevice.Volume = (float)value;
                    }
                }
            }
        }

        public bool IsUpdatingPosition
        {
            get => isUpdatingPosition;
            set
            {
                if (isUpdatingPosition != value)
                {
                    isUpdatingPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenFileCommand { get; }

        public MainViewModel()
        {
            PlayCommand = new RelayCommand(_ => Play(), _ => audioFile != null && !IsPlaying);
            PauseCommand = new RelayCommand(_ => Pause(), _ => IsPlaying);
            StopCommand = new RelayCommand(_ => Stop(), _ => audioFile != null);
            OpenFileCommand = new RelayCommand(_ => OpenFile());

            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OnPlaybackStopped;
        }

        public void Play()
        {
            if (audioFile == null || outputDevice == null) return;

            outputDevice.Play();
            IsPlaying = true;
            StartTimer();
        }

        public void Pause()
        {
            if (audioFile == null || outputDevice == null) return;

            outputDevice.Pause();
            IsPlaying = false;
            StopTimer();
        }

        public void Stop()
        {
            if (audioFile == null || outputDevice == null) return;

            outputDevice.Stop();
            IsPlaying = false;
            StopTimer();
            audioFile.Position = 0;
            UpdatePosition();
        }

        public void OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Stop();
                DisposeAudio();

                audioFile = new AudioFileReader(openFileDialog.FileName);
                outputDevice?.Init(audioFile);
                Volume = 1.0;
                UpdatePosition();
                UpdateTotalTime();
            }
        }

        public void SetPosition(double position)
        {
            if (audioFile == null) return;

            var newPosition = (long)(audioFile.Length * position);
            audioFile.Position = newPosition;
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (audioFile == null) return;

            IsUpdatingPosition = true;
            CurrentPosition = (double)audioFile.Position / audioFile.Length;
            CurrentTime = TimeSpan.FromSeconds(audioFile.CurrentTime.TotalSeconds).ToString(@"mm\:ss");
            IsUpdatingPosition = false;
        }

        private void UpdateTotalTime()
        {
            if (audioFile == null) return;

            TotalTime = TimeSpan.FromSeconds(audioFile.TotalTime.TotalSeconds).ToString(@"mm\:ss");
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            IsPlaying = false;
            StopTimer();
        }

        private System.Timers.Timer? timer;

        private void StartTimer()
        {
            timer = new System.Timers.Timer(100);
            timer.Elapsed += (sender, e) => UpdatePosition();
            timer.Start();
        }

        private void StopTimer()
        {
            timer?.Stop();
            timer?.Dispose();
            timer = null;
        }

        private void DisposeAudio()
        {
            audioFile?.Dispose();
            audioFile = null;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}