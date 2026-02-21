using System;
using System.Windows.Input;

namespace NextBala.Commands
{
    // Versão sem parâmetro (mantida para compatibilidade)
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // NOVA: Versão com parâmetro genérico
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Predicate<T?>? _canExecute;

        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter == null && typeof(T).IsValueType)
                return _canExecute(default);

            if (parameter is T typedParameter)
                return _canExecute(typedParameter);

            return false;
        }

        public void Execute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
            {
                _execute(default);
            }
            else if (parameter is T typedParameter)
            {
                _execute(typedParameter);
            }
            else
            {
                // Tenta converter se possível
                try
                {
                    var converted = (T)Convert.ChangeType(parameter, typeof(T));
                    _execute(converted);
                }
                catch
                {
                    _execute(default);
                }
            }
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}