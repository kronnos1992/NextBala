using System.Windows;
using NextBala.Data;

namespace NextBala;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated(); 
            // Se estiver usando Migrations, use:
            // db.Database.Migrate();
        }
    }
}