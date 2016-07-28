using CodeHub.Core.ViewModels.PullRequests;
using UIKit;
using System;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.ViewControllers.PullRequests
{
    public class PullRequestsViewController : BaseViewController<PullRequestsViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed" });

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var openViewController = new PullRequestListViewController { ViewModel = ViewModel.OpenPullRequests };
            var closedViewController = new PullRequestListViewController { ViewModel = ViewModel.ClosedPullRequests };

            AddChildViewController(openViewController);
            AddChildViewController(closedViewController);

            Add(openViewController.View);
            Add(closedViewController.View);

            NavigationItem.TitleView = _viewSegment;

            OnActivation(disposable =>
            {
                _viewSegment
                    .GetChangedObservable()
                    .Subscribe(x => ViewModel.SelectedFilter = x)
                    .AddTo(disposable);

                this.WhenAnyValue(x => x.ViewModel.SelectedFilter)
                    .Subscribe(x =>
                    {
                        View.EndEditing(true);
                        _viewSegment.SelectedSegment = x;
                        openViewController.View.Hidden = x != 0;
                        closedViewController.View.Hidden = x != 1;
                    })
                    .AddTo(disposable);
            });
        }
    } 
}

