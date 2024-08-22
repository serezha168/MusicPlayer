using MusicPlayer.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MusicPlayer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AudioPlayerService _audioPlayerService;
        private ObservableCollection<Track> _tracks;
        private Track _selectedTrack;
        private bool _isPlaying;

        public ObservableCollection<Track> Tracks
        {
            get => _tracks;
            set
            {
                _tracks = value;
                OnPropertyChanged();
            }
        }

        public Track SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                _selectedTrack = value;
                OnPropertyChanged();
                (PlayCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
                (PlayCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (PauseCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (StopCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }

        public MainViewModel(AudioPlayerService audioPlayerService)
        {
            _audioPlayerService = audioPlayerService;
            Tracks = new ObservableCollection<Track>();
            PlayCommand = new RelayCommand(Play, CanPlay);
            PauseCommand = new RelayCommand(Pause, CanPause);
            StopCommand = new RelayCommand(Stop, CanStop);
            LoadTracks();
        }

        private void Play()
        {
            if (SelectedTrack != null)
            {
                _audioPlayerService.Play(SelectedTrack.FilePath);
                IsPlaying = true;
            }
        }

        private void Pause()
        {
            _audioPlayerService.Pause();
            IsPlaying = false;
        }

        private void Stop()
        {
            _audioPlayerService.Stop();
            IsPlaying = false;
        }

        private bool CanPlay() => SelectedTrack != null && !IsPlaying;
        private bool CanPause() => IsPlaying;
        private bool CanStop() => IsPlaying;

        private void LoadTracks()
        {
            string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string[] supportedExtensions = { ".mp3", ".wav", ".ogg" };

            var files = Directory.GetFiles(musicFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()));

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                Tracks.Add(new Track
                {
                    Title = Path.GetFileNameWithoutExtension(file),
                    Artist = "Unknown", // ¬ будущем можно добавить чтение метаданных
                    FilePath = file
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class Track
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }
        public TimeSpan Duration { get; set; }
    }
}