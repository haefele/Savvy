using System;

namespace Savvy.Common
{
    public class DisposableAction : IDisposable
    {
        private readonly Action _actionToExecuteOnDispose;

        public DisposableAction(Action actionToExecuteOnDispose)
        {
            this._actionToExecuteOnDispose = actionToExecuteOnDispose;
        }

        public void Dispose()
        {
            this._actionToExecuteOnDispose?.Invoke();
        }
    }
}