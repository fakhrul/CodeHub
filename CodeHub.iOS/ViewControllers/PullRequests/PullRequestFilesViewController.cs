using CodeHub.ViewControllers;
using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using System;
using CodeHub.DialogElements;

namespace CodeHub.Views.PullRequests
{
    public class PullRequestFilesViewController : ViewModelCollectionDrivenDialogViewController<PullRequestFilesViewModel>
    {
        public PullRequestFilesViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.FileCode.ToEmptyListImage(), "There are no files."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var weakVm = new WeakReference<PullRequestFilesViewModel>(ViewModel);
            BindCollection(ViewModel.Files, x =>
            {
                var name = x.Filename.Substring(x.Filename.LastIndexOf("/", StringComparison.Ordinal) + 1);
                var el = new StringElement(name, x.Status, UITableViewCellStyle.Subtitle);
                el.Image = Octicon.FileCode.ToImage();
                el.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                el.Clicked.Subscribe(_ => weakVm.Get()?.GoToSourceCommand.Execute(x));
                return el;
            });
        }

        public override DialogViewController.Source CreateSizingSource()
        {
            return new CustomSource(this);
        }
    
        private class CustomSource : DialogViewController.Source
        {
            public CustomSource(PullRequestFilesViewController parent)
                : base(parent)
            {
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
            {
                var x = headerView as UITableViewHeaderFooterView;
                if (x != null)
                {
                    x.TextLabel.LineBreakMode = UILineBreakMode.HeadTruncation;
                }
            }
        }
    }
}



