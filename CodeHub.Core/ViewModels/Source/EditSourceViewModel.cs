using System;
using System.Threading.Tasks;
using CodeHub.Core.Messages;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Source
{
    public class EditSourceViewModel : LoadableViewModel
    {
        private string _text;
        public string Text
        {
            get { return _text; }
            private set { this.RaiseAndSetIfChanged(ref _text, value); }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public string Path { get; private set; }

        public string BlobSha { get; private set; }

        public string Branch { get; private set; }

        public EditSourceViewModel(string username, string repository, string branch = null, string path = null)
        {
            Username = username;
            Repository = repository;
            Branch = branch ?? "master";
            Path = path ?? string.Empty;

            if (!Path.StartsWith("/", StringComparison.Ordinal))
                Path = "/" + Path;
        }

        protected override async Task Load()
        {
            var request = this.GetApplication().Client.Users[Username].Repositories[Repository].GetContentFile(Path, Branch);
            var data = await this.GetApplication().Client.ExecuteAsync(request);
            BlobSha = data.Data.Sha;
            Text = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data.Data.Content));
        }

        public async Task Commit(string data, string message)
        {
            var request = this.GetApplication().Client.Users[Username].Repositories[Repository].UpdateContentFile(Path, message, data, BlobSha, Branch);
            var response = await this.GetApplication().Client.ExecuteAsync(request);
            Messenger.Publish(new SourceEditMessage { OldSha = BlobSha, Data = data, Update = response.Data });
        }
    }
}

