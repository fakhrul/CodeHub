using System;
using System.Threading.Tasks;
using GitHubSharp.Models;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Source
{
    public class BranchesAndTagsViewModel : LoadableViewModel
    {
        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public CollectionViewModel<ViewObject> Items { get; } = new CollectionViewModel<ViewObject>();

        public ReactiveCommand<object> GoToSourceCommand { get; }

        public BranchesAndTagsViewModel(string username, string repository, bool isShowingBranches = true)
        {
            Username = username;
            Repository = repository;
            SelectedFilter = isShowingBranches ? 0 : 1;

            Title = "Source";

            this.WhenAnyValue(x => x.SelectedFilter)
                .Skip(1)
                .InvokeCommand(LoadCommand);

            GoToSourceCommand = ReactiveCommand.Create();
            GoToSourceCommand.OfType<ViewObject>().Subscribe(obj =>
            {
                if (obj.Object is BranchModel)
                {
                    var x = obj.Object as BranchModel;
                    NavigateTo(new SourceTreeViewModel(Username, Repository, x.Name, trueBranch: true));
                }
                else if (obj.Object is TagModel)
                {
                    var x = obj.Object as TagModel;
                    NavigateTo(new SourceTreeViewModel(Username, Repository, x.Commit.Sha, trueBranch: false));
                }
            });
        }

        protected override Task Load()
        {
            if (SelectedFilter == 0)
            {
                var request = this.GetApplication().Client.Users[Username].Repositories[Repository].GetBranches();
                return this.RequestModel(request, response =>
                {
                    this.CreateMore(response, m => Items.MoreItems = m, d => Items.Items.AddRange(d.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x })));
                    Items.Items.Reset(response.Data.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x }));
                });
            }
            else
            {
                var request = this.GetApplication().Client.Users[Username].Repositories[Repository].GetTags();
                return this.RequestModel(request, response => 
                {
                    this.CreateMore(response, m => Items.MoreItems = m, d => Items.Items.AddRange(d.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x })));
                    Items.Items.Reset(response.Data.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x }));
                });
            }
        }

        public class ViewObject
        {
            public string Name { get; set; }
            public object Object { get; set; }
        }
    }
}

