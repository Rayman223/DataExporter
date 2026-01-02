//+------------------------------------------------------------------+
//| Data Exporter - Version 1.1.0                                    |
//| By Rayman223                                                     |
//+------------------------------------------------------------------+

using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using cAlgo.API;
using cAlgo.API.Internals;
using System.Diagnostics.Contracts;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class DataExporter : Robot
    {
        // Private fields for backtest append mode
        private System.IO.StreamWriter _writer;
        private HashSet<string> _writtenTimestamps;
        private string _fullPath;

        [Parameter("Output Path", Group = "Settings", DefaultValue = "D:\\Trading\\data\\")]
        public string OutputPath { get; set; }

        // This robot must run only in backtest mode; no toggle parameter needed.

        protected override void OnStart()
        {
            // Prepare output path and filename upfront
            string timeframe = Bars.TimeFrame.ToString().ToLower();
            string symbol = SymbolName.ToLower().Replace("/", "");
            string timestamp = Server.Time.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{symbol}_{timeframe}_{timestamp}.csv";
            _fullPath = Path.Combine(OutputPath, fileName);

            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
                Print($"✅ Created directory: {OutputPath}");
            }

            // Enforce backtest-only execution
            if (!IsBacktesting)
            {
                Print("❌ This robot must be run in backtest mode only. Aborting.");
                Stop();
                return;
            }

            // Backtest mode: open writer and let OnBar() append every bar (no duplicates)
            _writtenTimestamps = new HashSet<string>();
            if (File.Exists(_fullPath))
            {
                try
                {
                    var lines = File.ReadAllLines(_fullPath);
                    for (int i = 1; i < lines.Length; i++) // skip header
                    {
                        var parts = lines[i].Split(';');
                        if (parts.Length > 0)
                            _writtenTimestamps.Add(parts[0]);
                    }
                }
                catch (Exception ex)
                {
                    Print($"Warning reading existing CSV: {ex.Message}");
                }
            }

            // Ensure header exists
            if (!File.Exists(_fullPath))
            {
                using (var w = File.CreateText(_fullPath))
                {
                    w.WriteLine("timestamp;open;high;low;close;volume");
                }
            }

            // Open writer for append with UTF-8 encoding
            _writer = new StreamWriter(_fullPath, true, Encoding.UTF8) { AutoFlush = true };

            Print("🔁 Running in backtest mode — bars will be appended via OnBar().");
            Print("✅ Decimal format: POINT (.) - No comma separator - Ready for analysis!");
            return;
        }

        protected override void OnBar()
        {
            try
            {
                if (!IsBacktesting || _writer == null)
                    return;

                // closed bar index: Bars.Count - 2 (Bars.Count -1 is the current forming bar)
                int index = Bars.Count - 2;
                if (index < 0)
                    return;

                string timestamp = Bars.OpenTimes[index].ToString("yyyy-MM-dd HH:mm:ss");
                if (_writtenTimestamps != null && _writtenTimestamps.Contains(timestamp))
                    return; // already written

                string line = FormatBarData(index);
                _writer.WriteLine(line);
                _writtenTimestamps.Add(timestamp);
            }
            catch (Exception ex)
            {
                Print($"Error in OnBar: {ex.Message}");
            }
        }

        private string FormatBarData(int index)
        {
            // Format: timestamp;open;high;low;close;volume
            var openTime = Bars.OpenTimes[index];
            var open = Bars.OpenPrices[index];
            var high = Bars.HighPrices[index];
            var low = Bars.LowPrices[index];
            var close = Bars.ClosePrices[index];
            var volume = Bars.TickVolumes[index];

            // Format timestamp as ISO 8601
            string timestamp = openTime.ToString("yyyy-MM-dd HH:mm:ss");

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0};{1:F5};{2:F5};{3:F5};{4:F5};{5}",
                timestamp,
                open,
                high,
                low,
                close,
                volume
            );
        }

        protected override void OnStop()
        {
            try
            {
                if (_writer != null)
                {
                    _writer.Flush();
                    _writer.Close();
                    _writer = null;
                    Print($"✅ Backtest CSV saved: {_fullPath}");
                }
            }
            catch (Exception ex)
            {
                Print($"Error closing writer: {ex.Message}");
            }

            Print("🛑 Data Exporter Bot stopped.");
        }
    }
}