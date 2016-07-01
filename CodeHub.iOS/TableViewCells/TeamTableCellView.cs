using System;
using ReactiveUI;
using Foundation;
using CodeHub.Core.ViewModels.Organizations;
using System.Reactive.Linq;

namespace CodeHub.TableViewCells
{
    public class TeamTableCellView : BaseTableViewCell<TeamItemViewModel>
    {
        public static NSString Key = new NSString("TeamTableCell");

        public TeamTableCellView(IntPtr handle)
            : base(handle)
        {
            this.WhenAnyValue(x => x.ViewModel)
                .Subscribe(x => TextLabel.Text = x?.Name);
        }
    }
}
