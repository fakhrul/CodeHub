using CodeHub.Core.Utilities;
using GitHubSharp.Models;
using Humanizer;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Title { get; }

        public GitHubAvatar Avatar { get; }

        public IReactiveCommand<object> GoToCommand { get; } = ReactiveCommand.Create();

        public string Details { get; }

        internal PullRequestItemViewModel(PullRequestModel pullRequest)
        {
            var login = pullRequest?.User.Login ?? "Unknonwn User";
            var avatar = pullRequest?.User.AvatarUrl;
            Title = pullRequest.Title ?? "No Title";
            Avatar = new GitHubAvatar(avatar);
            Details = string.Format("#{0} opened {1} by {2}", pullRequest.Number, pullRequest.CreatedAt.UtcDateTime.Humanize(), login);
        }
    }
}

