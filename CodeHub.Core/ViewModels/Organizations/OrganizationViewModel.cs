using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Organizations
{
    public class OrganizationViewModel : LoadableViewModel
    {
        public string Username { get; }

        private UserModel _userModel;
        public UserModel Organization
        {
            get { return _userModel; }
            private set { this.RaiseAndSetIfChanged(ref _userModel, value); }
        }

        public OrganizationViewModel(string organizationName) 
        {
            Username = organizationName;
            Title = Username;

            Observable.Merge<IBaseViewModel>(
                GoToMembersCommand.Select(_ => new OrganizationMembersViewModel(Username)),
                GoToTeamsCommand.Select(_ => new TeamsViewModel(Username)),
                GoToFollowersCommand.Select(_ => new UserFollowersViewModel(Username)),
                GoToEventsCommand.Select(_ => new UserEventsViewModel(Username)),
                GoToGistsCommand.Select(_ => new UserGistsViewModel(Username)),
                GoToRepositoriesCommand.Select(_ => new OrganizationRepositoriesViewModel(Username)))
                .Subscribe(NavigateTo);
        }

        public ReactiveCommand<object> GoToMembersCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToTeamsCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToFollowersCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToEventsCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToGistsCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> GoToRepositoriesCommand { get; } = ReactiveCommand.Create();

        protected override Task Load()
        {
            return this.RequestModel(this.GetApplication().Client.Organizations[Username].Get(), response => Organization = response.Data);
        }
    }
}

