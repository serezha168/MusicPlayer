using NAudio.Wave;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace MusicPlayer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFile;
        private DispatcherTimer _timer;
        private string _currentFile;
        private bool _isPlaying;
        private double _currentPosition;
        private string _currentTime;
        private string _totalTime;
        private double _volume = 0.5;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenFileCommand { get; }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
            }
        }

        public double CurrentPosition
        {
            get => _currentPosition;
            set
            {
                _currentPosition = value;
                OnPropertyChanged();
            }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }

        public string TotalTime
        {
            get => _totalTime;
            set
            {
                _totalTime = value;
                OnPropertyChanged();
            }
        }

        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged();
                if (_outputDevice != null)
                    _outputDevice.Volume = (float)_volume;
            }
        }

        public bool IsUpdatingPosition { get; private set; }

        public double TotalDuration => _audioFile?.TotalTime.TotalSeconds ?? 0;

        public MainViewModel()
        {
            PlayCommand = new RelayCommand(Play, CanPlay);
            PauseCommand = new RelayCommand(Pause, CanPause);
            StopCommand = new RelayCommand(Stop, CanStop);
            OpenFileCommand = new RelayCommand(OpenFile);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_outputDevice != null && _audioFile != null)
            {
                IsUpdatingPosition = true;
                CurrentPosition = (double)_audioFile.Position / _audioFile.Length;
                CurrentTime = TimeSpan.FromSeconds(CurrentPosition * TotalDuration).ToString(@"mm\:ss");
                IsUpdatingPosition = false;
            }
        }

        private void Play()
        {
            if (_outputDevice == null)
            {
                _outputDevice = new WaveOutEvent();
                _outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (_audioFile == null)
            {
                _audioFile = new AudioFileReader(_currentFile);
                _outputDevice.Init(_audioFile);
            }
            _outputDevice.Play();
            IsPlaying = true;
            _timer.Start();
        }

        private void Pause()
        {
            _outputDevice?.Pause();
            IsPlaying = false;
            _timer.Stop();
        }

        private void Stop()
        {
            _outputDevice?.Stop();
            IsPlaying = false;
            _timer.Stop();
            if (_audioFile != null)
            {
                _audioFile.Position = 0;
            }
            UpdatePosition();
        }

        private void OpenFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _currentFile = openFileDialog.FileName;
                Stop();
                DisposeWave();
                Play();
                TotalTime = TimeSpan.FromSeconds(TotalDuration).ToString(@"mm\:ss");
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            DisposeWave();
            IsPlaying = false;
            _timer.Stop();
        }

        private void DisposeWave()
        {
            _outputDevice?.Dispose();
            _outputDevice = null;
            _audioFile?.Dispose();
            _audioFile = null;
        }

        private bool CanPlay() => !IsPlaying && !string.IsNullOrEmpty(_currentFile);
        private bool CanPause() => IsPlaying;
        private bool CanStop() => IsPlaying || (_outputDevice != null && _outputDevice.PlaybackState == PlaybackState.Paused);

        public void SetPosition(double position)
        {
            if (_outputDevice != null && _audioFile != null)
            {
                _audioFile.Position = (long)(position * _audioFile.Length);
                CurrentPosition = position;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter) => _execute();
    }
}