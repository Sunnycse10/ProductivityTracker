using ProductivityTracker.ViewModels;
using System.Windows;


namespace ProductivityTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainViewModel ViewModel { get; set; } = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();
            
        }

        
    }
}