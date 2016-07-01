using CodeHub.Core.ViewModels.Source;
using CodeHub.DialogElements;
using System;
using UIKit;
using CodeHub.Views;

namespace CodeHub.ViewControllers.Source
{
    public class ChangesetBranchesView : ViewModelCollectionDrivenDialogViewController<ChangesetBranchesViewModel>
    {
        public ChangesetBranchesView()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitBranch.ToEmptyListImage(), "There are no branches."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var weakVm = new WeakReference<ChangesetBranchesViewModel>(ViewModel);
            BindCollection(ViewModel.Branches, x => {
                var e = new StringElement(x.Name);
                e.Clicked.Subscribe(_ => weakVm.Get()?.GoToBranchCommand.Execute(x));
                return e;
            });
        }
    }
}

