using CodeHub.Core.Utilities;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; }

        public GitHubAvatar Avatar { get; }

        public IReactiveCommand<object> GoToCommand { get; } = ReactiveCommand.Create();

        public OrganizationItemViewModel(string name, GitHubAvatar avatar)
        {
            Name = name;
            Avatar = avatar;
        }
    }
}

