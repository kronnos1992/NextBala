using NextBala.ViewModels;
using System.Windows;

namespace NextBala
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new PedidoViewModel();
        }
    }
}
