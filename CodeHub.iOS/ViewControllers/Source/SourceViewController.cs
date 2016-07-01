using System;
using UIKit;
using CodeHub.Core.ViewModels.Source;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.WebViews;
using Splat;

namespace CodeHub.ViewControllers.Source
{
    public class SourceViewController : FileSourceViewController<SourceViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.LoadCommand.IsExecuting.Subscribe(x => 
            {
                if (x) return;
                if (!string.IsNullOrEmpty(ViewModel.ContentPath))
                {
                    LoadSource(new Uri("file://" + ViewModel.ContentPath)).ToBackground();
                }
                else if (!string.IsNullOrEmpty(ViewModel.FilePath))
                {
                    LoadFile(ViewModel.FilePath);
                }
            });
        }

        protected override UIActionSheet CreateActionSheet(string title)
        {
            var vm = ViewModel;
            var sheet = base.CreateActionSheet(title);
            var editButton = vm.CanEdit ? sheet.AddButton("Edit") : -1;
            sheet.Dismissed += (sender, e) => BeginInvokeOnMainThread(() => {
                if (e.ButtonIndex == editButton)
                {
                    EditSource();
                }
            });
            return sheet;
        }

        private void EditSource()
        {
            var vm = ViewModel;
            var vc = new EditSourceViewController(vm.Username, vm.Repository, vm.Branch, vm.Path);
            vc.NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Images.Buttons.CancelButton };
            vc.NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => DismissViewController(true, null);
            PresentViewController(new ThemedNavigationController(vc), true, null);
        }

        async Task LoadSource(Uri fileUri)
        {
            var fontSize = (int)UIFont.PreferredSubheadline.PointSize;
            var content = System.IO.File.ReadAllText(fileUri.LocalPath, System.Text.Encoding.UTF8);

            if (ViewModel.IsMarkdown)
            {
                var markdownContent = await Locator.Current.GetService<IApplicationService>().Client.Markdown.GetMarkdown(content);
                var model = new DescriptionModel(markdownContent, fontSize);
                var htmlContent = new MarkdownView { Model = model };
                LoadContent(htmlContent.GenerateString());
            }
            else
            {
                var zoom = UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone;
                var model = new SourceBrowserModel(content, "idea", fontSize, zoom, fileUri.LocalPath);
                var contentView = new SyntaxHighlighterView { Model = model };
                LoadContent(contentView.GenerateString());
            }
        }
    }
}

