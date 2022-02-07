using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MemoryUtil
{
    public class MemoryHelper
    {
        [DllImport("kernel32.dll")]
        protected static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

        private Process p;
        private Dictionary<string, Thread> freezeTimer;

        public MemoryHelper(Process p)
        {
            this.p = p;
            freezeTimer = new Dictionary<string, Thread>();
        }

        public void FreezeAddress(string name, DataType type, string write, params UIntPtr[] address)
        {
            if (!freezeTimer.ContainsKey(name))
            {
                Thread t = new Thread(() =>
                {
                    while (true)
                    {
                        foreach (var i in address)
                        {
                            WriteMemory(i, type, write);
                        }
                    }
                });

                freezeTimer.Add(name, t);
                t.Start();
            }
            else
            {
                throw new Exception("The name is already Contains");
            }
        }

        public void FreezePointer(string name, int offset, DataType type, string write)
        {
            if (!freezeTimer.ContainsKey(name))
            {
                Thread t = new Thread(() =>
                {
                    while (true)
                    {
                        WritePointer(offset, type, write);
                    }
                });

                freezeTimer.Add(name, t);
                t.Start();
            }
            else
            {
                throw new Exception("The name is already Contains");
            }
        }

        public void UnFreeze(params string[] name)
        {
            if (name == null) return;

            for (int i = 0; i < name.Length; ++i)
            {
                if (freezeTimer.ContainsKey(name[i]))
                {
                    freezeTimer.TryGetValue(name[i], out Thread thread);
                    thread.Abort();
                    thread.Join();
                    freezeTimer.Remove(name[i]);
                }
            }
        }

        public void UnFreezeAll()
        {
            freezeTimer = new Dictionary<string, Thread>();
        }

        public bool? WritePointer(int offset, DataType type, string write)
        {
            IntPtr address = IntPtr.Add(p.MainModule.BaseAddress, offset);
            byte[] result = ConvertToBytes(type, write);

            if (result != null)
                return WriteProcessMemory(p.Handle, address, result, (UIntPtr)result.Length, IntPtr.Zero);
            return null;
        }

        public bool? WriteMemory(UIntPtr address, DataType type, string write)
        {
            byte[] result = ConvertToBytes(type, write);
            if (result != null)
                return WriteProcessMemory(p.Handle, address, result, (UIntPtr)result.Length, IntPtr.Zero);
            else
                return null;
        }

        public T ReadMemory<T>(UIntPtr address, uint size)
        {
            byte[] buffer = new byte[size];
            ReadProcessMemory(p.Handle, address, buffer, size, 0);
            return BytesTo<T>(buffer);
        }

        public static T BytesTo<T>(byte[] array)
        {
            Type type = typeof(T);

            if (type == typeof(float))
                return (T)(object)BitConverter.ToSingle(array, 0);

            if (type == typeof(int))
                return (T)(object)BitConverter.ToInt32(array, 0);

            if (type == typeof(long))
                return (T)(object)BitConverter.ToInt64(array, 0);

            if (type == typeof(double))
                return (T)(object)BitConverter.ToDouble(array, 0);

            if (type == typeof(string))
                return (T)(object)Encoding.UTF8.GetString(array);

            throw new Exception("지원하지 않는 데이터타입입니다.");
        }

        public static byte[] ConvertToBytes(DataType type, string value)
        {
            byte[] bytes;

            switch (type)
            {
                case DataType.FLOAT:
                    bytes = BitConverter.GetBytes(Convert.ToSingle(value));
                    break;
                case DataType.INT:
                    bytes = BitConverter.GetBytes(Convert.ToInt32(value));
                    break;
                case DataType.DOUBLE:
                    bytes = BitConverter.GetBytes(Convert.ToDouble(value));
                    break;
                case DataType.LONG:
                    bytes = BitConverter.GetBytes(Convert.ToInt64(value));
                    break;
                case DataType.BYTE:
                    bytes = new byte[1];
                    bytes[0] = Convert.ToByte(value, 16);
                    break;
                case DataType.ARRAY_OF_BYTE:
                    if (value.Contains(" "))
                    {
                        string[] stringBytes;
                        stringBytes = value.Split(' ');

                        int c = stringBytes.Length;
                        bytes = new byte[c];
                        for (int i = 0; i < c; i++)
                        {
                            bytes[i] = Convert.ToByte(stringBytes[i], 16);
                        }
                    }
                    else
                    {
                        bytes = new byte[1];
                        bytes[0] = Convert.ToByte(value, 16);
                    }
                    break;
                case DataType.STRING:
                    bytes = System.Text.Encoding.UTF8.GetBytes(value);
                    break;
                default:
                    throw new Exception("This type is not convertable.");
            }
            return bytes;
        }
    }
}
