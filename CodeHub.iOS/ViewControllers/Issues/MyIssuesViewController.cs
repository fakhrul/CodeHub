using CodeHub.Core.ViewModels.Issues;
using UIKit;
using System;
using ReactiveUI;

namespace CodeHub.ViewControllers.Issues
{
    public class MyIssuesViewController : BaseIssuesViewController<MyIssuesViewModel>
    {
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] { "Open", "Closed", "Custom" });
        private UIBarButtonItem _segmentBarButton;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            var weakVm = new WeakReference<MyIssuesViewModel>(ViewModel);

            this.WhenAnyValue(x => x.ViewModel.SelectedFilter).Subscribe(x =>
            {
                var goodVm = weakVm.Get();

                if (x == 2 && goodVm != null)
                {
                    var filter = new CodeHub.Views.Filters.MyIssuesFilterViewController(goodVm.Issues);
                    var nav = new UINavigationController(filter);
                    PresentViewController(nav, true, null);
                }

                // If there is searching going on. Finish it.
                FinishSearch();
            });

            this.BindCollection(ViewModel.Issues, CreateElement);

            OnActivation(d =>
            {
                this.WhenAnyValue(x => x.ViewModel.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = (nint)x).AddTo(d);
                _viewSegment.GetChangedObservable().Subscribe(x => ViewModel.SelectedFilter = x).AddTo(d);
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

