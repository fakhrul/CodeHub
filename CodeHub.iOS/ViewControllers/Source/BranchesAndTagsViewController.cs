using System;
using CodeHub.Core.ViewModels.Source;
using UIKit;
using CodeHub.DialogElements;
using CodeHub.Views;
using ReactiveUI;

namespace CodeHub.ViewControllers.Source
{
    public class BranchesAndTagsViewController : ViewModelCollectionDrivenDialogViewController<BranchesAndTagsViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});

        public BranchesAndTagsViewController()
        {
            NavigationItem.TitleView = _viewSegment;

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.GitBranch.ToEmptyListImage(), "There are no items."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var weakVm = new WeakReference<BranchesAndTagsViewModel>(ViewModel);

            BindCollection(ViewModel.Items, x => {
                var e = new StringElement(x.Name);
                e.Clicked.Subscribe(MakeCallback(weakVm, x));
                return e;
            });

            OnActivation(d =>
            {
                this.WhenAnyValue(x => x.ViewModel.SelectedFilter)
                    .Subscribe(x => _viewSegment.SelectedSegment = x)
                    .AddTo(d);
                
                _viewSegment
                    .GetChangedObservable()
                    .Subscribe(_ => ViewModel.SelectedFilter = (int)_viewSegment.SelectedSegment)
                    .AddTo(d);
            });
        }

        private static Action<object> MakeCallback(WeakReference<BranchesAndTagsViewModel> weakVm, BranchesAndTagsViewModel.ViewObject viewObject)
        {
            return new Action<object>(_ => weakVm.Get()?.GoToSourceCommand.Execute(viewObject));
        }
    }
}

