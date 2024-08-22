using System.Windows;
using System.Windows.Controls;

namespace MusicPlayer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DataContext is MainViewModel viewModel && !viewModel.IsUpdatingPosition)
            {
                viewModel.SetPosition(e.NewValue);
            }
        }
    }
}