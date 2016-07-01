using System;
using CodeHub.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using GitHubSharp.Models;
using CodeHub.ViewControllers.Repositories;
using CodeHub.DialogElements;
using System.Reactive.Linq;
using ReactiveUI;

namespace CodeHub.Views.Source
{
    public class SourceTreeViewController : ViewModelCollectionDrivenDialogViewController<SourceTreeViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            BindCollection(ViewModel.Content, CreateElement);
            ViewModel.WhenAnyValue(x => x.ShouldShowPro)
                     .Where(x => x)
                     .Subscribe(_ => this.ShowPrivateView());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (string.IsNullOrEmpty(ViewModel.Path))
                Title = ViewModel.Repository;
            else
            {
                var path = ViewModel.Path.TrimEnd('/');
                Title = path.Substring(path.LastIndexOf('/') + 1);
            } 
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }

        private Element CreateElement(ContentModel x)
        {
            var weakVm = new WeakReference<SourceTreeViewModel>(ViewModel);
            if (x.Type.Equals("dir", StringComparison.OrdinalIgnoreCase))
            {
                var e = new StringElement(x.Name, Octicon.FileDirectory.ToImage());
                e.Clicked.Subscribe(_ => weakVm.Get()?.GoToItemCommand.Execute(x));
                return e;
            }
            if (x.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                if (x.DownloadUrl != null)
                {
                    var e = new StringElement(x.Name, Octicon.FileCode.ToImage());
                    e.Clicked.Subscribe(_ => weakVm.Get()?.GoToItemCommand.Execute(x));
                    return e;
                }
                else
                {
                    var e = new StringElement(x.Name, Octicon.FileSubmodule.ToImage());
                    e.Clicked.Subscribe(_ => weakVm.Get()?.GoToItemCommand.Execute(x));
                    return e;
                }
            }
            
            return new StringElement(x.Name) { Image = Octicon.FileMedia.ToImage() };
        }
    }
}

