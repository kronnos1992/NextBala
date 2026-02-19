using NextBala.Commands;
using NextBala.Models;
using NextBala.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NextBala.ViewModels
{
    public class PedidoViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly PedidoService _service;
        private System.Timers.Timer _timer;

        // Pedido atual
        public Pedido PedidoAtual { get; set; } = new Pedido();

        // Campos do cliente
        private string _clienteNome;
        public string ClienteNome
        {
            get => _clienteNome;
            set { _clienteNome = value; OnPropertyChanged(); }
        }

        private string _clienteTelefone;
        public string ClienteTelefone
        {
            get => _clienteTelefone;
            set { _clienteTelefone = value; OnPropertyChanged(); }
        }

        // Campos do item
        private string _marca;
        public string Marca { get => _marca; set { _marca = value; OnPropertyChanged(); } }

        private string _modelo;
        public string Modelo { get => _modelo; set { _modelo = value; OnPropertyChanged(); } }

        private string _defeito;
        public string Defeito { get => _defeito; set { _defeito = value; OnPropertyChanged(); } }

        private string _tecnico;
        public string Tecnico { get => _tecnico; set { _tecnico = value; OnPropertyChanged(); } }

        private string _preco;
        public string Preco { get => _preco; set { _preco = value; OnPropertyChanged(); } }

        private string _ticket;
        public string Ticket
        {
            get => _ticket;
            set { _ticket = value; OnPropertyChanged(); }
        }

        // Data e Hora atual
        private string _dataAtual;
        public string DataAtual
        {
            get => _dataAtual;
            set { _dataAtual = value; OnPropertyChanged(); }
        }

        // Listas
        public ObservableCollection<ItemPedido> Itens { get; set; } = new();
        public ObservableCollection<Pedido> Pedidos { get; set; } = new();

        // Comandos
        public RelayCommand AdicionarItemCommand { get; set; }
        public RelayCommand SalvarPedidoCommand { get; set; }
        public RelayCommand GerarTicketCommand { get; set; }

        public PedidoViewModel()
        {
            var db = new Data.AppDbContext();
            _service = new PedidoService(db);

            // Carrega pedidos existentes
            CarregarPedidos();

            // Comandos
            AdicionarItemCommand = new RelayCommand(AdicionarItem);
            SalvarPedidoCommand = new RelayCommand(SalvarPedido);
            GerarTicketCommand = new RelayCommand(GerarTicket);

            // Inicializar data atual
            DataAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // Iniciar relógio em tempo real
            IniciarRelogio();
        }

        // Método para carregar pedidos
        private void CarregarPedidos()
        {
            Pedidos.Clear();
            foreach (var p in _service.ObterPedidos())
            {
                // Garantir que a propriedade calculada funcione
                Pedidos.Add(p);
            }
        }

        // Inicia o timer para atualizar o relógio a cada segundo
        private void IniciarRelogio()
        {
            _timer = new System.Timers.Timer(1000); // 1 segundo
            _timer.Elapsed += (s, e) =>
            {
                // Atualizar na UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                });
            };
            _timer.Start();
        }

        // Adiciona um item ao pedido
        private void AdicionarItem()
        {
            if (string.IsNullOrWhiteSpace(Marca) || string.IsNullOrWhiteSpace(Modelo))
            {
                MessageBox.Show("Preencha marca e modelo do item.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(Preco, out decimal preco))
            {
                MessageBox.Show("Preço inválido.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var item = new ItemPedido
            {
                Marca = Marca,
                Modelo = Modelo,
                Defeito = Defeito,
                Tecnico = Tecnico,
                Preco = preco
            };

            Itens.Add(item);
            PedidoAtual.Itens.Add(item);

            // limpa campos
            Marca = Modelo = Defeito = Tecnico = Preco = string.Empty;
        }

        // Salva pedido no banco e já gera/imprime o ticket
        private void SalvarPedido()
        {
            if (string.IsNullOrWhiteSpace(ClienteNome))
            {
                MessageBox.Show("Informe o nome do cliente.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Itens.Any())
            {
                MessageBox.Show("Adicione pelo menos um item ao pedido.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var cliente = new Cliente
                {
                    Nome = ClienteNome,
                    Telefone = ClienteTelefone
                };

                _service.AdicionarCliente(cliente);

                PedidoAtual.Cliente = cliente;
                PedidoAtual.NumeroPedido = Pedidos.Count + 1;
                PedidoAtual.Data = DateTime.Now;

                _service.AdicionarPedido(PedidoAtual);

                // Adicionar à lista visível
                Pedidos.Add(PedidoAtual);

                // Disparar notificação para atualizar a propriedade calculada na UI
                OnPropertyChanged(nameof(Pedidos));

                // GERAR E IMPRIMIR TICKET AUTOMATICAMENTE
                string ticketGerado = _service.GerarTicket(PedidoAtual);
                Ticket = ticketGerado;

                // Imprimir duas cópias do ticket
                ImprimirTicket(ticketGerado, 2);

                // Preparar para novo pedido
                PedidoAtual = new Pedido();
                Itens.Clear();
                ClienteNome = string.Empty;
                ClienteTelefone = string.Empty;
                Ticket = string.Empty;

                MessageBox.Show("Pedido salvo e ticket impresso com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar pedido: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Gera ticket manualmente (opcional)
        private void GerarTicket()
        {
            if (PedidoAtual == null || PedidoAtual.Id == 0)
            {
                MessageBox.Show("Salve o pedido antes de gerar o ticket.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Ticket = _service.GerarTicket(PedidoAtual);

            var result = MessageBox.Show("Deseja imprimir o ticket?", "Impressão",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ImprimirTicket(Ticket, 2);
            }
        }

        // Imprime tickets
        private static void ImprimirTicket(string ticketTexto, int copias = 1)
        {
            try
            {
                PrintDocument pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = "Xprinter XP-235B";

                int largura = 280;   // ~58mm
                int altura = 120;    // Aumentei a altura para acomodar mais informações

                pd.DefaultPageSettings.PaperSize = new PaperSize("Ticket", largura, altura);

                int copiaAtual = 0;

                pd.PrintPage += (sender, e) =>
                {
                    using var font = new System.Drawing.Font("Consolas", 9, System.Drawing.FontStyle.Bold);

                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near
                    };

                    var area = new RectangleF(0, 0, largura, altura);

                    // Desenha o ticket
                    e.Graphics.DrawString(ticketTexto, font, Brushes.Black, area, format);

                    copiaAtual++;
                    e.HasMorePages = copiaAtual < copias;
                };

                pd.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao imprimir ticket: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        // Implementar IDisposable para limpar o timer
        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}