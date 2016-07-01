using System;
using ReactiveUI;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Foundation;
using Splat;
using CodeHub.Core.Services;
using System.Reactive.Disposables;
using UIKit;
using CodeHub.Core.ViewModels;

namespace CodeHub.ViewControllers
{
    public abstract class BaseViewController<TViewModel> : BaseViewController, IViewFor<TViewModel>
        where TViewModel : class
    {
        private TViewModel _viewModel;
        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set { this.RaiseAndSetIfChanged(ref _viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as TViewModel; }
        }

        protected BaseViewController()
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
    }

    public abstract class BaseViewController : ReactiveViewController, IActivatable
    {
        private readonly ISubject<bool> _appearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _appearedSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearedSubject = new Subject<bool>();
        private CompositeDisposable _disposables = new CompositeDisposable();

        #if DEBUG
        ~BaseViewController()
        {
            Console.WriteLine("All done with " + GetType().Name);
        }
        #endif

        public IObservable<bool> Appearing
        {
            get { return _appearingSubject.AsObservable(); }
        }

        public IObservable<bool> Appeared
        {
            get { return _appearedSubject.AsObservable(); }
        }

        public IObservable<bool> Disappearing
        {
            get { return _disappearingSubject.AsObservable(); }
        }

        public IObservable<bool> Disappeared
        {
            get { return _disappearedSubject.AsObservable(); }
        }

        public void OnActivation(Action<CompositeDisposable> d)
        {
            Appearing.Subscribe(_ => d(_disposables));
        }

        protected BaseViewController()
        {
            CommonConstructor();
        }

        protected BaseViewController(string nib, NSBundle bundle)
            : base(nib, bundle)
        {
            CommonConstructor();
        }

        private void CommonConstructor()
        {
            this.WhenActivated(_ => { });
        }

        private void DisposeActivations()
        {
            _disposables?.Dispose();
        }

        public override void ViewWillAppear(bool animated)
        {
            Locator.Current.GetService<IAnalyticsService>()
                   ?.LogScreen(GetType().Name);

            base.ViewWillAppear(animated);
            DisposeActivations();
            _disposables = new CompositeDisposable();
            _appearingSubject.OnNext(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _appearedSubject.OnNext(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            DisposeActivations();
            _disappearingSubject.OnNext(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _disappearedSubject.OnNext(animated);
        }

        protected override void Dispose(bool disposing)
        {
            InvokeOnMainThread(() => View.DisposeAll());
            base.Dispose(disposing);
        }
    }
}

