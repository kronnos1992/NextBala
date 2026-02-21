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
using System.Windows.Controls;

namespace NextBala.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly PedidoService _service;

        #region CONTROLE PERFORMANCE

        private bool _performanceFiltrada = true; // Valor padrão true para mostrar performance filtrada
        public bool PerformanceFiltrada
        {
            get => _performanceFiltrada;
            set
            {
                _performanceFiltrada = value;
                OnPropertyChanged();
                AtualizarPerformanceTecnicos(); // Atualiza quando alternar
            }
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

        #region SELEÇÃO

        private Pedido _pedidoSelecionado;
        public Pedido PedidoSelecionado
        {
            get => _pedidoSelecionado;
            set
            {
                _pedidoSelecionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PodeCancelar));
                OnPropertyChanged(nameof(PodeAlterarTecnico));
            }
        }

        public bool PodeCancelar => PedidoSelecionado?.Status == "Ativo";
        public bool PodeAlterarTecnico => PedidoSelecionado?.Status == "Ativo";

        #endregion

        #region FILTROS

        private string _tecnicoFiltro = "Todos";
        public string TecnicoFiltro
        {
            get => _tecnicoFiltro;
            set
            {
                if (_tecnicoFiltro != value)
                {
                    _tecnicoFiltro = value;
                    OnPropertyChanged();
                    AplicarFiltros();
                }
            }
        }

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

        private DateTime? _dataFiltro;
        public DateTime? DataFiltro
        {
            get => _dataFiltro;
            set
            {
                if (_dataFiltro != value)
                {
                    _dataFiltro = value;
                    OnPropertyChanged();
                    if (value.HasValue || _dataFinalFiltro.HasValue)
                        AplicarFiltros();
                    else
                        AtualizarTudo();
                }
            }
        }

        private DateTime? _dataFinalFiltro;
        public DateTime? DataFinalFiltro
        {
            get => _dataFinalFiltro;
            set
            {
                if (_dataFinalFiltro != value)
                {
                    _dataFinalFiltro = value;
                    OnPropertyChanged();
                    if (value.HasValue || _dataFiltro.HasValue)
                        AplicarFiltros();
                    else
                        AtualizarTudo();
                }
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
        public RelayCommand DiagnosticCommand { get; set; }
        public RelayCommand<Pedido> CancelarPedidoCommand { get; set; }
        public RelayCommand<Pedido> AlterarTecnicoCommand { get; set; }

        #endregion

        #region TOTAL GERAL

        private int _totalGeralPedidos;
        public int TotalGeralPedidos
        {
            get => _totalGeralPedidos;
            private set
            {
                _totalGeralPedidos = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public DashboardViewModel()
        {
            var db = new Data.AppDbContext();
            _service = new PedidoService(db);

            Pedidos = new ObservableCollection<Pedido>();
            PedidosFiltrados = new ObservableCollection<Pedido>();
            TecnicosLista = new ObservableCollection<string>();
            TecnicosPerformance = new ObservableCollection<TecnicoPerformance>();

            InicializarComandos();

            CarregarPedidos();
            AtualizarCardsHoje();
        }

        private void InicializarComandos()
        {
            FiltrarHojeCommand = new RelayCommand(FiltrarHoje);
            FiltrarMesCommand = new RelayCommand(FiltrarMes);
            LimparFiltrosCommand = new RelayCommand(LimparFiltros);
            AplicarFiltroDataCommand = new RelayCommand(AplicarFiltros);
            DiagnosticCommand = new RelayCommand(Helpers.DiagnosticHelper.ShowDiagnosticInfo);

            CancelarPedidoCommand = new RelayCommand<Pedido>(
                CancelarPedido,
                pedido => pedido?.Status == "Ativo");

            AlterarTecnicoCommand = new RelayCommand<Pedido>(
                AlterarTecnico,
                pedido => pedido?.Status == "Ativo");
        }

        #region PROCESSAR DATAS

        private void ProcessarData(string valor, bool inicial)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                if (inicial)
                    DataFiltro = null;
                else
                    DataFinalFiltro = null;
                return;
            }

            if (valor.Length == 10 &&
                DateTime.TryParseExact(valor, "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime data))
            {
                if (inicial)
                {
                    DataFiltro = data;
                }
                else
                {
                    DataFinalFiltro = data;
                }

                FiltroMesAtivo = false;
            }
        }

        #endregion

        #region FILTRO PRINCIPAL

        private void AtualizarPedidosFiltrados()
        {
            if (Pedidos == null || !Pedidos.Any())
            {
                PedidosFiltrados?.Clear();
                ResultadosFiltro = "Nenhum pedido encontrado";
                return;
            }

            var query = Pedidos.AsEnumerable();

            // Filtro por técnico
            if (!string.IsNullOrEmpty(TecnicoFiltro) && TecnicoFiltro != "Todos")
            {
                query = query.Where(p => p.Itens != null &&
                                         p.Itens.Any(i => i.Tecnico == TecnicoFiltro));
            }

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

            ResultadosFiltro = $"Mostrando {PedidosFiltrados.Count} de {Pedidos.Count} pedidos";

            AtualizarCardsFiltrados();
            AtualizarPerformanceTecnicos();
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

        #region AÇÕES DE PEDIDO

        private void CancelarPedido(Pedido pedido)
        {
            if (pedido == null) return;

            var result = MessageBox.Show(
                $"Deseja realmente cancelar o pedido Nº {pedido.NumeroPedido}?",
                "Confirmar Cancelamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _service.CancelarPedido(pedido.Id);
                    pedido.Status = "Cancelado";

                    AtualizarColecoes(pedido);
                    OnPropertyChanged(nameof(PodeCancelar));
                    OnPropertyChanged(nameof(PodeAlterarTecnico));

                    AtualizarCardsHoje();
                    AtualizarCardsFiltrados();
                    AtualizarPerformanceTecnicos();

                    MessageBox.Show("Pedido cancelado com sucesso!", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao cancelar pedido: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AlterarTecnico(Pedido pedido)
        {
            if (pedido == null) return;

            var dialog = CriarDialogAlterarTecnico(pedido);
            dialog.ShowDialog();
        }

        private Window CriarDialogAlterarTecnico(Pedido pedido)
        {
            var dialog = new Window
            {
                Title = "Alterar Técnico",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Selecione o novo técnico:",
                Margin = new Thickness(0, 0, 0, 10),
                FontWeight = FontWeights.Bold
            });

            var tecnicosDisponiveis = TecnicosLista?.Where(t => t != "Todos").ToList() ?? new List<string>();

            var comboBox = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                ItemsSource = tecnicosDisponiveis,
                SelectedIndex = 0
            };

            stackPanel.Children.Add(comboBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var btnOk = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };

            var btnCancel = new Button
            {
                Content = "Cancelar",
                Width = 80,
                Height = 30,
                IsCancel = true
            };

            buttonPanel.Children.Add(btnOk);
            buttonPanel.Children.Add(btnCancel);
            stackPanel.Children.Add(buttonPanel);

            dialog.Content = stackPanel;

            btnOk.Click += (s, e) =>
            {
                var novoTecnico = comboBox.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(novoTecnico))
                {
                    try
                    {
                        _service.AtualizarTecnicoPedido(pedido.Id, novoTecnico);

                        if (pedido.Itens != null)
                        {
                            foreach (var item in pedido.Itens)
                            {
                                item.Tecnico = novoTecnico;
                            }
                        }

                        AtualizarColecoes(pedido);
                        AtualizarPerformanceTecnicos();

                        MessageBox.Show($"Técnico alterado para {novoTecnico} com sucesso!",
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                        dialog.DialogResult = true;
                        dialog.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao alterar técnico: {ex.Message}", "Erro",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    dialog.Close();
                }
            };

            btnCancel.Click += (s, e) => dialog.Close();

            return dialog;
        }

        private void AtualizarColecoes(Pedido pedido)
        {
            var index = Pedidos.IndexOf(pedido);
            if (index >= 0)
            {
                Pedidos[index] = pedido;
            }

            index = PedidosFiltrados.IndexOf(pedido);
            if (index >= 0)
            {
                PedidosFiltrados[index] = pedido;
            }
        }

        #endregion

        #region CARDS

        private void AtualizarCardsHoje()
        {
            var hoje = DateTime.Today;
            var pedidosHoje = Pedidos?.Where(p => p.Data.Date == hoje && p.Status != "Cancelado").ToList() ?? new List<Pedido>();

            PedidosHoje = pedidosHoje.Count;
            FaturamentoHoje = pedidosHoje.Sum(p => p.Total);
            TicketMedio = PedidosHoje > 0 ? Math.Round(FaturamentoHoje / PedidosHoje, 2) : 0;
            ItensEmServico = pedidosHoje.SelectMany(p => p.Itens ?? Enumerable.Empty<ItemPedido>()).Count();
        }

        private void AtualizarCardsFiltrados()
        {
            var pedidos = PedidosFiltrados?.Where(p => p.Status != "Cancelado").ToList() ?? new List<Pedido>();

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
            try
            {
                var lista = _service.ObterPedidos().ToList();

                Pedidos.Clear();
                foreach (var p in lista)
                    Pedidos.Add(p);

                CarregarTecnicos(lista);
                AtualizarPedidosFiltrados();
                AtualizarCardsHoje();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar pedidos: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CarregarTecnicos(List<Pedido> lista)
        {
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
        }

        private void AtualizarPerformanceTecnicos()
        {
            // Usar PedidosFiltrados para a performance (já respeita os filtros de data e técnico)
            var fonte = PedidosFiltrados;

            if (fonte == null || !fonte.Any())
            {
                TecnicosPerformance?.Clear();
                TotalGeralPedidos = 0;
                return;
            }

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

            TotalGeralPedidos = perf.Sum(t => t.TotalPedidos);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
        }
    }

    public class TecnicoPerformance
    {
        public string Nome { get; set; }
        public int TotalPedidos { get; set; }
    }
}