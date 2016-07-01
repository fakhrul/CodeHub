using System;
using CodeHub.DialogElements;
using CodeHub.ViewControllers;
using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using System.Linq;
using CoreGraphics;
using CodeHub.Core.Utilities;
using CodeHub.ViewControllers.Repositories;
using CodeHub.Transitions;
using GitHubSharp.Models;
using System.Collections.Generic;
using ReactiveUI;

namespace CodeHub.Views.Repositories
{
    public class RepositoriesTrendingViewController : ViewModelDrivenDialogViewController<RepositoriesTrendingViewModel>
    {
        private readonly TrendingTitleButton _trendingTitleButton = new TrendingTitleButton { Frame = new CGRect(0, 0, 200f, 32f) };

        public RepositoriesTrendingViewController() 
            : base(true, UITableViewStyle.Plain)
        {
            ViewModel = new RepositoriesTrendingViewModel();
            EnableSearch = false;
            NavigationItem.TitleView = _trendingTitleButton;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 64f;
            TableView.SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);

            ViewModel.WhenAnyValue(x => x.Repositories).Subscribe(repos => {
                var repositories = repos ?? Enumerable.Empty<Tuple<string, IList<RepositoryModel>>>();
                Root.Reset(repositories.Select(x => {
                    var s = new Section(CreateHeaderView(x.Item1));
                    s.Reset(x.Item2.Select(repo => {
                        var description = ViewModel.ShowRepositoryDescription ? repo.Description : string.Empty;
                        var avatar = new GitHubAvatar(repo.Owner?.AvatarUrl);
                        var username = repo.Owner?.Login;
                        var sse = new RepositoryElement(repo.Name, repo.StargazersCount, repo.Forks, description, username, avatar) { ShowOwner = true };
                        sse.Tapped += () =>
                        {
                            var view = new RepositoryViewController(username, repo.Name);
                            NavigationController.PushViewController(view, true);
                        };
                        return sse;
                    }));
                    return s;
                }));
            });

            OnActivation(d => {
                _trendingTitleButton.GetClickedObservable().Subscribe(_ => ShowLanguages()).AddTo(d);
                ViewModel.WhenAnyValue(x => x.SelectedLanguage).Subscribe(l => _trendingTitleButton.Text = l.Name).AddTo(d);
            });
        }

        private void ShowLanguages()
        {
            var vm = new WeakReference<RepositoriesTrendingViewModel>(ViewModel as RepositoriesTrendingViewModel);
            var view = new LanguagesViewController();
            view.SelectedLanguage = vm.Get()?.SelectedLanguage;
            view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Images.Buttons.CancelButton };
            view.NavigationItem.LeftBarButtonItem.GetClickedObservable().Subscribe(_ => DismissViewController(true, null));
            view.Language.Subscribe(x => {
                Root.Clear();
                var a = vm.Get();
                if (a != null)
                    a.SelectedLanguage = x;
                DismissViewController(true, null);
            });
            var ctrlToPresent = new ThemedNavigationController(view);
            ctrlToPresent.TransitioningDelegate = new SlideDownTransition();
            PresentViewController(ctrlToPresent, true, null);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
            {
                NavigationController.NavigationBar.ShadowImage = new UIImage();
                _trendingTitleButton.TintColor = NavigationController.NavigationBar.TintColor;
            }
        }

        private static UILabel CreateHeaderView(string name)
        {
            return new UILabel(new CGRect(0, 0, 320f, 26f)) 
            {
                BackgroundColor = Theme.CurrentTheme.PrimaryColor,
                Text = name,
                Font = UIFont.BoldSystemFontOfSize(14f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center
            };
        }
    }
}

