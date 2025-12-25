# 📊 DataExporter – cTrader cBot

**DataExporter** is a cBot for **cTrader / cAlgo** designed to **export OHLCV market data** (Open, High, Low, Close, Volume) for a given symbol and timeframe **exclusively during backtesting**.

---

## ✨ Features

- ✅ **Backtest-only execution** (auto-aborts in live or demo mode)  
- 📁 Automatic output directory creation  
- 📄 Timestamped CSV files (no overwriting)  
- 🔁 Bar-by-bar export using `OnBar()`  
- 🚫 No duplicate candles (unique timestamps)  
- 🕒 ISO 8601 timestamp format  
- 📈 Exports: open, high, low, close, volume  

---

## 📂 CSV Format

timestamp;open;high;low;close;volume  
2025-01-01 00:00:00;1.09523;1.09610;1.09480;1.09590;1234  

- Separator: `;`  
- Prices formatted to 5 decimals  
- Volume = Tick Volume  

---

## ⚙️ Parameters

| Parameter | Description | Default |
|---------|------------|---------|
| Output Path | Folder where CSV files are saved | `D:\Trading\data\` |

File name is generated automatically:

symbol_timeframe_timestamp.csv  

Example:  
eurusd_h1_20251225_101530.csv  

---

## 🚀 Usage

1. Copy `DataExporter.cs` into:  
   Documents\cTrader\Robots\  

2. Compile the cBot in cTrader  

3. Run a **backtest only**  

4. The CSV file is generated automatically at the end of the backtest  

⚠️ **If launched outside backtesting mode, the cBot will stop immediately**

---

## ❌ Intentional Limitations

- ❌ No live or demo execution  
- ❌ No real-time exporting  
- ❌ Single symbol per backtest  

These limitations are intentional to ensure clean and consistent datasets.

---

## 📜 Licence

This project is licensed under:

**Creative Commons Attribution–NonCommercial 4.0 International (CC BY-NC 4.0)**

✔️ You may:
- Share  
- Modify  
- Redistribute  
- Use for personal or educational purposes  

❌ You may NOT:
- Use commercially  
- Sell or monetise  
- Integrate into paid products or services  

Full licence text:  
https://creativecommons.org/licenses/by-nc/4.0/

---

## 🤝 Contributions

Contributions, improvements and suggestions are welcome  
(issues, pull requests, forks 👍).

---

## 📌 Disclaimer

This cBot is provided **as-is**, without any warranty.  
It does **not** constitute financial advice.
