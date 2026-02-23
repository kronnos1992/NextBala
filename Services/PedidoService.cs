using Microsoft.EntityFrameworkCore;
using NextBala.Data;
using NextBala.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextBala.Services
{
    public class PedidoService
    {
        private readonly AppDbContext _context;

        public PedidoService(AppDbContext context)
        {
            _context = context;
        }

        // ========== CLIENTES ==========

        public void AdicionarCliente(Cliente cliente)
        {
            try
            {
                var existente = _context.Clientes
                    .FirstOrDefault(c => c.Telefone == cliente.Telefone && !string.IsNullOrEmpty(cliente.Telefone));

                if (existente != null)
                {
                    existente.Nome = cliente.Nome;
                    _context.SaveChanges();
                }
                else
                {
                    _context.Clientes.Add(cliente);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao adicionar cliente: {ex.Message}");
            }
        }

        public Cliente ObterClientePorTelefone(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return null;

            try
            {
                return _context.Clientes
                    .FirstOrDefault(c => c.Telefone == telefone);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar cliente: {ex.Message}");
            }
        }

        public void AtualizarCliente(Cliente cliente)
        {
            try
            {
                var existente = _context.Clientes.Find(cliente.Id);
                if (existente != null)
                {
                    existente.Nome = cliente.Nome;
                    existente.Telefone = cliente.Telefone;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar cliente: {ex.Message}");
            }
        }

        public List<Cliente> ObterTodosClientes()
        {
            try
            {
                return _context.Clientes.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter clientes: {ex.Message}");
            }
        }

        // ========== PEDIDOS ==========

        public void AdicionarPedido(Pedido pedido)
        {
            try
            {
                if (pedido.Cliente != null)
                {
                    if (pedido.Cliente.Id == 0)
                    {
                        _context.Clientes.Add(pedido.Cliente);
                    }
                    else
                    {
                        _context.Entry(pedido.Cliente).State = EntityState.Unchanged;
                    }
                }

                _context.Pedidos.Add(pedido);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao adicionar pedido: {ex.Message}");
            }
        }

        public List<Pedido> ObterPedidos()
        {
            try
            {
                return _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Itens)
                    .OrderByDescending(p => p.Data)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter pedidos: {ex.Message}");
            }
        }

        public Pedido ObterPedidoPorId(int id)
        {
            try
            {
                return _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Itens)
                    .FirstOrDefault(p => p.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter pedido: {ex.Message}");
            }
        }

        public List<Pedido> ObterPedidosDoDia(DateTime data)
        {
            try
            {
                return _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Itens)
                    .Where(p => p.Data.Date == data.Date)
                    .OrderBy(p => p.NumeroPedido)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter pedidos do dia: {ex.Message}");
            }
        }

        // Cancelar pedido
        public void CancelarPedido(int pedidoId)
        {
            try
            {
                var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == pedidoId);
                if (pedido != null)
                {
                    pedido.Status = "Cancelado";
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao cancelar pedido: {ex.Message}");
            }
        }

        // NOVO: Atualizar técnico do pedido
        public void AtualizarTecnicoPedido(int pedidoId, string novoTecnico)
        {
            try
            {
                var pedido = _context.Pedidos
                    .Include(p => p.Itens)
                    .FirstOrDefault(p => p.Id == pedidoId);

                if (pedido != null && pedido.Itens != null)
                {
                    // Verificar se o pedido não está cancelado
                    if (pedido.Status == "Cancelado")
                    {
                        throw new Exception("Não é possível alterar o técnico de um pedido cancelado.");
                    }

                    // Validar se o novo técnico é válido
                    if (string.IsNullOrWhiteSpace(novoTecnico))
                    {
                        throw new Exception("O nome do técnico não pode ser vazio.");
                    }

                    // Registrar o técnico anterior para log/histórico (opcional)
                    var tecnicoAnterior = pedido.Itens.FirstOrDefault()?.Tecnico;

                    // Atualizar todos os itens do pedido com o novo técnico
                    foreach (var item in pedido.Itens)
                    {
                        item.Tecnico = novoTecnico;
                    }

                    _context.SaveChanges();

                    // Aqui você poderia adicionar um log da alteração se necessário
                    // LogAlteracaoTecnico(pedidoId, tecnicoAnterior, novoTecnico);
                }
                else
                {
                    throw new Exception("Pedido não encontrado ou não possui itens.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar técnico do pedido: {ex.Message}");
            }
        }

        // Método adicional: Atualizar técnico de um item específico
        public void AtualizarTecnicoItem(int itemId, string novoTecnico)
        {
            try
            {
                var item = _context.ItensPedido
                    .Include(i => i.Pedido)
                    .FirstOrDefault(i => i.Id == itemId);

                if (item != null)
                {
                    // Verificar se o pedido não está cancelado
                    if (item.Pedido.Status == "Cancelado")
                    {
                        throw new Exception("Não é possível alterar o técnico de um item de pedido cancelado.");
                    }

                    // Validar se o novo técnico é válido
                    if (string.IsNullOrWhiteSpace(novoTecnico))
                    {
                        throw new Exception("O nome do técnico não pode ser vazio.");
                    }

                    item.Tecnico = novoTecnico;
                    _context.SaveChanges();
                }
                else
                {
                    throw new Exception("Item não encontrado.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar técnico do item: {ex.Message}");
            }
        }

        // Método adicional: Obter pedidos por técnico
        public List<Pedido> ObterPedidosPorTecnico(string tecnico)
        {
            try
            {
                return _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Itens)
                    .Where(p => p.Itens.Any(i => i.Tecnico == tecnico))
                    .OrderByDescending(p => p.Data)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter pedidos por técnico: {ex.Message}");
            }
        }

        // Método adicional: Obter estatísticas por técnico
        public Dictionary<string, object> ObterEstatisticasPorTecnico(string tecnico)
        {
            try
            {
                var pedidos = _context.Pedidos
                    .Include(p => p.Itens)
                    .Where(p => p.Itens.Any(i => i.Tecnico == tecnico))
                    .ToList();

                var stats = new Dictionary<string, object>
                {
                    { "TotalPedidos", pedidos.Count },
                    { "TotalItens", pedidos.SelectMany(p => p.Itens).Count(i => i.Tecnico == tecnico) },
                    { "FaturamentoTotal", pedidos.Sum(p => p.Total) },
                    { "MediaPorPedido", pedidos.Any() ? pedidos.Average(p => p.Total) : 0 },
                    { "PedidosAtivos", pedidos.Count(p => p.Status == "Ativo") },
                    { "PedidosCancelados", pedidos.Count(p => p.Status == "Cancelado") }
                };

                return stats;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter estatísticas por técnico: {ex.Message}");
            }
        }

        // ========== ITENS ==========

        public void AdicionarItem(ItemPedido item)
        {
            try
            {
                _context.ItensPedido.Add(item);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao adicionar item: {ex.Message}");
            }
        }

        public List<ItemPedido> ObterItensPorPedido(int pedidoId)
        {
            try
            {
                return _context.ItensPedido
                    .Where(i => i.PedidoId == pedidoId)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter itens: {ex.Message}");
            }
        }

        public void AtualizarItem(ItemPedido item)
        {
            try
            {
                var existente = _context.ItensPedido.Find(item.Id);
                if (existente != null)
                {
                    existente.Marca = item.Marca;
                    existente.Modelo = item.Modelo;
                    existente.Defeito = item.Defeito;
                    existente.Tecnico = item.Tecnico;
                    existente.Preco = item.Preco;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar item: {ex.Message}");
            }
        }

        public void RemoverItem(int itemId)
        {
            try
            {
                var item = _context.ItensPedido.Find(itemId);
                if (item != null)
                {
                    _context.ItensPedido.Remove(item);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao remover item: {ex.Message}");
            }
        }

        // ========== TICKET ==========

        public string GerarTicket(Pedido pedido)
        {
            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("                         ****** TICKET ******");
                sb.AppendLine($"                    Pedido Nº: {pedido.NumeroPedido}");
                sb.AppendLine($"                    Cliente: {pedido.Cliente?.Nome}");
                sb.AppendLine($"                    Telefone: {pedido.Cliente?.Telefone}");
                sb.AppendLine($"                    Data: {pedido.Data:dd/MM/yyyy}");

                // Adicionar informações dos itens com técnico
                if (pedido.Itens != null && pedido.Itens.Any())
                {
                    foreach (var item in pedido.Itens)
                    {
                        sb.AppendLine($"                    Técnico: {item.Tecnico ?? "Não atribuído"}");
                    }
                }
                else
                {
                    sb.AppendLine("Nenhum item no pedido");
                }
                sb.AppendLine("                    === BALANGOLA ===");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gerar ticket: {ex.Message}");
            }
        }

        // ========== MÉTODOS AUXILIARES ==========

        // Obter lista de todos os técnicos
        public List<string> ObterTodosTecnicos()
        {
            try
            {
                return _context.ItensPedido
                    .Where(i => !string.IsNullOrWhiteSpace(i.Tecnico) && i.Tecnico != "Não atribuído")
                    .Select(i => i.Tecnico)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter lista de técnicos: {ex.Message}");
            }
        }

        // Obter contagem de pedidos por técnico
        public Dictionary<string, int> ObterContagemPedidosPorTecnico()
        {
            try
            {
                return _context.ItensPedido
                    .Where(i => !string.IsNullOrWhiteSpace(i.Tecnico) && i.Tecnico != "Não atribuído")
                    .GroupBy(i => i.Tecnico)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter contagem por técnico: {ex.Message}");
            }
        }

        // Validar se um técnico existe
        public bool TecnicoExiste(string tecnico)
        {
            try
            {
                return _context.ItensPedido
                    .Any(i => i.Tecnico == tecnico);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao verificar existência do técnico: {ex.Message}");
            }
        }
    }
}