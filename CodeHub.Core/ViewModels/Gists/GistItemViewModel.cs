using System;
using ReactiveUI;
using CodeHub.Core.Utilities;
using Humanizer;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public GitHubAvatar Avatar { get; }

        public string Title { get; }

        public string Description { get; }

        public DateTimeOffset UpdatedAt { get; }

        public string UpdatedString { get; }

        public IReactiveCommand<object> GoToCommand { get; } = ReactiveCommand.Create();

        public GistItemViewModel(string title, string avatarUrl, string description, DateTimeOffset updatedAt)
        {
            Title = title;
            Description = description;
            UpdatedAt = updatedAt;
            UpdatedString = UpdatedAt.Humanize();
            Avatar = new GitHubAvatar(avatarUrl);
        }
    }
}