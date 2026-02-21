using NextBala.Commands;
using NextBala.Models;
using NextBala.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NextBala.ViewModels
{
    public class PedidoViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly PedidoService _service;
        private System.Timers.Timer _timer;

        // Pedido atual
        private Pedido _pedidoAtual;
        public Pedido PedidoAtual
        {
            get => _pedidoAtual;
            set { _pedidoAtual = value; OnPropertyChanged(); }
        }

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
        public string Marca
        {
            get => _marca;
            set { _marca = value; OnPropertyChanged(); }
        }

        private string _modelo;
        public string Modelo
        {
            get => _modelo;
            set { _modelo = value; OnPropertyChanged(); }
        }

        private string _defeito;
        public string Defeito
        {
            get => _defeito;
            set { _defeito = value; OnPropertyChanged(); }
        }

        private string _tecnico;
        public string Tecnico
        {
            get => _tecnico;
            set { _tecnico = value; OnPropertyChanged(); }
        }

        private string _preco;
        public string Preco
        {
            get => _preco;
            set { _preco = value; OnPropertyChanged(); }
        }

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

        // Listas principais
        public ObservableCollection<ItemPedido> Itens { get; set; } = new();
        public ObservableCollection<Pedido> Pedidos { get; set; } = new();
        public ObservableCollection<Pedido> PedidosFiltrados { get; set; } = new();

        // Lista de técnicos para filtro
        private ObservableCollection<string> _tecnicosLista;
        public ObservableCollection<string> TecnicosLista
        {
            get => _tecnicosLista;
            set { _tecnicosLista = value; OnPropertyChanged(); }
        }

        // Propriedades para filtros
        private string _tecnicoFiltro;
        public string TecnicoFiltro
        {
            get => _tecnicoFiltro;
            set
            {
                _tecnicoFiltro = value;
                OnPropertyChanged();
                AplicarFiltros();
            }
        }

        private string _dataTextoFiltro;
        public string DataTextoFiltro
        {
            get => _dataTextoFiltro;
            set
            {
                if (_dataTextoFiltro != value)
                {
                    _dataTextoFiltro = value;
                    OnPropertyChanged();

                    // Tenta converter quando tem 10 caracteres (dd/MM/yyyy)
                    if (!string.IsNullOrWhiteSpace(value) && value.Length == 10)
                    {
                        if (DateTime.TryParseExact(value, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data))
                        {
                            DataFiltro = data;
                            FiltroHoje = false;
                            FiltroMes = false;
                        }
                        else
                        {
                            // Data inválida - limpa o filtro
                            DataFiltro = null;
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(value))
                    {
                        DataFiltro = null;
                    }
                }
            }
        }

        private DateTime? _dataFiltro;
        public DateTime? DataFiltro
        {
            get => _dataFiltro;
            set
            {
                _dataFiltro = value;
                OnPropertyChanged();
                AplicarFiltros();
            }
        }

        private bool _filtroHoje;
        public bool FiltroHoje
        {
            get => _filtroHoje;
            set
            {
                _filtroHoje = value;
                OnPropertyChanged();
                if (value)
                {
                    DataTextoFiltro = DateTime.Today.ToString("dd/MM/yyyy");
                    DataFiltro = DateTime.Today;
                    FiltroMes = false;
                }
            }
        }

        private bool _filtroMes;
        public bool FiltroMes
        {
            get => _filtroMes;
            set
            {
                _filtroMes = value;
                OnPropertyChanged();
                if (value)
                {
                    DataTextoFiltro = string.Empty;
                    DataFiltro = null;
                    FiltroHoje = false;
                    AplicarFiltros();
                }
            }
        }

        private string _resultadosFiltro;
        public string ResultadosFiltro
        {
            get => _resultadosFiltro;
            set { _resultadosFiltro = value; OnPropertyChanged(); }
        }

        // Comandos
        public RelayCommand AdicionarItemCommand { get; set; }
        public RelayCommand SalvarPedidoCommand { get; set; }
        public RelayCommand GerarTicketCommand { get; set; }
        public RelayCommand NovoPedidoCommand { get; set; }
        public RelayCommand LimparFiltrosCommand { get; set; }
        public RelayCommand AplicarFiltroDataCommand { get; set; }
        public RelayCommand FiltrarHojeCommand { get; set; }
        public RelayCommand FiltrarMesCommand { get; set; }

        // Comando para cancelar pedido
        public RelayCommand<Pedido> CancelarPedidoCommand { get; set; }

        public PedidoViewModel()
        {
            var db = new Data.AppDbContext();
            _service = new PedidoService(db);

            PedidoAtual = new Pedido();
            TecnicosLista = new ObservableCollection<string>();

            // Carrega pedidos existentes
            CarregarPedidos();

            // Comandos sem parâmetro
            AdicionarItemCommand = new RelayCommand(AdicionarItem);
            SalvarPedidoCommand = new RelayCommand(SalvarPedido);
            GerarTicketCommand = new RelayCommand(GerarTicket);
            NovoPedidoCommand = new RelayCommand(NovoPedido);
            LimparFiltrosCommand = new RelayCommand(LimparFiltros);
            AplicarFiltroDataCommand = new RelayCommand(AplicarFiltroData);
            FiltrarHojeCommand = new RelayCommand(FiltrarHoje);
            FiltrarMesCommand = new RelayCommand(FiltrarMes);

            // Comando com parâmetro
            CancelarPedidoCommand = new RelayCommand<Pedido>(CancelarPedido);

            // Inicializar data atual
            DataAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // Iniciar relógio em tempo real
            IniciarRelogio();

            // Inicializar filtro com "Todos"
            TecnicoFiltro = "Todos";
        }

        // ========== MÉTODOS PRINCIPAIS ==========

        private void CarregarPedidos()
        {
            try
            {
                Pedidos.Clear();
                foreach (var p in _service.ObterPedidos())
                {
                    Pedidos.Add(p);
                }
                AtualizarPedidosFiltrados();
                CarregarTecnicos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar pedidos: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CarregarTecnicos()
        {
            try
            {
                var tecnicos = Pedidos
                    .SelectMany(p => p.Itens)
                    .Select(i => i.Tecnico)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                TecnicosLista.Clear();
                TecnicosLista.Add("Todos");
                foreach (var t in tecnicos)
                {
                    TecnicosLista.Add(t);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar técnicos: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            AtualizarPedidosFiltrados();
        }

        private void AplicarFiltroData()
        {
            AplicarFiltros();
        }

        private void FiltrarHoje()
        {
            FiltroHoje = true;
        }

        private void FiltrarMes()
        {
            FiltroMes = true;
        }

        private void AtualizarPedidosFiltrados()
        {
            try
            {
                var query = Pedidos.AsEnumerable();

                // Filtro por técnico
                if (!string.IsNullOrEmpty(TecnicoFiltro) && TecnicoFiltro != "Todos")
                {
                    query = query.Where(p => p.Itens != null &&
                                           p.Itens.Any(i => i.Tecnico == TecnicoFiltro));
                }

                // Filtro por data específica
                if (DataFiltro.HasValue)
                {
                    query = query.Where(p => p.Data.Date == DataFiltro.Value.Date);
                }
                // Filtro por mês atual
                else if (FiltroMes)
                {
                    var hoje = DateTime.Today;
                    query = query.Where(p => p.Data.Year == hoje.Year &&
                                           p.Data.Month == hoje.Month);
                }

                var listaFiltrada = query.OrderByDescending(p => p.Data).ToList();

                PedidosFiltrados.Clear();
                foreach (var pedido in listaFiltrada)
                {
                    PedidosFiltrados.Add(pedido);
                }

                ResultadosFiltro = $"Mostrando {PedidosFiltrados.Count} de {Pedidos.Count} pedidos";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao filtrar pedidos: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimparFiltros()
        {
            TecnicoFiltro = "Todos";
            DataTextoFiltro = string.Empty;
            DataFiltro = null;
            FiltroHoje = false;
            FiltroMes = false;
            AplicarFiltros();
        }

        private void IniciarRelogio()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataAtual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                });
            };
            _timer.Start();
        }

        // ========== MÉTODOS DE NEGÓCIO ==========

        private void AdicionarItem()
        {
            if (string.IsNullOrWhiteSpace(Marca))
            {
                MessageBox.Show("Preencha a marca do item.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Modelo))
            {
                MessageBox.Show("Preencha o modelo do item.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(Preco, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal preco))
            {
                MessageBox.Show("Preço inválido. Use apenas números (ex: 15000.50)", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var item = new ItemPedido
            {
                Marca = Marca,
                Modelo = Modelo,
                Defeito = Defeito ?? string.Empty,
                Tecnico = string.IsNullOrWhiteSpace(Tecnico) ? "Não atribuído" : Tecnico,
                Preco = preco
            };

            Itens.Add(item);
            PedidoAtual.Itens.Add(item);

            Marca = string.Empty;
            Modelo = string.Empty;
            Defeito = string.Empty;
            Tecnico = string.Empty;
            Preco = string.Empty;

            if (PedidoAtual.Id > 0)
            {
                Ticket = _service.GerarTicket(PedidoAtual);
            }
        }

        private int ObterProximoNumeroPedido()
        {
            var hoje = DateTime.Today;

            var pedidosDeHoje = Pedidos
                .Where(p => p.Data.Date == hoje)
                .ToList();

            if (!pedidosDeHoje.Any())
                return 1;

            var maiorNumeroHoje = pedidosDeHoje.Max(p => p.NumeroPedido);
            return maiorNumeroHoje + 1;
        }

        private void NovoPedido()
        {
            var result = MessageBox.Show("Iniciar novo pedido? Os dados não salvos serão perdidos.",
                "Novo Pedido", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                PedidoAtual = new Pedido();
                Itens.Clear();
                ClienteNome = string.Empty;
                ClienteTelefone = string.Empty;
                Ticket = string.Empty;
            }
        }

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
                // Converter cliente para Pascal Case
                ClienteNome = ToPascalCase(ClienteNome.Trim());

                Cliente clienteExistente = null;

                if (!string.IsNullOrWhiteSpace(ClienteTelefone))
                {
                    clienteExistente = _service.ObterClientePorTelefone(ClienteTelefone);
                }

                if (clienteExistente != null)
                {
                    PedidoAtual.Cliente = clienteExistente;
                    PedidoAtual.ClienteId = clienteExistente.Id;

                    if (clienteExistente.Nome != ClienteNome)
                    {
                        clienteExistente.Nome = ClienteNome;
                        _service.AtualizarCliente(clienteExistente);
                    }
                }
                else
                {
                    var novoCliente = new Cliente
                    {
                        Nome = ClienteNome,
                        Telefone = ClienteTelefone ?? string.Empty
                    };

                    _service.AdicionarCliente(novoCliente);
                    PedidoAtual.Cliente = novoCliente;
                    PedidoAtual.ClienteId = novoCliente.Id;
                }

                // Converter técnico de cada item para Pascal Case
                foreach (var item in Itens)
                {
                    if (!string.IsNullOrWhiteSpace(item.Tecnico))
                    {
                        item.Tecnico = ToPascalCase(item.Tecnico.Trim());
                    }
                }

                PedidoAtual.NumeroPedido = ObterProximoNumeroPedido();
                PedidoAtual.Data = DateTime.Now;
                PedidoAtual.Status = "Ativo";

                _service.AdicionarPedido(PedidoAtual);

                Pedidos.Add(PedidoAtual);

                string ticketGerado = _service.GerarTicket(PedidoAtual);
                Ticket = ticketGerado;

                ImprimirTicket(ticketGerado, 2);

                AtualizarPedidosFiltrados();
                CarregarTecnicos();

                var pedidoSalvo = PedidoAtual;

                PedidoAtual = new Pedido();
                Itens.Clear();
                ClienteNome = string.Empty;
                ClienteTelefone = string.Empty;
                Ticket = string.Empty;

                MessageBox.Show($"Pedido #{pedidoSalvo.NumeroPedido} salvo e ticket impresso com sucesso!",
                    "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar pedido: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ToPascalCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Divide o texto em palavras
            var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    // Primeira letra maiúscula, resto minúsculo
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            return string.Join(" ", words);
        }

        private void CancelarPedido(Pedido? pedido)
        {
            if (pedido == null) return;

            var result = MessageBox.Show(
                $"Tem certeza que deseja cancelar o pedido #{pedido.NumeroPedido} do cliente {pedido.Cliente?.Nome}?",
                "Confirmar Cancelamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _service.CancelarPedido(pedido.Id);

                    pedido.Status = "Cancelado";

                    var index = Pedidos.IndexOf(pedido);
                    if (index >= 0)
                    {
                        Pedidos[index] = pedido;
                    }

                    AtualizarPedidosFiltrados();

                    MessageBox.Show($"Pedido #{pedido.NumeroPedido} cancelado com sucesso!",
                        "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao cancelar pedido: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

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

        // ========== IMPLEMENTAÇÃO DE INotifyPropertyChanged ==========

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        // ========== IMPLEMENTAÇÃO DE IDisposable ==========

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}