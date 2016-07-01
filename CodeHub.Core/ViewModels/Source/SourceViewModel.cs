using System.Threading.Tasks;
using CodeHub.Core.Messages;
using System.Linq;
using System;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceViewModel : FileSourceViewModel
    {
        private readonly IDisposable _editToken;
        private static readonly string[] MarkdownExtensions = { ".markdown", ".mdown", ".mkdn", ".md", ".mkd", ".mdwn", ".mdtxt", ".mdtext", ".text" };

        private string _name;
        private string _gitUrl;
        private bool _forceBinary;

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public string Branch { get; private set; }

        public bool TrueBranch { get; private set; }

        public string Path { get; private set; }

        protected override async Task Load()
        {
            var fileName = System.IO.Path.GetFileName(_name);
            var filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
            string mime = string.Empty;

            using (var stream = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                mime = await this.GetApplication().Client.DownloadRawResource2(_gitUrl, stream) ?? string.Empty;
            }

            FilePath = filepath;

            // We can force a binary representation if it was passed during init. In which case we don't care to figure out via the mime.
            if (_forceBinary)
                return;

            var isText = mime.Contains("charset");
            if (isText)
            {
                ContentPath = FilePath;
            }
        }

        public bool CanEdit
        {
            get { return ContentPath != null && TrueBranch; }
        }
  
        public SourceViewModel(string username, string repository, string branch, string path, string htmlUrl, string name, 
                               string gitUrl, bool forceBinary = false, bool trueBranch = false)
        {
            Path = path;
            HtmlUrl = htmlUrl;
            _name = name;
            _gitUrl = gitUrl;
            _forceBinary = forceBinary;
            Username = username;
            Repository = repository;
            Branch = branch;
            TrueBranch = trueBranch;

            //Create the filename
            var fileName = System.IO.Path.GetFileName(Path);
            if (fileName == null)
                fileName = Path.Substring(Path.LastIndexOf('/') + 1);

            //Create the temp file path
            Title = fileName;

            var extension = System.IO.Path.GetExtension(Path);
            IsMarkdown = MarkdownExtensions.Contains(extension);

            _editToken = Messenger.Subscribe<SourceEditMessage>(x =>
            {
                if (x.OldSha == null || x.Update == null)
                    return;
                _gitUrl = x.Update.Content.GitUrl;
                if (LoadCommand.CanExecute(null))
                    LoadCommand.Execute(true);
            });
        }
    }
}