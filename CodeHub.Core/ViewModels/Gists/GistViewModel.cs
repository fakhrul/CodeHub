using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewModel : LoadableViewModel
    {
        public string Id { get; }

        private GistModel _gist;
        public GistModel Gist
        {
            get { return _gist; }
            set { this.RaiseAndSetIfChanged(ref _gist, value); }
        }

        private bool _starred;
        public bool IsStarred
        {
            get { return _starred; }
            private set { this.RaiseAndSetIfChanged(ref _starred, value); }
        }

        public CollectionViewModel<GistCommentModel> Comments { get; } = new CollectionViewModel<GistCommentModel>();

        public ReactiveCommand<object> GoToFileSourceCommand { get; }

        public ReactiveCommand<object> GoToHtmlUrlCommand { get; }

        public ReactiveCommand<object> GoToUserCommand { get; }

        public ReactiveCommand<Unit> ForkCommand { get; }

        public ReactiveCommand<Unit> ToggleStarCommand { get; }

        public GistViewModel(GistModel gist)
            : this(gist.Id)
        {
            Gist = gist;
        }

        public GistViewModel(string id)
        {
            Id = id;

            Title = "Gist";

            var application = GetService<IApplicationService>();

            ForkCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var data = await application.Client.ExecuteAsync(this.GetApplication().Client.Gists[Id].ForkGist());
                NavigateTo(new GistViewModel(data.Data));
            });

            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist).Select(x => x != null));
            GoToHtmlUrlCommand.Subscribe(_ => NavigateTo(new WebBrowserViewModel(Gist.HtmlUrl)));

            GoToUserCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Gist.Owner.Login).Select(x => x != null));
            GoToUserCommand.Subscribe(_ => NavigateTo(new UserViewModel(Gist.Owner.Login)));

            GoToFileSourceCommand = ReactiveCommand.Create();
            GoToFileSourceCommand
                .OfType<GistFileModel>()
                .Select(x => new GistFileViewModel(x))
                .Subscribe(NavigateTo);

            ToggleStarCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                try
                {
                    var request = IsStarred ? application.Client.Gists[Id].Unstar() : application.Client.Gists[Id].Star();
                    await application.Client.ExecuteAsync(request);
                    IsStarred = !IsStarred;
                }
                catch
                {
                    DisplayAlert("Unable to start gist. Please try again.");
                }
            });
        }

        protected override Task Load()
        {
            var t1 = this.RequestModel(this.GetApplication().Client.Gists[Id].Get(), response => Gist = response.Data);
            this.RequestModel(this.GetApplication().Client.Gists[Id].IsGistStarred(), response => IsStarred = response.Data).ToBackground();
            Comments.SimpleCollectionLoad(this.GetApplication().Client.Gists[Id].GetComments()).ToBackground();
            return t1;
        }

        public async Task Edit(GistEditModel editModel)
        {
            var response = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Gists[Id].EditGist(editModel));
            Gist = response.Data;
        }
    }
}

