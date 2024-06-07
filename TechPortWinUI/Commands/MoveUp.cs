using TechPortWinUI.Desk;

namespace TechPortWinUI.Commands
{
    internal class MoveUp : CommandBase
    {
        private readonly IDesk _activeDesk;

        public MoveUp(ref IDesk activeDesk)
        {
            _activeDesk = activeDesk;
        }

        public override void Execute(object? parameter)
        {
            _activeDesk.MoveUpAsync();
        }

        public override bool CanExecute(object? parameter)
        {
            return (_activeDesk.BluetoothConnectionStatus == Windows.Devices.Bluetooth.BluetoothConnectionStatus.Connected);
        }
    }
}
