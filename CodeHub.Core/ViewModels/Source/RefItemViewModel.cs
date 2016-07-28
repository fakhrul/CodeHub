using ReactiveUI;

namespace CodeHub.Core.ViewModels.Source
{
    public class RefItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; }

        public IReactiveCommand<object> GoToCommand { get; } = ReactiveCommand.Create();
        
        public RefItemViewModel(string name)
        {
            Name = name;
        }
    }
}

