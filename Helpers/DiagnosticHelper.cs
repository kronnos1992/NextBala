using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace NextBala.Helpers
{
    public static class DiagnosticHelper
    {
        public static void ShowDiagnosticInfo()
        {
            string info = ObterInfoDiagnostico();
            MessageBox.Show(info, "Informações de Diagnóstico",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static string ObterInfoDiagnostico()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                var location = assembly.Location;

                var info = $"=== DIAGNÓSTICO DO APLICATIVO ===\n" +
                          $"Versão: {version}\n" +
                          $"Executável: {location}\n" +
                          $"Diretório: {Path.GetDirectoryName(location)}\n" +
                          $".NET Version: {Environment.Version}\n" +
                          $"OS: {Environment.OSVersion}\n" +
                          $"64-bit: {Environment.Is64BitProcess}\n" +
                          $"User: {Environment.UserName}\n" +
                          $"Machine: {Environment.MachineName}\n" +
                          $"\n=== BANCO DE DADOS ===\n";

                // Testar AppData
                string appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "NextBala");

                info += $"AppData Path: {appData}\n";
                info += $"AppData Existe: {Directory.Exists(appData)}\n";

                // Testar permissões
                try
                {
                    Directory.CreateDirectory(appData);
                    info += $"Criação de diretório: OK\n";

                    string testFile = Path.Combine(appData, "test.tmp");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    info += $"Escrita/Leitura: OK\n";
                }
                catch (Exception ex)
                {
                    info += $"Permissões: FALHA - {ex.Message}\n";
                }

                // Testar arquivos do aplicativo
                var currentDir = Path.GetDirectoryName(location);
                var files = Directory.GetFiles(currentDir, "*.dll");
                info += $"\n=== DLLs CARREGADAS ({files.Length}) ===\n";

                var sqliteDlls = files.Where(f => f.Contains("SQLite") || f.Contains("sqlite")).ToList();
                info += $"SQLite DLLs encontradas: {sqliteDlls.Count}\n";

                foreach (var dll in sqliteDlls.Take(5))
                {
                    info += $"  - {Path.GetFileName(dll)}\n";
                }

                return info;
            }
            catch (Exception ex)
            {
                return $"Erro ao obter diagnóstico: {ex.Message}";
            }
        }
    }
}