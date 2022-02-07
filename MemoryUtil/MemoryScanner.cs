using MemoryUtil.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoryUtil
{

    public class MemoryScanner
    {
        private List<MEMORY_BASIC_INFORMATION> memoryRegion;
        protected uint pid;
        public int percent = 0;

        public MemoryScanner(uint pid)
        {
            this.pid = pid;
            memoryRegion = new List<MEMORY_BASIC_INFORMATION>();
        }

        [DllImport("kernel32.dll")]
        protected static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        protected static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

        protected void GetMemInfo(IntPtr pHandle)
        {
            IntPtr Addy = new IntPtr();
            while (true)
            {
                MEMORY_BASIC_INFORMATION memInfo = new MEMORY_BASIC_INFORMATION();
                int memDump = VirtualQueryEx(pHandle, Addy, out memInfo, Marshal.SizeOf(memInfo));
                if (memDump != 0)
                {
                    if ((memInfo.State & 0x1000) != 0 && (memInfo.Protect & 0x100) == 0)
                        memoryRegion.Add(memInfo);

                    Addy = new IntPtr(memInfo.BaseAddress.ToInt32() + (int)memInfo.RegionSize);
                }
                else
                    break;
            }
        }

        private static byte[] ConvertHexStringToByte(string convertString)
        {
            convertString = convertString.Trim().Replace(" ", "").Replace("??", "00").Replace("?", "00");

            byte[] convertArr = new byte[convertString.Length / 2];

            for (int i = 0; i < convertArr.Length; i++)
            {
                convertArr[i] = Convert.ToByte(convertString.Substring(i * 2, 2), 16);
            }
            return convertArr;
        }

        private static bool[] GetMaskArray(string[] pattern)
        {
            bool[] flags = new bool[pattern.Length];

            for (int i = 0; i < pattern.Length; ++i)
            {
                flags[i] = pattern[i].Contains("?");
            }

            return flags;
        }

        private List<IntPtr> FindPattern(byte[] buffer, string pattern)
        {
            string[] patternArray = pattern.Split(' ');
            byte[] patternBytes = ConvertHexStringToByte(pattern);
            bool[] mask = GetMaskArray(patternArray);

            List<IntPtr> result = new List<IntPtr>();

            for (int i = 0; i < buffer.Length - patternArray.Length; ++i)
            {
                bool found = true;

                for (int j = 0; j < patternArray.Length; ++j)
                {
                    if (
                        !mask[j] &&
                        buffer[i + j] != patternBytes[j]
                        )
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    result.Add(new IntPtr(i));
            }

            if (result.Count == 0)
                return null;
            return result;
        }

        private byte[] GetMemoryBuffer(IntPtr handle, MEMORY_BASIC_INFORMATION mbi)
        {
            byte[] buffer = new byte[mbi.RegionSize];
            ReadProcessMemory(handle, mbi.BaseAddress, buffer, mbi.RegionSize, 0);

            return buffer;
        }

        public List<uint> AoBScan(byte[] Pattern)
        {
            return AoBScan(Encoding.Default.GetString(Pattern));
        }

        public List<uint> AoBScan(string Pattern)
        {
            int temp = 0;
            Process p = Process.GetProcessById((int)this.pid);
            if (p.Id == 0) return null;
            memoryRegion = new List<MEMORY_BASIC_INFORMATION>();
            List<uint> address = new List<uint>();
            GetMemInfo(p.Handle);

            percent = 0;

            Parallel.For(0, memoryRegion.Count, i =>
            {
                temp++;
                percent = (int)((((float)temp) / memoryRegion.Count) * 100);
                byte[] buffer = GetMemoryBuffer(p.Handle, memoryRegion[i]);
                List<IntPtr> result = FindPattern(buffer, Pattern);

                if (result != null)
                    result.ForEach(res =>
                        address.Add((uint)memoryRegion[i].BaseAddress.ToInt32() + (uint)res.ToInt32()));
            });

            if (address.Count == 0)
                return null;

            address.Sort();
            return address;
        }
    }
}
