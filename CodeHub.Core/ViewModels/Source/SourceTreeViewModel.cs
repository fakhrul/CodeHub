using System;
using System.Threading.Tasks;
using GitHubSharp.Models;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceTreeViewModel : LoadableViewModel
    {
        private readonly IFeaturesService _featuresService;
        private readonly IApplicationService _applicationService;

        public CollectionViewModel<ContentModel> Content { get; } = new CollectionViewModel<ContentModel>();

        public string Username { get; }

        public string Path { get; }

        public string Branch { get; }

        public bool TrueBranch { get; }

        public string Repository { get; }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            private set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        public IReactiveCommand<object> GoToItemCommand { get; }
            
        public SourceTreeViewModel(string username, string repository, string branch, string path = null, bool trueBranch = false)
        {
            _applicationService = Locator.Current.GetService<IApplicationService>();
            _featuresService = Locator.Current.GetService<IFeaturesService>();

            Username = username;
            Repository = repository;
            Branch = branch;
            Path = path ?? "/";
            TrueBranch = trueBranch;

            GoToItemCommand = ReactiveCommand.Create();
            GoToItemCommand.OfType<ContentModel>().Subscribe(x => {
                if (x.Type.Equals("dir", StringComparison.OrdinalIgnoreCase))
                {
                    NavigateTo(new SourceTreeViewModel(Username, Repository, Branch, x.Path, TrueBranch));
                }
                if (x.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
                {
                    if (x.DownloadUrl == null)
                    {
                        var nameAndSlug = x.GitUrl.Substring(x.GitUrl.IndexOf("/repos/", StringComparison.Ordinal) + 7);
                        var indexOfGit = nameAndSlug.LastIndexOf("/git", StringComparison.Ordinal);
                        indexOfGit = indexOfGit < 0 ? 0 : indexOfGit;
                        var repoId = RepositoryIdentifier.FromFullName(nameAndSlug.Substring(0, indexOfGit));
                        if (repoId == null)
                            return;

                        var sha = x.GitUrl.Substring(x.GitUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);
                        NavigateTo(new SourceTreeViewModel(repoId?.Owner, repoId?.Name, sha));
                    }
                    else
                    {
                        NavigateTo(new SourceViewModel(Username, Repository, Branch, x.Path, x.HtmlUrl, x.Name, x.GitUrl, trueBranch: TrueBranch));
                    }
                }
            });
        }

        protected override Task Load()
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
            {
                var request = _applicationService.Client.Users[Username].Repositories[Repository].Get();
                _applicationService.Client.ExecuteAsync(request)
                    .ToBackground(x => ShouldShowPro = x.Data.Private && !_featuresService.IsProEnabled);
            }
            
            return Content.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetContent(Path, Branch));
        }
    }
}

