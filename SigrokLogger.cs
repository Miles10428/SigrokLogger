/**
 * @file SigrokLogger.cs
 * @brief This file contains the implementation of the SigrokCapture class.
 * @author u10428
 * @version 0.1.0
 * @date 2025-03-19
 * 
 * @copyright Copyright (c) 2025
 * 
 */
using System;
using System.Diagnostics;
using System.IO;				
using System.Threading;

namespace SigrokLogger
{
    public class SigrokCapture
    {
        private readonly int _maxAttempts;   // 最大重試次數
        private readonly int _durationMs;   // 擷取時間長度（毫秒）
        private Thread _captureThread;
        //private bool _stopRequested = false;
        private CancellationTokenSource _cancellationTokenSource;

        // 固定路徑和參數（如果需要可以改用屬性或方法來調整）
        private const string SoftwarePath = @"D:\Miles\sigrok-cli-NIGHTLY-x86_64-release-installer";
        private const string BaseLogPath = @"D:\Miles\sigrok-log";
        private const int SampleRate = 2000000;
        private const string Channels = "D0=SCL,D2=SDA,D4=DET";
        private const int DelayMs = 1000;
        //private bool _isInitialized = false;

        // 建構子，讓使用者指定最大重試次數和擷取時間長度
        public SigrokCapture()
        {
            //_maxAttempts = maxAttempts;
            //_durationMs = durationMs;
        }

        // 新增一個使用Thread的方法來執行擷取
        public void StartCaptureWithThread(int maxAttempts, int durationMs)
        {
            int attempt = 0;
            string today = DateTime.Now.ToString("yyyyMMdd");

            // 檢查軟體路徑是否存在
            if (!Directory.Exists(SoftwarePath))
            {
                throw new DirectoryNotFoundException($"Sigrok軟體路徑不存在: {SoftwarePath}");
            }

            // 檢查並創建日誌目錄
            if (!Directory.Exists(BaseLogPath+"\\"+today))
            {
                Directory.CreateDirectory(BaseLogPath+"\\"+today);
            }

            // 重置停止標誌
            //_stopRequested = false;
            // 創建新的 CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            // 創建並啟動新線程
            _captureThread = new Thread(() =>
            {
                //while (attempt < _maxAttempts && !_stopRequested)
                //{
                //    string timestamp = DateTime.Now.ToString("HHmmss");
                //    //string logPath = $@"{BaseLogPath}\{today}_{timestamp}.sr";
                //    string logPath = Path.Combine(BaseLogPath, $"{today}_{timestamp}.sr");
                //    string command = $"sigrok-cli -d fx2lafw --config samplerate={SampleRate} --time {_durationMs} --channels {Channels} --frames 1 -t DET=r -O srzip -o {logPath}";

                //    ExecuteCommand(command);
                //    attempt++;
                //    Thread.Sleep(DelayMs);
                //}
                try
                {
                    while (attempt < maxAttempts && !cancellationToken.IsCancellationRequested)
                    {
                        string timestamp = DateTime.Now.ToString("HHmmss");
                        string logPath = Path.Combine(BaseLogPath, today, $"{today}_{timestamp}.sr");
                        string command = $"sigrok-cli -d fx2lafw --config samplerate={SampleRate} --time {durationMs} --channels {Channels} --frames 1 -t DET=r -O srzip -o {logPath}";

                        ExecuteCommand(command);
                        attempt++;

                        // 使用 CancellationToken 的 WaitHandle 來等待
                        // 這樣可以在等待期間響應取消請求
                        cancellationToken.WaitHandle.WaitOne(DelayMs);

                        // 檢查是否已請求取消
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                catch (OperationCanceledException)
                {
                    // 操作被取消，正常退出
                    Console.WriteLine("Capture operation was cancelled.");
                }
                catch (Exception ex)
                {
                    // 捕獲其他異常
                    Console.WriteLine($"Error in capture thread: {ex.Message}");
                }
            });

            // 設置為背景線程，這樣主程序結束時線程會自動終止
            _captureThread.IsBackground = true;
            _captureThread.Start();
        }

        // 添加一個方法來停止擷取
        public void StopCapture()
        {
            // 使用 CancellationTokenSource 來取消操作
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();

                // 可選：等待線程結束
                if (_captureThread != null && _captureThread.IsAlive)
                {
                    // 設置超時時間，避免無限等待
                    _captureThread.Join(100);
                }

                // 釋放 CancellationTokenSource
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        // 添加一個方法來檢查擷取是否正在進行
        public bool IsCapturing()
        {
            return _captureThread != null && _captureThread.IsAlive;
        }

        ////開始執行擷取
        //public void StartCapture()
        //{
        //    int attempt = 0;
        //    string today = DateTime.Now.ToString("yyyyMMdd");

        //    // Check if the log directory exists, if not, create it
        //    if (!Directory.Exists(BaseLogPath))
        //    {
        //        Directory.CreateDirectory(BaseLogPath);
        //    }

        //    while (attempt < _maxAttempts)
        //    {
        //        string timestamp = DateTime.Now.ToString("HHmmss");
        //        string logPath = $@"{BaseLogPath}\{today}_{timestamp}.sr";
        //        string command = $"sigrok-cli -d fx2lafw --config samplerate={SampleRate} --time {_durationMs} --channels {Channels} --frames 1 -t DET=r -O srzip -o {logPath}";

        //        ExecuteCommand(command);
        //        attempt++;
        //        Thread.Sleep(DelayMs);
        //    }
        //}

        //執行命令的私有方法
        private void ExecuteCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                Arguments = "/k"
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    using (var input = process.StandardInput)
                    {
                        if (input.BaseStream.CanWrite)
                        {
                            input.WriteLine($"cd {SoftwarePath}");
                            input.WriteLine(command);
                            //input.WriteLine("exit");
                        }
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    //Console.WriteLine("Output:");
                    //Console.WriteLine(output);

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Error:");
                        Console.WriteLine(error);
                    }

                    process.WaitForExit();
                }
            }
        }
    }
}
