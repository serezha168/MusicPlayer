// Views/MainWindow.xaml.cs
using System.Windows;
using MusicPlayer;

namespace MusicPlayer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Создаем экземпляр AudioPlayerService
            var audioPlayerService = new AudioPlayerService();

            // Создаем экземпляр MainViewModel, передавая ему AudioPlayerService
            var viewModel = new MainViewModel(audioPlayerService);

            // Устанавливаем DataContext для окна
            DataContext = viewModel;
        }
    }
}
