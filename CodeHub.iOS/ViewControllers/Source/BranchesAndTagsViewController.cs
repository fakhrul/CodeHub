using System;
using CodeHub.Core.ViewModels.Source;
using UIKit;
using ReactiveUI;

namespace CodeHub.ViewControllers.Source
{
    public class BranchesAndTagsViewController : BaseViewController<BranchesAndTagsViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var branchesViewController = new BranchesViewController { ViewModel = ViewModel.Branches };
            var tagsViewController = new TagsViewController { ViewModel = ViewModel.Tags };

            AddChildViewController(branchesViewController);
            AddChildViewController(tagsViewController);

            Add(branchesViewController.View);
            Add(tagsViewController.View);

            NavigationItem.TitleView = _viewSegment;

            OnActivation(disposable =>
            {
                _viewSegment
                    .GetChangedObservable()
                    .Subscribe(x => ViewModel.ShowBranches = x == 0)
                    .AddTo(disposable);

                this.WhenAnyValue(x => x.ViewModel.ShowBranches)
                    .Subscribe(x =>
                    {
                        View.EndEditing(true);
                        _viewSegment.SelectedSegment = x ? 0 : 1;
                        branchesViewController.View.Hidden = !x;
                        tagsViewController.View.Hidden = x;
                    })
                    .AddTo(disposable);
            });
        }
    }
}

