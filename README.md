# GeoGuessr Steam Active Panorama Opener (C#)

**RU:** Программа для автоматического открытия текущей панорамы GeoGuessr (Steam-версия) в браузере через чтение оперативной памяти. 

**EN:** A high-performance C# utility that scans the memory of the Steam version of GeoGuessr to extract the current round's `panoid` and automatically opens the location in your default browser.

---

## 🚀 Features / Возможности
- **Automatic Detection:** Instantly extracts the active game coordinates/panorama ID directly from process memory.
- **Auto-open:** Opens Google Maps with the exact street view location.
- **Zero Lag:** Features a highly optimized memory polling buffer with minimum CPU footprint.
- **Anti-Spam:** 18-second cooldown between browser triggers to prevent duplicate tabs.

---

## 🛠 How to Use / Как запустить

### RU:
1. Запустите игру **GeoGuessr** в Steam.
2. Скомпилируйте и запустите этот проект в Visual Studio (или запустите готовый `.exe` файл).
3. Начните раунд. Как только локация загрузится, утилита автоматически откроет вкладку с точным местом в вашем браузере.

### EN:
1. Launch **GeoGuessr** via Steam.
2. Compile and run this project using Visual Studio (or launch the compiled `.exe`).
3. Start a game round. Once the map loads, the tool will automatically open the exact Street View location in your browser.

---

## ⚠️ Disclaimer
This repository is created for educational and personal research purposes only. Use at your own risk.
