using CodeHub.Core.Utilities;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Username { get; }

        public GitHubAvatar Avatar { get; }

        public IReactiveCommand<object> GoToCommand { get; } = ReactiveCommand.Create();

        public UserItemViewModel(string username, GitHubAvatar avatar)
        {
            Username = username;
            Avatar = avatar;
        }
    }
}

