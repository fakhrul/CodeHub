using System;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;

namespace CodeHub.Core.ViewModels
{
    public abstract class FileSourceViewModel : LoadableViewModel
    {
        private static readonly string[] BinaryMIMEs = new string[] 
        { 
            "image/", "video/", "audio/", "model/", "application/pdf", "application/zip", "application/gzip"
        };

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            protected set { this.RaiseAndSetIfChanged(ref _filePath, value); }
        }

        public bool IsMarkdown { get; protected set; }

        private string _contentPath;
        public string ContentPath
        {
            get { return _contentPath; }
            protected set { this.RaiseAndSetIfChanged(ref _contentPath, value); }
        }

        private string _htmlUrl;
        public string HtmlUrl
        {
            get { return _htmlUrl; }
            protected set { this.RaiseAndSetIfChanged(ref _htmlUrl, value); }
        }

        public IReactiveCommand<object> GoToHtmlUrlCommand { get; }

        protected FileSourceViewModel()
        {
            GoToHtmlUrlCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.HtmlUrl).Select(x => !string.IsNullOrEmpty(x)));
            GoToHtmlUrlCommand.Subscribe(_ => NavigateTo(new WebBrowserViewModel(HtmlUrl)));
        }

        protected static string CreatePlainContentFile(string data, string filename)
        {
            var filepath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), filename);
            System.IO.File.WriteAllText(filepath, data, System.Text.Encoding.UTF8);
            return filepath;
        }

        protected static bool IsBinary(string mime)
        {
            var lowerMime = mime.ToLower();
            return BinaryMIMEs.Any(lowerMime.StartsWith);
        }
    }
}

