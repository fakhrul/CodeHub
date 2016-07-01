using ReactiveUI;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class TeamItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; }

        public IReactiveCommand<object> GoToCommand { get; } = ReactiveCommand.Create();

        public TeamItemViewModel(string name)
        {
            Name = name;
        }
    }
}