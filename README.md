# ONLY SUPPORT 32Bits Game
## How to use?
#### MemoryHelper - Code Example
```c#
using MemoryUtil;

public class ExampleForMemoryHelper 
{
    private void example()
    {
        Process p = Process.GetProcessesByName("ldboxheadless"/*your process name*/).First();

        MemoryHelper helper = new MemoryHelper(p);

        UIntPtr addr1 = new UIntPtr(0x12345678);

        helper.ReadMemory<string>(addr1, 16); // 16 bytes size string
        helper.ReadMemory<float>(addr1, 4);
        helper.ReadMemory<double>(addr1, 8);
        helper.ReadMemory<int>(addr1, 4);

        helper.WriteMemory(addr1, MHelper.DataType.ARRAY_OF_BYTE, "12 AB 56 CD");
        helper.WriteMemory(addr1, MHelper.DataType.INT, "12345");

        helper.WritePointer(0x12345, MemoryHelper.DataType.INT, "12345"); // same to CheatEngine+0x12345

        helper.FreezeAddress("WallHack1", addr1, MemoryHelper.DataType.FLOAT, "0"); // Freeze to Float 0

        helper.FreezeAddress("WallHack1", addr1, MemoryHelper.DataType.FLOAT, "0"); // ERROR:same name

        helper.FreezePointer("WallHack2", 0x123456, MemoryHelper.DataType.FLOAT, "0");

        helper.UnFreeze("WallHack1", "WallHack2"); // unfreeze
    }
}
```
-Easy for using
-Simple Functions

#### MemoryScanner - Code Example
```c#
using MemoryUtil;

public class ExampleForMemoryScanner
{
    private void example()
    {
        Process p = Process.GetProcessesByName("ldboxheadless"/*your process name*/).First();

        MemoryScanner scanner = new MemoryScanner((uint)p.Id);
        var result = scanner.AoBScan("12 34 56 ?? 99 11 AA BB ?? ?? FD");
        var result2 scanner.AoBScan(new byte[] {0x00, 0x12, 0x34, 0x56});
        for(var address in result) {
            // TODO
        }
    }
}
```
