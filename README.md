# SigrokLogger

`SigrokLogger` 是一個基於 C# 的自動化數位訊號擷取工具，透過 `sigrok-cli` 執行多通道訊號錄製，並支援多次重試、指定擷取時長、檔案自動命名與分步驟記錄等功能。  
此工具特別適用於使用 **fx2lafw** 裝置進行 I²C / UART / GPIO 等訊號分析與測試流程記錄。

## 功能特點

- **自動化擷取**：支援多次重試與延遲間隔設定
- **多通道支援**：可同時擷取多條數位訊號
- **自動命名檔案**：依日期、時間與步驟自動生成檔名
- **執行緒控制**：擷取過程在背景執行，支援隨時停止
- **日誌管理**：自動建立每日的日誌目錄

## 系統需求

- Windows 系統
- 下載安裝可執行的 [sigrok-cli和Pluseview](https://sigrok.org/wiki/Downloads)軟體
- 支援 **fx2lafw** 韌體的邏輯分析儀硬體，如MuseLab推出的開源的邏輯分析儀[nanoDLA](https://www.muselab-tech.com/zi-ji-dong-shou-zuo-kai-yuan-luo-ji-fen-xi-yi/)
- Pulseview支援I2C ASCII套件 [PulseView-Decoder-I2C-ASCII](https://github.com/Palingenesis/PulseView-Decoder-I2C-ASCII)

## 安裝與使用

1. 安裝 `sigrok-cli` 並確認能在命令列中執行。
2. 將PulseView-Decoder-I2C-ASCII安裝包解壓縮到pluseview的協定解析器（protocol decoders）所在路徑，例如：
   ```plaintext
   pulseview-NIGHTLY-x86_64-release-installer\share\libsigrokdecode\decoders
   ```
3. 修改 `SigrokLogger.cs` 中的路徑設定：
   ```csharp
   private const string SoftwarePath = @"D:\使用者\sigrok-cli-NIGHTLY-x86_64-release-installer";
   private const string BaseLogPath = @"D:\使用者\sigrok-log";
   ```
4. 在專案中建立 SigrokCapture 實例並呼叫：
   ```csharp
    var capture = new SigrokLogger.SigrokCapture();
    capture.StartCaptureWithThread(maxAttempts: 5, durationMs: 1000, step: 1);
    ```
5. 停止擷取：
   ```csharp
   capture.StopCapture();
6. 範例輸出檔案命名規則
   ```csharp
   YYYYMMDD\YYYYMMDD_HHMMSS_step_X.sr
   ``` 
   例如：
   ```csharp
   20250801\20250801_143210_step_1.sr
   ```
