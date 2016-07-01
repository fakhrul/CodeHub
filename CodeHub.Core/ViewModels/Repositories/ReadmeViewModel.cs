using System;
using System.Text;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class ReadmeViewModel : LoadableViewModel
    {
        private readonly IMarkdownService _markdownService;

        public string RepositoryOwner { get; private set; }

        public string RepositoryName { get; private set; }

        private string _contentText;
        public string ContentText
        {
            get { return _contentText; }
            private set { this.RaiseAndSetIfChanged(ref _contentText, value); }
        }

        private ContentModel _contentModel;
        public ContentModel ContentModel
        {
            get { return _contentModel; }
            set { this.RaiseAndSetIfChanged(ref _contentModel, value); }
        }

        public string HtmlUrl
        {
            get { return _contentModel?.HtmlUrl; }
        }

        public ReadmeViewModel(string username, string repository)
        {
            RepositoryOwner = username;
            RepositoryName = repository;
            Title = "Readme";
            _markdownService = Locator.Current.GetService<IMarkdownService>();
        }

        protected override async Task Load()
        {
            var cmd = this.GetApplication().Client.Users[RepositoryOwner].Repositories[RepositoryName].GetReadme();
            var result = await this.GetApplication().Client.ExecuteAsync(cmd);
            ContentModel = result.Data;
            ContentText = _markdownService.Convert(Encoding.UTF8.GetString(Convert.FromBase64String(result.Data.Content)));
        }
    }
}
