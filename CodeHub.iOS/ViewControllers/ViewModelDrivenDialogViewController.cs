using System;
using UIKit;
using CodeHub.Core.ViewModels;
using CodeHub.Views;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Utilities;
using Splat;
using CodeHub.Core.Services;

namespace CodeHub.ViewControllers
{
    public abstract class PrettyDialogViewController<TViewModel> : ViewModelDrivenDialogViewController<TViewModel>
        where TViewModel : class
    {
        protected readonly SlideUpTitleView SlideUpTitle;
        protected readonly ImageAndTitleHeaderView HeaderView;
        private readonly UIView _backgroundHeaderView;

        public override string Title
        {
            get
            {
                return base.Title;
            }
            set
            {
                HeaderView.Text = value;
                SlideUpTitle.Text = value;
                base.Title = value;
                RefreshHeaderView();
            }
        }

        protected PrettyDialogViewController()
        {
            HeaderView = new ImageAndTitleHeaderView();
            SlideUpTitle = new SlideUpTitleView(44f) { Offset = 100f };
            NavigationItem.TitleView = SlideUpTitle;
            _backgroundHeaderView = new UIView();
        }

        public override UIRefreshControl RefreshControl
        {
            get { return base.RefreshControl; }
            set
            {
                if (value != null)
                    value.TintColor = UIColor.White;
                base.RefreshControl = value;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.ShadowImage = new UIImage();
            HeaderView.BackgroundColor = NavigationController.NavigationBar.BarTintColor;
            HeaderView.TextColor = NavigationController.NavigationBar.TintColor;
            HeaderView.SubTextColor = NavigationController.NavigationBar.TintColor.ColorWithAlpha(0.8f);
            _backgroundHeaderView.BackgroundColor = HeaderView.BackgroundColor;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (NavigationController != null)
                NavigationController.NavigationBar.ShadowImage = null;
        }

        protected void RefreshHeaderView()
        {
            TableView.TableHeaderView = HeaderView;
            TableView.ReloadData();
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            TableView.BeginUpdates();
            TableView.TableHeaderView = HeaderView;
            TableView.EndUpdates();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.TableHeaderView = HeaderView;
            TableView.SectionHeaderHeight = 0;

            var frame = TableView.Bounds;
            frame.Y = -frame.Size.Height;
            _backgroundHeaderView.Frame = frame;
            _backgroundHeaderView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _backgroundHeaderView.Layer.ZPosition = -1f;
            TableView.InsertSubview(_backgroundHeaderView, 0);
        }

        protected override void DidScroll(CoreGraphics.CGPoint p)
        {
            if (NavigationController == null)
                return;

            if (p.Y > 0)
                NavigationController.NavigationBar.ShadowImage = null;
            if (p.Y <= 0 && NavigationController.NavigationBar.ShadowImage == null)
                NavigationController.NavigationBar.ShadowImage = new UIImage();
            SlideUpTitle.Offset = 108 + 28 - p.Y;
        }
    }

    public abstract class ViewModelDrivenDialogViewController<TViewModel> : DialogViewController, IViewFor<TViewModel>
        where TViewModel : class
    {
        private readonly LoadingIndicator _indicator = new LoadingIndicator();
        private bool _manualRefreshRequested;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var loadableViewModel = ViewModel as LoadableViewModel;
            if (loadableViewModel != null)
            {
                RefreshControl = new UIRefreshControl();
                OnActivation(d =>
                {
                    loadableViewModel.LoadCommand.IsExecuting.Subscribe(x =>
                    {
                        if (x)
                        {
                            _indicator.Up();
                            RefreshControl.BeginRefreshing();

                            if (!_manualRefreshRequested)
                            {
                                UIView.Animate(0.25, 0f, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseOut,
                                    () => TableView.ContentOffset = new CoreGraphics.CGPoint(0, -RefreshControl.Frame.Height), null);
                            }

                            foreach (var t in (ToolbarItems ?? Enumerable.Empty<UIBarButtonItem>()))
                                t.Enabled = false;
                        }
                        else
                        {
                            _indicator.Down();

                            if (RefreshControl.Refreshing)
                            {
                                // Stupid bug...
                                BeginInvokeOnMainThread(() =>
                                {
                                    UIView.Animate(0.25, 0.0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseOut,
                                        () => TableView.ContentOffset = new CoreGraphics.CGPoint(0, 0), null);
                                    RefreshControl.EndRefreshing(); 
                                });
                            }

                            foreach (var t in (ToolbarItems ?? Enumerable.Empty<UIBarButtonItem>()))
                                t.Enabled = true;

                            _manualRefreshRequested = false;
                        }
                    }).AddTo(d);
                });
            }
        }

        protected ViewModelDrivenDialogViewController(bool push = true, UITableViewStyle style = UITableViewStyle.Grouped)
            : base(style, push)
        {
            Appearing
                .Take(1)
                .Select(_ => this.WhenAnyValue(x => x.ViewModel))
                .Switch()
                .OfType<ILoadableViewModel>()
                .Select(x => x.LoadCommand)
                .Subscribe(x => x.ExecuteIfCan());

            OnActivation(disposable =>
            {
                this.WhenAnyValue(x => x.ViewModel)
                    .OfType<IProvidesTitle>()
                    .Select(x => x.WhenAnyValue(y => y.Title))
                    .Switch()
                    .Subscribe(x => Title = x)
                    .AddTo(disposable);

                this.WhenAnyValue(x => x.ViewModel)
                    .OfType<IRoutingViewModel>()
                    .Select(x => x.RequestNavigation)
                    .Switch()
                    .Subscribe(x =>
                    {
                        var viewModelViewService = Locator.Current.GetService<IViewModelViewService>();
                        var viewType = viewModelViewService.GetViewFor(x.GetType());
                        var view = (IViewFor)Activator.CreateInstance(viewType);
                        view.ViewModel = x;
                        HandleNavigation(x, view as UIViewController);
                    }).AddTo(disposable);
            });
        }

        protected virtual void HandleNavigation(IBaseViewModel viewModel, UIViewController view)
        {
            if (view is IModalViewController)
            {
                PresentViewController(new ThemedNavigationController(view), true, null);
                viewModel.RequestDismiss.Subscribe(_ => DismissViewController(true, null));
            }
            else
            {
                NavigationController.PushViewController(view, true);
                viewModel.RequestDismiss.Subscribe(_ => NavigationController.PopToViewController(this, true));
            }
        }

        private void HandleRefreshRequested(object sender, EventArgs e)
        {
            var loadableViewModel = ViewModel as LoadableViewModel;
            if (loadableViewModel != null)
            {
                _manualRefreshRequested = true;
                loadableViewModel.LoadCommand.Execute(true);
            }
        }

        bool _isLoaded = false;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (!_isLoaded)
            {
                var loadableViewModel = ViewModel as LoadableViewModel;
                if (loadableViewModel != null)
                    loadableViewModel.LoadCommand.Execute(false);
                _isLoaded = true;
            }

            if (RefreshControl != null)
                RefreshControl.ValueChanged += HandleRefreshRequested;
        }

        private TViewModel _viewModel;
        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set { this.RaiseAndSetIfChanged(ref _viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            if (RefreshControl != null)
                RefreshControl.ValueChanged -= HandleRefreshRequested;
        }
    }
}

