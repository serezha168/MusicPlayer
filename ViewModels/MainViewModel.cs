using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Wave;
using TagLib;

namespace MusicPlayer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private IWavePlayer _waveOutDevice;
        private AudioFileReader _audioFileReader;
        private readonly DispatcherTimer _timer;
        private MusicFile _selectedMusicFile;
        private bool _isPlaying;
        private double _currentPosition;
        private string _currentTime;
        private string _totalTime;
        private float _volume = 1.0f;
        private string _currentTrackInfo;

        public MainViewModel()
        {
            MusicFiles = new ObservableCollection<MusicFile>();

            PlayCommand = new RelayCommand(Play, CanPlay);
            PauseCommand = new RelayCommand(Pause, CanPause);
            StopCommand = new RelayCommand(Stop, CanStop);
            OpenFileCommand = new RelayCommand(OpenFile);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;

            InitializeAudioDevice();
        }

        private void InitializeAudioDevice()
        {
            try
            {
                _waveOutDevice = new WaveOutEvent();
                _waveOutDevice.PlaybackStopped += OnPlaybackStopped;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации аудио: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MusicFile> MusicFiles { get; }

        public MusicFile SelectedMusicFile
        {
            get => _selectedMusicFile;
            set
            {
                if (_selectedMusicFile != value)
                {
                    _selectedMusicFile = value;
                    OnPropertyChanged();
                    (PlayCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged();
                    (PlayCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (PauseCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (StopCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public double CurrentPosition
        {
            get => _currentPosition;
            set
            {
                if (Math.Abs(_currentPosition - value) > 0.001)
                {
                    _currentPosition = value;
                    OnPropertyChanged();
                    if (_audioFileReader != null)
                    {
                        _audioFileReader.Position = (long)(_audioFileReader.Length * value);
                    }
                }
            }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                if (_currentTime != value)
                {
                    _currentTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TotalTime
        {
            get => _totalTime;
            set
            {
                if (_totalTime != value)
                {
                    _totalTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                if (Math.Abs(_volume - value) > 0.001)
                {
                    _volume = value;
                    OnPropertyChanged();
                    if (_waveOutDevice != null)
                    {
                        _waveOutDevice.Volume = value;
                    }
                }
            }
        }

        public string CurrentTrackInfo
        {
            get => _currentTrackInfo;
            set
            {
                if (_currentTrackInfo != value)
                {
                    _currentTrackInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenFileCommand { get; }

        private void Play()
        {
            if (SelectedMusicFile != null)
            {
                try
                {
                    if (System.IO.File.Exists(SelectedMusicFile.FilePath))
                    {
                        if (_audioFileReader == null || _audioFileReader.FileName != SelectedMusicFile.FilePath)
                        {
                            Stop();
                            _audioFileReader = new AudioFileReader(SelectedMusicFile.FilePath);
                            _waveOutDevice.Init(_audioFileReader);
                        }

                        _waveOutDevice.Play();
                        IsPlaying = true;
                        UpdateTrackInfo(SelectedMusicFile.FilePath);
                        _timer.Start();
                    }
                    else
                    {
                        MessageBox.Show($"Файл не найден: {SelectedMusicFile.FilePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при воспроизведении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanPlay() => SelectedMusicFile != null && !IsPlaying;

        private void Pause()
        {
            _waveOutDevice?.Pause();
            IsPlaying = false;
            _timer.Stop();
        }

        private bool CanPause() => IsPlaying;

        private void Stop()
        {
            _waveOutDevice?.Stop();
            if (_audioFileReader != null)
            {
                _audioFileReader.Position = 0;
            }
            IsPlaying = false;
            CurrentPosition = 0;
            CurrentTime = "00:00";
            _timer.Stop();
        }

        private bool CanStop() => IsPlaying;

        private void OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    MusicFiles.Add(new MusicFile { Name = System.IO.Path.GetFileName(fileName), FilePath = fileName });
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (IsPlaying && _audioFileReader != null)
            {
                CurrentPosition = (double)_audioFileReader.Position / _audioFileReader.Length;
                CurrentTime = TimeSpan.FromSeconds(_audioFileReader.CurrentTime.TotalSeconds).ToString(@"mm\:ss");
                TotalTime = TimeSpan.FromSeconds(_audioFileReader.TotalTime.TotalSeconds).ToString(@"mm\:ss");
            }
        }

        private void UpdateTrackInfo(string filePath)
        {
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    string artist = string.Join(", ", file.Tag.Performers);
                    string title = file.Tag.Title;
                    CurrentTrackInfo = $"{artist} - {title}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении метаданных: {ex.Message}");
                CurrentTrackInfo = System.IO.Path.GetFileNameWithoutExtension(filePath);
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            IsPlaying = false;
            _timer.Stop();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MusicFile
    {
        public string Name { get; set; }
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

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}