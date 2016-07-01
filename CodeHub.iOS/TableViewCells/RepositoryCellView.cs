using System;
using Foundation;
using UIKit;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive.Linq;
using ObjCRuntime;

namespace CodeHub.TableViewCells
{
    public partial class RepositoryCellView : BaseTableViewCell<RepositoryItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("RepositoryCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("RepositoryCellView");

        public static RepositoryCellView Create()
        {
            var cell = new RepositoryCellView();
            var views = NSBundle.MainBundle.LoadNib("RepositoryCellView", cell, null);
            return Runtime.GetNSObject(views.ValueAt(0)) as RepositoryCellView;
        }

        public RepositoryCellView()
        {
        }

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Caption.TextColor = Theme.CurrentTheme.MainTitleColor;
            Description.TextColor = Theme.CurrentTheme.MainTextColor;

            Image1.Image = Octicon.Star.ToImage(12);
            Image3.Image = Octicon.RepoForked.ToImage(12);
            UserImage.Image = Octicon.Person.ToImage(12);

            SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);
            Caption.TextColor = Theme.CurrentTheme.MainTitleColor;
            Description.TextColor = Theme.CurrentTheme.MainTextColor;
            BigImage.Layer.MasksToBounds = true;
            BigImage.Layer.CornerRadius = BigImage.Bounds.Height / 2f;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    Label1.Text = x.Stars.ToString();
                    Label3.Text = x.Forks.ToString();
                    RepoName.Hidden = x.Owner == null;
                    UserImage.Hidden = RepoName.Hidden;
                    Caption.Text = x.Name;
                    RepoName.Text = x.Owner ?? string.Empty;
                    Description.Hidden = string.IsNullOrWhiteSpace(x.Description);
                    Description.Text = x.Description ?? string.Empty;
                    BigImage.SetAvatar(x.Avatar);
                });
        }
    }
}

