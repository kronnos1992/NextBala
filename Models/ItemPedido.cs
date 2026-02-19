

namespace NextBala.Models;

public class ItemPedido
{
    public int Id { get; set; }   // <-- ESSA LINHA É OBRIGATÓRIA PARA EF
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public string Defeito { get; set; }
    public string Tecnico { get; set; }
    public decimal Preco { get; set; }

    // Relacionamento com Pedido
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; }
    
}

