using System.Reactive;
using ReactiveUI;

namespace CodeHub.Core.ViewModels
{
    public interface IPaginatableViewModel
    {
        IReactiveCommand<Unit> LoadMoreCommand { get; }
    }
}

