using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Organizations;
using Foundation;
using ReactiveUI;
using UIKit;

namespace CodeHub.TableViewCells
{
    public class OrganizationTableViewCell : BaseTableViewCell<OrganizationItemViewModel>
    {
        public static NSString Key = new NSString(typeof(OrganizationTableViewCell).Name);

        public OrganizationTableViewCell(IntPtr handle)
            : base(handle)
        {
            Initialize();
        }

        public OrganizationTableViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            Initialize();
        }

        private void Initialize()
        {
            SeparatorInset = new UIEdgeInsets(0, 48f, 0, 0);
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            ImageView.Layer.CornerRadius = 16f;
            ImageView.Layer.MasksToBounds = true;

            this.WhenAnyValue(x => x.ViewModel)
                .Subscribe(x =>
                {
                TextLabel.Text = x?.Name;
                    ImageView.SetAvatar(x?.Avatar);
                });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ImageView.Frame = new CoreGraphics.CGRect(6, 6, 32, 32);
            TextLabel.Frame = new CoreGraphics.CGRect(48, TextLabel.Frame.Y, TextLabel.Frame.Width, TextLabel.Frame.Height);
            if (DetailTextLabel != null)
                DetailTextLabel.Frame = new CoreGraphics.CGRect(48, DetailTextLabel.Frame.Y, DetailTextLabel.Frame.Width, DetailTextLabel.Frame.Height);
        }
    }
}
