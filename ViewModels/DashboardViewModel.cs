using NextBala.Commands;
using NextBala.Models;
using NextBala.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NextBala.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly PedidoService _service;

        #region CONTROLE PERFORMANCE

        private bool _performanceFiltrada;
        public bool PerformanceFiltrada
        {
            get => _performanceFiltrada;
            set { _performanceFiltrada = value; OnPropertyChanged(); }
        }

        #endregion

        #region CARDS HOJE

        private int _pedidosHoje;
        private decimal _faturamentoHoje;
        private decimal _ticketMedio;
        private int _itensEmServico;

        public int PedidosHoje { get => _pedidosHoje; set { _pedidosHoje = value; OnPropertyChanged(); } }
        public decimal FaturamentoHoje { get => _faturamentoHoje; set { _faturamentoHoje = value; OnPropertyChanged(); } }
        public decimal TicketMedio { get => _ticketMedio; set { _ticketMedio = value; OnPropertyChanged(); } }
        public int ItensEmServico { get => _itensEmServico; set { _itensEmServico = value; OnPropertyChanged(); } }

        #endregion

        #region CARDS FILTRADOS

        private int _pedidosFiltradosTotal;
        private decimal _faturamentoFiltrado;
        private decimal _ticketMedioFiltrado;
        private int _itensFiltrados;

        public int PedidosFiltradosTotal { get => _pedidosFiltradosTotal; set { _pedidosFiltradosTotal = value; OnPropertyChanged(); } }
        public decimal FaturamentoFiltrado { get => _faturamentoFiltrado; set { _faturamentoFiltrado = value; OnPropertyChanged(); } }
        public decimal TicketMedioFiltrado { get => _ticketMedioFiltrado; set { _ticketMedioFiltrado = value; OnPropertyChanged(); } }
        public int ItensFiltrados { get => _itensFiltrados; set { _itensFiltrados = value; OnPropertyChanged(); } }

        #endregion

        #region LISTAS

        public ObservableCollection<Pedido> Pedidos { get; set; }
        public ObservableCollection<Pedido> PedidosFiltrados { get; set; }
        public ObservableCollection<string> TecnicosLista { get; set; }
        private ObservableCollection<TecnicoPerformance> _tecnicosPerformance;
        public ObservableCollection<TecnicoPerformance> TecnicosPerformance
        {
            get => _tecnicosPerformance;
            set
            {
                _tecnicosPerformance = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region FILTROS

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

        // DATA INICIAL TEXTO
        private string _dataTextoFiltro;
        public string DataTextoFiltro
        {
            get => _dataTextoFiltro;
            set
            {
                _dataTextoFiltro = value;
                OnPropertyChanged();
                ProcessarData(value, true);
            }
        }

        // DATA FINAL TEXTO
        private string _dataTextoFinalFiltro;
        public string DataTextoFinalFiltro
        {
            get => _dataTextoFinalFiltro;
            set
            {
                _dataTextoFinalFiltro = value;
                OnPropertyChanged();
                ProcessarData(value, false);
            }
        }

        // DATA INICIAL
        private DateTime? _dataFiltro;
        public DateTime? DataFiltro
        {
            get => _dataFiltro;
            set
            {
                _dataFiltro = value;
                OnPropertyChanged();
                if (value.HasValue || _dataFinalFiltro.HasValue)
                    AplicarFiltros();
                else
                    AtualizarTudo();
            }
        }

        // DATA FINAL
        private DateTime? _dataFinalFiltro;
        public DateTime? DataFinalFiltro
        {
            get => _dataFinalFiltro;
            set
            {
                _dataFinalFiltro = value;
                OnPropertyChanged();
                if (value.HasValue || _dataFiltro.HasValue)
                    AplicarFiltros();
                else
                    AtualizarTudo();
            }
        }

        private bool _filtroMesAtivo;
        public bool FiltroMesAtivo
        {
            get => _filtroMesAtivo;
            set
            {
                _filtroMesAtivo = value;
                OnPropertyChanged();
            }
        }

        private string _resultadosFiltro;
        public string ResultadosFiltro
        {
            get => _resultadosFiltro;
            set
            {
                _resultadosFiltro = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region COMANDOS

        public RelayCommand FiltrarHojeCommand { get; set; }
        public RelayCommand FiltrarMesCommand { get; set; }
        public RelayCommand LimparFiltrosCommand { get; set; }
        public RelayCommand AplicarFiltroDataCommand { get; set; }

        #endregion

        public DashboardViewModel()
        {
            var db = new Data.AppDbContext();
            _service = new PedidoService(db);

            Pedidos = new ObservableCollection<Pedido>();
            PedidosFiltrados = new ObservableCollection<Pedido>();
            TecnicosLista = new ObservableCollection<string>();
            TecnicosPerformance = new ObservableCollection<TecnicoPerformance>();

            FiltrarHojeCommand = new RelayCommand(FiltrarHoje);
            FiltrarMesCommand = new RelayCommand(FiltrarMes);
            LimparFiltrosCommand = new RelayCommand(LimparFiltros);
            AplicarFiltroDataCommand = new RelayCommand(AplicarFiltros);

            TecnicoFiltro = "Todos";

            CarregarPedidos();
            AtualizarCardsHoje();
        }

        #region PROCESSAR DATAS

        private void ProcessarData(string valor, bool inicial)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                if (inicial)
                    _dataFiltro = null;
                else
                    _dataFinalFiltro = null;

                return;
            }

            if (valor.Length == 10 &&
                DateTime.TryParseExact(valor, "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime data))
            {
                if (inicial)
                {
                    if (_dataFiltro != data)
                    {
                        _dataFiltro = data;
                        OnPropertyChanged(nameof(DataFiltro));
                    }
                }
                else
                {
                    if (_dataFinalFiltro != data)
                    {
                        _dataFinalFiltro = data;
                        OnPropertyChanged(nameof(DataFinalFiltro));
                    }
                }

                FiltroMesAtivo = false;
                AplicarFiltros();
            }
        }

        #endregion

        #region FILTRO PRINCIPAL

        private void AtualizarPedidosFiltrados()
        {
            var query = Pedidos.AsEnumerable();

            // Filtro por técnico
            if (!string.IsNullOrEmpty(TecnicoFiltro) && TecnicoFiltro != "Todos")
                query = query.Where(p => p.Itens != null &&
                                         p.Itens.Any(i => i.Tecnico == TecnicoFiltro));

            // INTERVALO DE DATAS
            if (DataFiltro.HasValue && DataFinalFiltro.HasValue)
            {
                if (DataFiltro > DataFinalFiltro)
                {
                    MessageBox.Show("Data inicial não pode ser maior que a data final.", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                query = query.Where(p =>
                    p.Data.Date >= DataFiltro.Value.Date &&
                    p.Data.Date <= DataFinalFiltro.Value.Date);
            }
            else if (DataFiltro.HasValue)
            {
                query = query.Where(p => p.Data.Date >= DataFiltro.Value.Date);
            }
            else if (DataFinalFiltro.HasValue)
            {
                query = query.Where(p => p.Data.Date <= DataFinalFiltro.Value.Date);
            }
            else if (FiltroMesAtivo)
            {
                var hoje = DateTime.Today;
                query = query.Where(p => p.Data.Month == hoje.Month &&
                                         p.Data.Year == hoje.Year);
            }

            var lista = query.OrderByDescending(p => p.Data).ToList();

            PedidosFiltrados.Clear();
            foreach (var p in lista)
                PedidosFiltrados.Add(p);

            // Atualizar contador de resultados
            ResultadosFiltro = $"Mostrando {PedidosFiltrados.Count} de {Pedidos.Count} pedidos";

            AtualizarCardsFiltrados();
            AtualizarPerformanceTecnicos(); // <- AGORA USA OS PEDIDOS FILTRADOS
        }

        private void AplicarFiltros()
        {
            FiltroMesAtivo = false;
            AtualizarPedidosFiltrados();
        }

        private void AtualizarTudo()
        {
            AtualizarPedidosFiltrados();
            AtualizarCardsHoje();
        }

        #endregion

        #region BOTÕES

        private void FiltrarHoje()
        {
            FiltroMesAtivo = false;
            DataTextoFiltro = DateTime.Today.ToString("dd/MM/yyyy");
            DataTextoFinalFiltro = DateTime.Today.ToString("dd/MM/yyyy");
            // As propriedades DataFiltro e DataFinalFiltro serão atualizadas via ProcessarData
        }

        private void FiltrarMes()
        {
            DataTextoFiltro = string.Empty;
            DataTextoFinalFiltro = string.Empty;
            DataFiltro = null;
            DataFinalFiltro = null;
            FiltroMesAtivo = true;
            AtualizarPedidosFiltrados();
        }

        private void LimparFiltros()
        {
            TecnicoFiltro = "Todos";
            DataTextoFiltro = string.Empty;
            DataTextoFinalFiltro = string.Empty;
            DataFiltro = null;
            DataFinalFiltro = null;
            FiltroMesAtivo = false;
            AtualizarPedidosFiltrados();
        }

        #endregion

        #region CARDS

        private void AtualizarCardsHoje()
        {
            var hoje = DateTime.Today;
            var pedidosHoje = Pedidos.Where(p => p.Data.Date == hoje && p.Status != "Cancelado").ToList();

            PedidosHoje = pedidosHoje.Count;
            FaturamentoHoje = pedidosHoje.Sum(p => p.Total);
            TicketMedio = PedidosHoje > 0 ? FaturamentoHoje / PedidosHoje : 0;
            ItensEmServico = pedidosHoje.SelectMany(p => p.Itens ?? Enumerable.Empty<ItemPedido>()).Count();
        }

        private void AtualizarCardsFiltrados()
        {
            var pedidos = PedidosFiltrados.Where(p => p.Status != "Cancelado").ToList();

            PedidosFiltradosTotal = pedidos.Count;
            FaturamentoFiltrado = pedidos.Sum(p => p.Total);
            TicketMedioFiltrado = PedidosFiltradosTotal > 0
                ? Math.Round(FaturamentoFiltrado / PedidosFiltradosTotal, 2)
                : 0;

            ItensFiltrados = pedidos.SelectMany(p => p.Itens ?? Enumerable.Empty<ItemPedido>()).Count();
        }

        #endregion

        #region DADOS

        private void CarregarPedidos()
        {
            var lista = _service.ObterPedidos().ToList();

            Pedidos.Clear();
            foreach (var p in lista)
                Pedidos.Add(p);

            // Carregar lista de técnicos para o ComboBox
            var tecnicos = lista
                .SelectMany(p => p.Itens ?? Enumerable.Empty<ItemPedido>())
                .Where(i => !string.IsNullOrWhiteSpace(i.Tecnico))
                .Select(i => i.Tecnico)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            TecnicosLista.Clear();
            TecnicosLista.Add("Todos");
            foreach (var t in tecnicos)
                TecnicosLista.Add(t);

            AtualizarPedidosFiltrados();
            AtualizarCardsHoje();
        }

        private void AtualizarPerformanceTecnicos()
        {
            // Usar PedidosFiltrados para a performance (respeita os filtros de data)
            var fonte = PedidosFiltrados.Any() ? PedidosFiltrados : Pedidos;

            var perf = fonte
                .Where(p => p.Status != "Cancelado")
                .SelectMany(p => p.Itens ?? Enumerable.Empty<ItemPedido>())
                .Where(i => !string.IsNullOrWhiteSpace(i.Tecnico))
                .GroupBy(i => i.Tecnico)
                .Select(g => new TecnicoPerformance
                {
                    Nome = g.Key,
                    TotalPedidos = g.Count()
                })
                .OrderByDescending(t => t.TotalPedidos)
                .ToList();

            TecnicosPerformance.Clear();
            foreach (var t in perf)
                TecnicosPerformance.Add(t);

            // Calcular total geral de pedidos nos itens (para exibir no rodapé)
            TotalGeralPedidos = perf.Sum(t => t.TotalPedidos);
            OnPropertyChanged(nameof(TotalGeralPedidos));
        }

        // Propriedade para o total geral de pedidos (usada no XAML)
        public int TotalGeralPedidos { get; private set; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string nome = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
    }

    public class TecnicoPerformance
    {
        public string Nome { get; set; }
        public int TotalPedidos { get; set; }
    }
}