using CodeHub.Core.Utilities;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public IReactiveCommand<object> GoToCommand { get; } = ReactiveCommand.Create();

        public string Name { get; }

        public string Description { get; }

        public int Stars { get; }

        public int Forks { get; }

        public GitHubAvatar Avatar { get; }

        public string Owner { get; }

        public RepositoryItemViewModel(string name, string description, string owner, int stars, int forks, GitHubAvatar avatar)
        {
            Name = name;
            Description = description;
            Owner = owner;
            Avatar = avatar;
            Stars = stars;
            Forks = forks;
        }
    }
}