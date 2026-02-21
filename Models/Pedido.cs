using System;
using System.Collections.Generic;
using System.Linq;

namespace NextBala.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int NumeroPedido { get; set; }
        public DateTime Data { get; set; } = DateTime.Now;

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public List<ItemPedido> Itens { get; set; } = new();

        public decimal Total => Itens.Sum(i => i.Preco);

        // Status do pedido (Ativo ou Cancelado)
        public string Status { get; set; } = "Ativo";

        public string TecnicosConcatenados =>
            Itens != null && Itens.Any()
                ? string.Join(", ", Itens.Select(i => i.Tecnico).Distinct())
                : "Sem técnico";
    }
}