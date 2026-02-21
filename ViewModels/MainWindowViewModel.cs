using NextBala.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NextBala.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        private string _dataAtual;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public string DataAtual
        {
            get => _dataAtual;
            set { _dataAtual = value; OnPropertyChanged(); }
        }

        public RelayCommand NavigateToDashboardCommand { get; set; }
        public RelayCommand NavigateToPedidosCommand { get; set; }

        public MainWindowViewModel()
        {
            // Inicializa com Dashboard
            CurrentView = new DashboardViewModel();

            // Atualiza data/hora
            DataAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // Comandos de navegação
            NavigateToDashboardCommand = new RelayCommand(NavigateToDashboard);
            NavigateToPedidosCommand = new RelayCommand(NavigateToPedidos);
        }

        private void NavigateToDashboard()
        {
            CurrentView = new DashboardViewModel();
        }

        private void NavigateToPedidos()
        {
            CurrentView = new PedidoViewModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}