using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace LR2NowPlaying
{
    class Program
    {
        static int Main(string[] args)
        {
            Process[] lr2Processes = Process.GetProcessesByName("LRHbody");
            if (lr2Processes.Length == 0)
            {
                Console.Error.WriteLine("LRHbody process not found");
                return 1;
            }
            Process lr2Process = lr2Processes[0];
            String dllPath = AppDomain.CurrentDomain.BaseDirectory + "/LR2mind.dll";
            uint dllPathLength = (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char)));

            IntPtr lr2ProcHandle = Imports.OpenProcess(Imports.PROCESS_CREATE_THREAD | Imports.PROCESS_QUERY_INFORMATION | Imports.PROCESS_VM_OPERATION | Imports.PROCESS_VM_WRITE | Imports.PROCESS_VM_READ, false, lr2Process.Id);
            if (lr2ProcHandle == IntPtr.Zero)
            {
                Console.Error.WriteLine("error while opening LRHbody process");
                return 1;
            }

            IntPtr loadLibraryAddr = Imports.GetProcAddress(Imports.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr mindBaseAddress = Imports.VirtualAllocEx(lr2ProcHandle, IntPtr.Zero, dllPathLength, Imports.MEM_COMMIT, Imports.PAGE_EXECUTE_READWRITE);

            UIntPtr bytesWritten;
            Imports.WriteProcessMemory(lr2ProcHandle, mindBaseAddress, Encoding.Default.GetBytes(dllPath), dllPathLength, out bytesWritten);

            IntPtr loadThread = Imports.CreateRemoteThread(lr2ProcHandle, IntPtr.Zero, 0, loadLibraryAddr, mindBaseAddress, 0, IntPtr.Zero);
            if (loadThread == IntPtr.Zero)
            {
                Console.Error.WriteLine("error while creating thread");
                return 1;
            }

            return 0;

            // Cleaning process

            Imports.WaitForSingleObject(loadThread, Imports.INFINITE);
            IntPtr baseAddressMind;
            Imports.GetExitCodeThread(loadThread, out baseAddressMind);
            Console.WriteLine("load_library_thread - detached " + baseAddressMind);

            Imports.CloseHandle(loadThread);
            Imports.VirtualFreeEx(lr2ProcHandle, mindBaseAddress, 0, Imports.MEM_RELEASE);

            IntPtr freeLibraryAddr = Imports.GetProcAddress(Imports.GetModuleHandle("kernel32.dll"), "FreeLibrary");
            IntPtr freeThread = Imports.CreateRemoteThread(lr2ProcHandle, IntPtr.Zero, 0, freeLibraryAddr, baseAddressMind, 0, IntPtr.Zero);

            Imports.WaitForSingleObject(freeThread, Imports.INFINITE);
            Imports.CloseHandle(freeThread);

            return 0;
        }
    }
}
