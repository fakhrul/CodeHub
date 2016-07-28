using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Source;
using Foundation;
using ReactiveUI;

namespace CodeHub.TableViewCells
{
    public class RefTableViewCell : BaseTableViewCell<RefItemViewModel>
    {
        public static NSString Key = new NSString(typeof(RefTableViewCell).Name);

        public RefTableViewCell(IntPtr handle)
            : base(handle)
        {
            this.WhenAnyValue(x => x.ViewModel)
                .Subscribe(x => TextLabel.Text = x?.Name);
        }
    }
}
