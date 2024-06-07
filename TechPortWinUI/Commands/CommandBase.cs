using System.Windows.Input;

namespace TechPortWinUI.Commands
{
    public abstract class CommandBase : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public virtual bool CanExecute(object? parameter) => true;

        public abstract void Execute(object? parameter);

        private void OnCanExecuteChange() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
