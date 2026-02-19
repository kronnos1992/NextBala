using Microsoft.EntityFrameworkCore;
using NextBala.Data;
using NextBala.Models;
using System.Collections.Generic;
using System.Linq;

namespace NextBala.Services
{
    public class PedidoService
    {
        private readonly AppDbContext _context;

        public PedidoService(AppDbContext context)
        {
            _context = context;

            // Garantir que o banco de dados existe
            try
            {
                _context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao conectar ao banco de dados: {ex.Message}\n\nCaminho: {_context.DbPath}",
                    "Erro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        // Salvar novo pedido
        public void AdicionarPedido(Pedido pedido)
        {
            if (pedido == null)
                throw new ArgumentNullException(nameof(pedido));

            try
            {
                // Adiciona pedido e itens no EF
                _context.Pedidos.Add(pedido);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar pedido: {ex.Message}", ex);
            }
        }

        // Obter todos os pedidos
        public List<Pedido> ObterPedidos()
        {
            try
            {
                var pedidosExistentes = _context.Pedidos
                    .Include(p => p.Itens)   // <--- importantíssimo
                    .Include(p => p.Cliente)
                    .OrderByDescending(p => p.Data) // Mais recentes primeiro
                    .ToList();

                return pedidosExistentes;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao carregar pedidos: {ex.Message}",
                    "Erro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<Pedido>();
            }
        }

        // Buscar cliente por Id
        public Cliente? ObterCliente(int id)
        {
            try
            {
                return _context.Clientes
                    .FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao buscar cliente: {ex.Message}",
                    "Erro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        // Adicionar cliente
        public void AdicionarCliente(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            try
            {
                // Verificar se cliente já existe (opcional)
                var clienteExistente = _context.Clientes
                    .FirstOrDefault(c => c.Nome == cliente.Nome && c.Telefone == cliente.Telefone);

                if (clienteExistente != null)
                {
                    // Se já existe, usar o existente
                    cliente.Id = clienteExistente.Id;
                }
                else
                {
                    _context.Clientes.Add(cliente);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar cliente: {ex.Message}", ex);
            }
        }

        public Pedido? ObterUltimoPedidoSalvo()
        {
            try
            {
                return _context.Pedidos
                    .Include(p => p.Itens)
                    .Include(p => p.Cliente)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao buscar último pedido: {ex.Message}",
                    "Erro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        // Gerar ticket completo em string
        public string GerarTicket(Pedido pedido)
        {
            if (pedido == null)
                return string.Empty;

            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("====== TICKET ======");
                sb.AppendLine($"Pedido Nº: {pedido.NumeroPedido}");
                sb.AppendLine($"Cliente: {pedido.Cliente?.Nome}");
                sb.AppendLine($"Telefone: {pedido.Cliente?.Telefone}");
                sb.AppendLine($"Data: {pedido.Data:dd/MM/yyyy}");
                sb.AppendLine("=== BALANGOLA ===");


                return sb.ToString();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao gerar ticket: {ex.Message}",
                    "Erro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return "Erro ao gerar ticket";
            }
        }

        // Método adicional: Pesquisar pedidos por cliente
        public List<Pedido> ObterPedidosPorCliente(string nomeCliente)
        {
            try
            {
                return _context.Pedidos
                    .Include(p => p.Itens)
                    .Include(p => p.Cliente)
                    .Where(p => p.Cliente.Nome.Contains(nomeCliente))
                    .OrderByDescending(p => p.Data)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao pesquisar pedidos: {ex.Message}",
                    "Erro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<Pedido>();
            }
        }

        // Método adicional: Obter pedidos por período
        public List<Pedido> ObterPedidosPorPeriodo(DateTime inicio, DateTime fim)
        {
            try
            {
                return _context.Pedidos
                    .Include(p => p.Itens)
                    .Include(p => p.Cliente)
                    .Where(p => p.Data >= inicio && p.Data <= fim)
                    .OrderByDescending(p => p.Data)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erro ao buscar pedidos por período: {ex.Message}",
                    "Erro", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<Pedido>();
            }
        }
    }
}
