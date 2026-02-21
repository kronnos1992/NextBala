using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace NextBala
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite apenas dígitos
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]$");
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Permite teclas de controle
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab ||
                e.Key == Key.Enter || e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Home || e.Key == Key.End)
            {
                return;
            }

            // Permite apenas dígitos
            if (e.Key < Key.D0 || e.Key > Key.D9)
            {
                if (e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
                {
                    e.Handled = true;
                }
            }
        }
    }
}