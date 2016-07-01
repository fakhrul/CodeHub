using System;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Organizations;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Users
{
    public class UserViewModel : LoadableViewModel
    {
        public string Username { get; }

        private UserModel _user;
        public UserModel User
        {
            get { return _user; }
            private set { this.RaiseAndSetIfChanged(ref _user, value); }
        }

        private bool _isFollowing;
        public bool IsFollowing
        {
            get { return _isFollowing; }
            private set { this.RaiseAndSetIfChanged(ref _isFollowing, value); }
        }

        public bool IsLoggedInUser => string.Equals(Username, this.GetApplication().Account.Username);

        public ReactiveCommand<object> GoToOrganizationsCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToFollowingCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToFollowersCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToEventsCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToGistsCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToRepositoriesCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<Unit> ToggleFollowingCommand { get; }

        public UserViewModel(string username)
        {
            Title = Username = username;

            var application = GetService<IApplicationService>();

            ToggleFollowingCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                try
                {
                    if (IsFollowing)
                        await application.Client.ExecuteAsync(application.Client.AuthenticatedUser.Unfollow(Username));
                    else
                        await application.Client.ExecuteAsync(application.Client.AuthenticatedUser.Follow(Username));
                    IsFollowing = !IsFollowing;
                }
                catch
                {
                    DisplayAlert("Unable to follow user! Please try again.");
                }
            });

            Observable.Merge<IBaseViewModel>(
                GoToOrganizationsCommand.Select(_ => new OrganizationsViewModel(Username)),
                GoToFollowingCommand.Select(_ => new UserFollowingsViewModel(Username)),
                GoToFollowersCommand.Select(_ => new UserFollowersViewModel(Username)),
                GoToEventsCommand.Select(_ => new UserEventsViewModel(Username)),
                GoToGistsCommand.Select(_ => new UserGistsViewModel(Username)),
                GoToRepositoriesCommand.Select(_ => new UserRepositoriesViewModel(Username)))
                .Subscribe(NavigateTo);
        }

        protected override Task Load()
        {
            this.RequestModel(this.GetApplication().Client.AuthenticatedUser.IsFollowing(Username), x => IsFollowing = x.Data).ToBackground();
            return this.RequestModel(this.GetApplication().Client.Users[Username].Get(), response => User = response.Data);
        }
    }
}

