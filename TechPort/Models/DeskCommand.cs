using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TechPort.Helpers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Console = TechPort.Helpers.Console;

namespace TechPort.Models
{
    public class DeskCommand
    {
        static double MIN_HEIGHT = 0.62; //Minimum desk height in meters
        static double MAX_HEIGHT = 1.27; //Maximum desk height in meters

        const string UUID_HEIGHT = "99fa0021-338a-1024-8a49-009c0215f78a";
        const string UUID_COMMAND = "99fa0002-338a-1024-8a49-009c0215f78a";

        static readonly byte[] COMMAND_UP = new byte[] { 0x47, 0x00 };
        static readonly byte[] COMMAND_DOWN = new byte[] { 0x46, 0x00 };
        static readonly byte[] COMMAND_STOP = new byte[] { 0xFF, 0x00 };

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





        public static (short, UInt32) GetHeightAndSpeed(UInt32 data)
        {
            short speed = (short)(data >> 16);
            UInt32 height = data & (UInt32)0xFFFF;

            return (speed, height);
        }

        public short GetSpeed(UInt32 data)
        {
            var (speed, height) = GetHeightAndSpeed(data);
        }

        public UInt32 GetHeight(UInt32 data)
        {
            var (speed, height) = GetHeightAndSpeed(data);
        }


    }
}
