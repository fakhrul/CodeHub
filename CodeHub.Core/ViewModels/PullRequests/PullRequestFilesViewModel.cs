using System;
using System.Linq;
using System.Threading.Tasks;
using GitHubSharp.Models;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Core.ViewModels.Source;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestFilesViewModel : LoadableViewModel
    {
        public CollectionViewModel<CommitModel.CommitFileModel> Files { get; } = new CollectionViewModel<CommitModel.CommitFileModel>();

        public long PullRequestId { get; private set; }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public IReactiveCommand<object> GoToSourceCommand { get; } = ReactiveCommand.Create();

        public PullRequestFilesViewModel(string username, string repository, long id)
        {
            Username = username;
            Repository = repository;
            PullRequestId = id;

            Title = "Files";

            GoToSourceCommand
                .OfType<CommitModel.CommitFileModel>()
                .Subscribe(x =>
                {
                    var name = x.Filename.Substring(x.Filename.LastIndexOf("/", StringComparison.Ordinal) + 1);
                    NavigateTo(new SourceViewModel(Username, Repository, null, x.Filename, null, name, x.ContentsUrl, x.Patch == null));
                });


            Files.GroupingFunction = (x) => x.GroupBy(y => {
                var filename = "/" + y.Filename;
                return filename.Substring(0, filename.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
            }).OrderBy(y => y.Key);
        }

        protected override Task Load()
        {
            return Files.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[PullRequestId].GetFiles());
        }
    }
}

