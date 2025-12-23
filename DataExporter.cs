using System;
using System.Linq;
using System.Text;
using System.IO;
using cAlgo.API;
using cAlgo.API.Internals;
using System.Diagnostics.Contracts;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class DataExporter : Robot
    {
        [Parameter("Bars to Export", Group = "Settings", DefaultValue = 10000)]
        public int BarsToExport { get; set; }

        [Parameter("Output Path", Group = "Settings", DefaultValue = "D:\\Trading-IA\\data\\")]
        public string OutputPath { get; set; }

        [Parameter("Include Volume", Group = "Settings", DefaultValue = true)]
        public bool IncludeVolume { get; set; }

        protected override void OnStart()
        {
            ExportData();
            Stop();
        }

        private void ExportData()
        {
            try
            {
                Print("=".PadRight(60, '='));
                Print("📊 DATA EXPORT STARTED");
                Print("=".PadRight(60, '='));

                // Créer le dossier s'il n'existe pas
                if (!Directory.Exists(OutputPath))
                {
                    Directory.CreateDirectory(OutputPath);
                    Print($"✅ Created directory: {OutputPath}");
                }

                // Générer le nom du fichier
                string timeframe = MarketSeries.TimeFrame.ToString().ToLower();
                string symbol = SymbolName.ToLower().Replace("/", "");
                string timestamp = Server.Time.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{symbol}_{timeframe}_{timestamp}.csv";
                string fullPath = Path.Combine(OutputPath, fileName);

                // Créer le StringBuilder pour le CSV
                var csv = new StringBuilder();

                // Header
                if (IncludeVolume)
                {
                    csv.AppendLine("timestamp;open;high;low;close;volume");
                }
                else
                {
                    csv.AppendLine("timestamp;open;high;low;close");
                }

                // Calculer l'index de départ (dernières BarsToExport barres)
                int lastIndex = Bars.Count - 1;
                int startIndex = Math.Max(0, lastIndex - BarsToExport + 1);
                int actualBarsExported = (lastIndex >= startIndex) ? (lastIndex - startIndex + 1) : 0;

                Print($"📈 Exporting {actualBarsExported} bars...");
                Print($"   Symbol: {SymbolName}");
                Print($"   lastIndex: {lastIndex}");
                Print($"   startIndex: {startIndex}");
                Print($"   Timeframe: {MarketSeries.TimeFrame}");
                Print($"   Date range: {Bars.OpenTimes[startIndex]:yyyy-MM-dd} to {Bars.LastBar.OpenTime:yyyy-MM-dd}");

                // Export data
                for (int i = startIndex; i < Bars.Count; i++)
                {
                    string line = FormatBarData(i);
                    csv.AppendLine(line);
                }

                // Write to file
                File.WriteAllText(fullPath, csv.ToString());

                // Success message
                Print("=".PadRight(60, '='));
                Print("✅ EXPORT COMPLETED SUCCESSFULLY!");
                Print($"📁 File saved: {fullPath}");
                Print($"📊 Total bars exported: {actualBarsExported}");
                Print($"💾 File size: {new FileInfo(fullPath).Length / 1024} KB");
                Print("=".PadRight(60, '='));
                Print("");
                Print("📝 Next steps:");
                Print($"   1. File is ready at: {fullPath}");
                Print("   2. Run: python prepare_training_data.py");
                Print("   3. Train your model!");
                Print("");

                // Show success on chart
                Chart.DrawStaticText("ExportSuccess",
                    $"✅ EXPORT COMPLETE!\n\n" +
                    $"File: {fileName}\n" +
                    $"Bars: {actualBarsExported}\n" +
                    $"Location: {OutputPath}\n\n" +
                    $"Bot will stop in 5 seconds...",
                    VerticalAlignment.Center,
                    HorizontalAlignment.Center,
                    Color.LimeGreen);

            }
            catch (Exception ex)
            {
                Print("=".PadRight(60, '='));
                Print("❌ ERROR DURING EXPORT!");
                Print($"Error: {ex.Message}");
                Print($"Stack trace: {ex.StackTrace}");
                Print("=".PadRight(60, '='));

                Chart.DrawStaticText("ExportError",
                    $"❌ EXPORT FAILED!\n\n" +
                    $"Error: {ex.Message}\n\n" +
                    $"Check the log for details.",
                    VerticalAlignment.Center,
                    HorizontalAlignment.Center,
                    Color.Red);
            }
        }

        private string FormatBarData(int index)
        {
            // Format: timestamp,open,high,low,close,volume
            var openTime = Bars.OpenTimes[index];
            var open = Bars.OpenPrices[index];
            var high = Bars.HighPrices[index];
            var low = Bars.LowPrices[index];
            var close = Bars.ClosePrices[index];

            // Format timestamp as ISO 8601
            string timestamp = openTime.ToString("yyyy-MM-dd HH:mm:ss");

            if (IncludeVolume)
            {
                var volume = Bars.TickVolumes[index];
                return $"{timestamp};{open:F5};{high:F5};{low:F5};{close:F5};{volume}";
            }
            else
            {
                return $"{timestamp};{open:F5};{high:F5};{low:F5};{close:F5}";
            }
        }

        protected override void OnStop()
        {
            Print("🛑 Data Exporter Bot stopped.");
        }
    }
}