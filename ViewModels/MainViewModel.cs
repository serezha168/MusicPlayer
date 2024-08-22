using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Wave;

namespace MusicPlayer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private AudioFileReader _audioFileReader;
        private WaveOutEvent _waveOutEvent;
        private DispatcherTimer _timer;
        private Track _selectedTrack;
        private bool _isPlaying;
        private double _currentPosition;
        private double _totalDuration;
        private float _volume = 1.0f;
        private string _currentTime;
        private string _totalTime;
        private string _musicFolderPath;

        public ObservableCollection<Track> Tracks { get; set; }
        public Track SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                _selectedTrack = value;
                OnPropertyChanged(nameof(SelectedTrack));
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        public double CurrentPosition
        {
            get => _currentPosition;
            set
            {
                _currentPosition = value;
                OnPropertyChanged(nameof(CurrentPosition));
            }
        }

        public double TotalDuration
        {
            get => _totalDuration;
            set
            {
                _totalDuration = value;
                OnPropertyChanged(nameof(TotalDuration));
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged(nameof(Volume));
                if (_waveOutEvent != null)
                {
                    _waveOutEvent.Volume = _volume;
                }
            }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged(nameof(CurrentTime));
            }
        }

        public string TotalTime
        {
            get => _totalTime;
            set
            {
                _totalTime = value;
                OnPropertyChanged(nameof(TotalTime));
            }
        }

        public string MusicFolderPath
        {
            get => _musicFolderPath;
            set
            {
                _musicFolderPath = value;
                OnPropertyChanged(nameof(MusicFolderPath));
                LoadTracks();
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand SelectFolderCommand { get; }

        public MainViewModel()
        {
            Tracks = new ObservableCollection<Track>();
            PlayCommand = new RelayCommand(Play, CanPlay);
            StopCommand = new RelayCommand(Stop, CanStop);
            PauseCommand = new RelayCommand(Pause, CanPause);
            SelectFolderCommand = new RelayCommand(SelectFolder);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _timer.Tick += Timer_Tick;
        }

        private void SelectFolder()
        {
            var dialog = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Выберите папку",
                Title = "Выберите папку с музыкой"
            };

            if (dialog.ShowDialog() == true)
            {
                MusicFolderPath = Path.GetDirectoryName(dialog.FileName);
            }
        }

        private void LoadTracks()
        {
            if (string.IsNullOrEmpty(MusicFolderPath) || !Directory.Exists(MusicFolderPath))
            {
                return;
            }

            Tracks.Clear();
            var files = Directory.GetFiles(MusicFolderPath, "*.mp3");
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                Tracks.Add(new Track
                {
                    Title = Path.GetFileNameWithoutExtension(file),
                    FilePath = file
                });
            }
        }

        private void Play()
        {
            if (SelectedTrack == null) return;

            if (_waveOutEvent == null)
            {
                _waveOutEvent = new WaveOutEvent();
                _audioFileReader = new AudioFileReader(SelectedTrack.FilePath);
                _waveOutEvent.Init(_audioFileReader);
            }

            _waveOutEvent.Play();
            IsPlaying = true;
            _timer.Start();

            TotalDuration = _audioFileReader.TotalTime.TotalSeconds;
            TotalTime = _audioFileReader.TotalTime.ToString(@"mm\:ss");
        }

        private void Stop()
        {
            _waveOutEvent?.Stop();
            _audioFileReader?.Dispose();
            _waveOutEvent?.Dispose();
            _waveOutEvent = null;
            _audioFileReader = null;
            IsPlaying = false;
            _timer.Stop();
            CurrentPosition = 0;
            CurrentTime = "00:00";
        }

        private void Pause()
        {
            if (_waveOutEvent?.PlaybackState == PlaybackState.Playing)
            {
                _waveOutEvent.Pause();
                IsPlaying = false;
                _timer.Stop();
            }
            else if (_waveOutEvent?.PlaybackState == PlaybackState.Paused)
            {
                _waveOutEvent.Play();
                IsPlaying = true;
                _timer.Start();
            }
        }

        private bool CanPlay() => SelectedTrack != null && !IsPlaying;
        private bool CanStop() => IsPlaying;
        private bool CanPause() => IsPlaying || (_waveOutEvent?.PlaybackState == PlaybackState.Paused);

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_audioFileReader != null)
            {
                CurrentPosition = _audioFileReader.CurrentTime.TotalSeconds;
                CurrentTime = _audioFileReader.CurrentTime.ToString(@"mm\:ss");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Track
    {
        public string Title { get; set; }
        public string FilePath { get; set; }
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

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}