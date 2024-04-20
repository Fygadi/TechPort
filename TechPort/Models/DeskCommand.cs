using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechPort.Helpers;

namespace TechPort.Models
{
    public class DeskCommand
    {
        const double MIN_HEIGHT = 0.62; //Minimum desk height in meters
        const double MAX_HEIGHT = 1.27; //Maximum desk height in meters

        const string UUID_HEIGHT = "99fa0021-338a-1024-8a49-009c0215f78a";
        const string UUID_COMMAND = "99fa0002-338a-1024-8a49-009c0215f78a";

        static readonly byte[] COMMAND_UP =  { 0x47, 0x00 };
        static readonly byte[] COMMAND_DOWN =  { 0x46, 0x00 };
        static readonly byte[] COMMAND_STOP = { 0xFF, 0x00 };

        public async Task<bool> MoveUpAsync()
        {
            return await BluetoothService.WriteAsync(null, COMMAND_UP);
        }

        public async Task<bool> MoveDownAsync()
        {
            return await BluetoothService.WriteAsync(null, COMMAND_UP);
        }

        public bool MoveToPosition()
        {
            // TODO: Implement the logic
            return false;
        }

        public List<uint> GetSavedPosition()
        {
            // TODO: Implement the logic
            return null;
        }

        public bool SavePosition(uint position, uint index)
        {
            // TODO: Implement the logic
            return false;
        }

        public UInt32 GetCurrentHeight()
        {
            // TODO: Implement the logic.
            return 0;
        }

        public short GetSpeed(UInt32 data) => GetHeightAndSpeed(data).Item1;
        public UInt32 GetHeight(UInt32 data) => GetHeightAndSpeed(data).Item2;
        public static (short, UInt32) GetHeightAndSpeed(UInt32 data)
        {
            short speed = (short)(data >> 16);
            UInt32 height = data & (UInt32)0xFFFF;

            return (speed, height);
        }
    }
}
