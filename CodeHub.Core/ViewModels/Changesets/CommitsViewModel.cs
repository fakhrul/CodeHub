using GitHubSharp.Models;
using System.Threading.Tasks;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.Services;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.Changesets
{
    public abstract class CommitsViewModel : LoadableViewModel
    {
        private readonly IFeaturesService _featuresService;
        private readonly IApplicationService _applicationService;

        public string Username { get; }

        public string Repository { get; }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            protected set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        public CollectionViewModel<CommitModel> Commits { get; } = new CollectionViewModel<CommitModel>();

        protected CommitsViewModel(string username, string repository)
        {
            _applicationService = Locator.Current.GetService<IApplicationService>();
            _featuresService = Locator.Current.GetService<IFeaturesService>();

            Title = "Commits";
        }

        protected override Task Load()
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
            {
                var repoRequest = _applicationService.Client.Users[Username].Repositories[Repository].Get();
                _applicationService.Client.ExecuteAsync(repoRequest)
                    .ToBackground(x => ShouldShowPro = x.Data.Private && !_featuresService.IsProEnabled);
            }
            
            return Commits.SimpleCollectionLoad(GetRequest());
        }

        protected abstract GitHubRequest<List<CommitModel>> GetRequest();
    }
}

