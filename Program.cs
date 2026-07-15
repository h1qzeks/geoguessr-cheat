using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

class GeoGuessrAutocatcher
{
    // === 1. ВИНАПИ ИМПОРТЫ ===
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    // === 2. СТРУКТУРА ДЛЯ X64 ===
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public uint Alignment1; // Выравнивание для x64
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
        public uint Alignment2; // Дополнительное выравнивание
    }

    private static readonly Regex PanoidRegex = new Regex(@"panoid\W{1,5}([a-zA-Z0-9_-]{22})(?:\W|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static HashSet<string> seenPanoids = new HashSet<string>();
    private static DateTime lastBrowserOpenTime = DateTime.MinValue;

    // Выделяем буфер на 10 МБ заранее, чтобы не нагружать Garbage Collector
    private static byte[] sharedBuffer = new byte[1024 * 1024 * 10];

    static void Main(string[] args)
    {
        Console.WriteLine("[*] Запуск");
        Console.WriteLine("[*] Запустите geoguessr.exe");
        Console.WriteLine("[*] Ждём раунд...");

        while (true)
        {
            ScanProcessMemory("GeoGuessr");
            Thread.Sleep(2000);
        }
    }

    public static void ScanProcessMemory(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        if (processes.Length == 0) return;

        foreach (var proc in processes)
        {
            IntPtr hProcess = OpenProcess(0x0010 | 0x0400, false, proc.Id);
            if (hProcess == IntPtr.Zero) continue;

            IntPtr address = IntPtr.Zero;
            MEMORY_BASIC_INFORMATION mbi = new MEMORY_BASIC_INFORMATION();
            int mbiSize = Marshal.SizeOf(mbi);

            while (VirtualQueryEx(hProcess, address, out mbi, mbiSize) != 0)
            {
                if (mbi.State == 0x1000 && (mbi.Protect & 0x01) == 0 && (mbi.Protect & 0x100) == 0)
                {
                    long regionSize = mbi.RegionSize.ToInt64();

                    if (regionSize > 0 && regionSize <= sharedBuffer.Length)
                    {
                        IntPtr bytesRead;
                        if (ReadProcessMemory(hProcess, mbi.BaseAddress, sharedBuffer, (int)regionSize, out bytesRead))
                        {
                            int readCount = (int)bytesRead.ToInt64();

                            string asciiString = Encoding.UTF8.GetString(sharedBuffer, 0, readCount);
                            MatchMatches(asciiString);

                            string unicodeString = Encoding.Unicode.GetString(sharedBuffer, 0, readCount);
                            MatchMatches(unicodeString);
                        }
                    }
                }

                address = unchecked((IntPtr)(mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64()));
            }
            CloseHandle(hProcess);
        }
    }

    private static void MatchMatches(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        MatchCollection matches = PanoidRegex.Matches(text);

        foreach (Match match in matches)
        {
            if (match.Groups.Count < 2 || string.IsNullOrEmpty(match.Groups[1].Value)) continue;

            string value = match.Groups[1].Value;

            if (seenPanoids.Contains(value)) continue;

            double secondsSinceLastOpen = (DateTime.Now - lastBrowserOpenTime).TotalSeconds;

            if (secondsSinceLastOpen >= 18)
            {
                lastBrowserOpenTime = DateTime.Now;
                seenPanoids.Add(value);

                Console.WriteLine($"\n[+] НОВЫЙ РАУНД! PANOID: {value}");
                Console.WriteLine("[*] Открываю Google Maps...");

                string googleMapsUrl = $"https://www.google.com/maps/@?api=1&map_action=pano&pano={value}";

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = googleMapsUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[-] Ошибка браузера: {ex.Message}");
                }
            }
        }
    }
}