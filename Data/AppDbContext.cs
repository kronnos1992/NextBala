using System.IO;
using Microsoft.EntityFrameworkCore;
using NextBala.Models;

namespace NextBala.Data;

public class AppDbContext : DbContext
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>(); 
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    public string DbPath { get; private set; }

    public AppDbContext()
    {
        // Definir caminho do banco de dados na pasta do usuário
        string appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NextBala");

        // Criar pasta se não existir
        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
        }

        DbPath = Path.Combine(appDataFolder, "NextBala.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");

        // Opcional: Para debug
        #if DEBUG
                options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
                options.EnableSensitiveDataLogging();
        #endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar relacionamentos

        modelBuilder.Entity<ItemPedido>()
            .HasOne(i => i.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(i => i.PedidoId);

        // OU crie um índice composto com Data + NumeroPedido
        modelBuilder.Entity<Pedido>()
            .HasIndex(p => new { p.Data, p.NumeroPedido })
            .IsUnique(true); // Único por dia (melhor opção!)

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.Data);

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Nome);

        base.OnModelCreating(modelBuilder);
    }
}