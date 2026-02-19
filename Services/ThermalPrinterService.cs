using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;

namespace NextBala.Services
{
    public class ThermalPrinterService
    {
        private const int LARGURA_58MM = 280;

        // 🔎 Detecta automaticamente a Xprinter instalada
        private string? ObterImpressoraXprinter()
        {
            return PrinterSettings.InstalledPrinters
                .Cast<string>()
                .FirstOrDefault(p => p.ToLower().Contains("xprinter"));
        }

        // ✂️ Envia comando ESC/POS de corte
        private void EnviarComandoCorte()
        {
            string? printer = ObterImpressoraXprinter();
            if (printer == null) return;

            using var pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printer;

            pd.PrintPage += (s, e) =>
            {
                byte[] cortar = new byte[] { 0x1B, 0x69 }; // ESC i
                e.Graphics.DrawString(" ", new Font("Consolas", 1), Brushes.Black, 0, 0);
                e.HasMorePages = false;
            };

            pd.Print();
        }

        // 🧾 Impressão principal
        public void ImprimirTicketSimples(int numero, string cliente, DateTime data)
        {
            string? printer = ObterImpressoraXprinter();

            if (printer == null)
                throw new Exception("Nenhuma impressora Xprinter encontrada.");

            string texto = GerarLayout(numero, cliente, data);

            using var pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printer;

            int altura = 180;

            pd.DefaultPageSettings.PaperSize =
                new PaperSize("Ticket", LARGURA_58MM, altura);

            pd.PrintPage += (sender, e) =>
            {
                float y = 10;

                using var fontTitulo = new Font("Consolas", 10, FontStyle.Bold);
                using var fontNormal = new Font("Consolas", 8);

                // Centralizar
                StringFormat center = new StringFormat
                {
                    Alignment = StringAlignment.Center
                };

                e.Graphics.DrawString("NEXTBALA", fontTitulo, Brushes.Black,
                    new RectangleF(0, y, LARGURA_58MM, 20), center);

                y += 30;

                e.Graphics.DrawString($"Pedido Nº {numero}", fontNormal, Brushes.Black, 0, y);
                y += 20;

                e.Graphics.DrawString($"Cliente: {cliente}", fontNormal, Brushes.Black, 0, y);
                y += 20;

                e.Graphics.DrawString($"Data: {data:dd/MM/yyyy}", fontNormal, Brushes.Black, 0, y);
                y += 30;

                e.Graphics.DrawString("Obrigado pela preferência!", fontNormal, Brushes.Black,
                    new RectangleF(0, y, LARGURA_58MM, 20), center);
            };

            pd.Print();

            // ✂️ corta o papel
            EnviarComandoCorte();
        }

        private string GerarLayout(int numero, string cliente, DateTime data)
        {
            var sb = new StringBuilder();

            sb.AppendLine("NEXTBALA");
            sb.AppendLine("-----------------------------");
            sb.AppendLine($"Pedido Nº: {numero}");
            sb.AppendLine($"Cliente: {cliente}");
            sb.AppendLine($"Data: {data:dd/MM/yyyy}");
            sb.AppendLine("-----------------------------");

            return sb.ToString();
        }
    }
}
