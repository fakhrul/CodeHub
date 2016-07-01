using System;
using System.Threading.Tasks;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Source
{
    public class ChangesetBranchesViewModel : LoadableViewModel
    {
        public string Username { get; }

        public string Repository { get; }

        public CollectionViewModel<BranchModel> Branches = new CollectionViewModel<BranchModel>();

        public IReactiveCommand<object> GoToBranchCommand { get; } = ReactiveCommand.Create();

        public ChangesetBranchesViewModel(string username, string repository)
        {
            Username = username;
            Repository = repository;

            Title = "Changeset Branch";

            GoToBranchCommand
                .OfType<BranchModel>()
                .Select(x => new ChangesetsViewModel(Username, Repository, x.Name))
                .Subscribe(NavigateTo);
        }

        protected override Task Load()
        {
            return Branches.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetBranches());
        }
    }
}

