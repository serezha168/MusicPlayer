using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AudioPlayerService _audioPlayerService;
        private Track _selectedTrack;
        private bool _isPlaying;

        public MainViewModel()
        {
            _audioPlayerService = new AudioPlayerService();
            Tracks = new ObservableCollection<Track>();
            LoadTracks();

            PlayCommand = new RelayCommand(Play, CanPlay);
            StopCommand = new RelayCommand(Stop, CanStop);
        }

        public ObservableCollection<Track> Tracks { get; set; }

        public Track SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                if (_selectedTrack != value)
                {
                    _selectedTrack = value;
                    OnPropertyChanged(nameof(SelectedTrack));
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
                    OnPropertyChanged(nameof(IsPlaying));
                    (PlayCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (StopCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }

        private void LoadTracks()
        {
            // Здесь должна быть логика загрузки треков
            // Пример:
            Tracks.Add(new Track { Title = "Трек 1", Artist = "Исполнитель 1", FilePath = "path/to/track1.mp3" });
            Tracks.Add(new Track { Title = "Трек 2", Artist = "Исполнитель 2", FilePath = "path/to/track2.mp3" });
        }

        private void Play()
        {
            if (SelectedTrack != null)
            {
                _audioPlayerService.Play(SelectedTrack.FilePath);
                IsPlaying = true;
            }
        }

        private bool CanPlay()
        {
            return SelectedTrack != null && !IsPlaying;
        }

        private void Stop()
        {
            _audioPlayerService.Stop();
            IsPlaying = false;
        }

        private bool CanStop()
        {
            return IsPlaying;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
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

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
