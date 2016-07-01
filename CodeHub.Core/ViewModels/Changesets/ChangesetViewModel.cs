using System;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Source;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using Splat;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class ChangesetViewModel : LoadableViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IFeaturesService _featuresService;

        public string Node { get; }

        public string Username { get; }

        public string Repository { get; }

        public bool ShowRepository { get; }

        private CommitModel _commitModel;
        public CommitModel Changeset
        {
            get { return _commitModel; }
            private set { this.RaiseAndSetIfChanged(ref _commitModel, value); }
        }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        //public ICommand GoToFileCommand
        //{
        //    get
        //    { 
        //        return new MvxCommand<CommitModel.CommitFileModel>(x =>
        //        {
        //                if (x.Patch == null)
        //                {
        //                    ShowViewModel<SourceViewModel>(new SourceViewModel.NavObject { GitUrl = x.ContentsUrl, HtmlUrl = x.BlobUrl, Name = x.Filename, Path = x.Filename, ForceBinary = true });
        //                }
        //                else
        //                {
        //                    Mvx.Resolve<CodeHub.Core.Services.IViewModelTxService>().Add(x);
        //                    ShowViewModel<ChangesetDiffViewModel>(new ChangesetDiffViewModel.NavObject { Username = User, Repository = Repository, Branch = _commitModel.Sha, Filename = x.Filename });
        //                }

        //        });
        //    }
        //}

        public ReactiveCommand<object> GoToHtmlUrlCommand { get; }

        public ReactiveCommand<object> GoToRepositoryCommand { get; }

        public ReactiveCommand<object> GoToFileCommand { get; }

        public CollectionViewModel<CommentModel> Comments { get; } = new CollectionViewModel<CommentModel>();

        public ReactiveCommand<object> GoToOwner { get; }
     
        public ChangesetViewModel(string username, string repository, string node, bool showRepository = false)
        {
            _applicationService = Locator.Current.GetService<IApplicationService>();
            _featuresService = Locator.Current.GetService<IFeaturesService>();

            Username = username;
            Repository = repository;
            Node = node;
            ShowRepository = showRepository;
            Title = "Commit " + (Node.Length > 6 ? Node.Substring(0, 6) : Node);

            GoToOwner = ReactiveCommand.Create(this.WhenAnyValue(x => x.Changeset).Select(x => x?.Author?.Login != null));
            GoToOwner
                .Select(_ => new UserViewModel(Changeset?.Author?.Login))
                .Subscribe(NavigateTo);

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Changeset).Select(x => x != null));
            GoToHtmlUrlCommand
                .Select(_ => new WebBrowserViewModel(Changeset.Url))
                .Subscribe(NavigateTo);

            GoToRepositoryCommand = ReactiveCommand.Create();
            GoToRepositoryCommand
                .Select(_ => new RepositoryViewModel(Username, Repository))
                .Subscribe(NavigateTo);

            GoToFileCommand = ReactiveCommand.Create();
            GoToFileCommand
                .OfType<CommitModel.CommitFileModel>()
                .Select<CommitModel.CommitFileModel, BaseViewModel>(x =>
                {
                    if (x.Patch == null)
                        return new SourceViewModel(Username, Repository, null, x.Filename, x.BlobUrl, x.Filename, x.ContentsUrl, true);
                    return new ChangesetDiffViewModel(Username, Repository, _commitModel.Sha, x.Filename, x);
                })
                .Subscribe(NavigateTo);
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

            var t1 = this.RequestModel(_applicationService.Client.Users[Username].Repositories[Repository].Commits[Node].Get(), response => Changeset = response.Data);
            Comments.SimpleCollectionLoad(_applicationService.Client.Users[Username].Repositories[Repository].Commits[Node].Comments.GetAll()).ToBackground();
            return t1;
        }

        public async Task AddComment(string text)
        {
            var c = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Users[Username].Repositories[Repository].Commits[Node].Comments.Create(text));
            Comments.Items.Add(c.Data);
        }
    }
}

