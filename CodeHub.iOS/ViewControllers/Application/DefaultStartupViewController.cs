using CodeHub.Core.ViewModels.App;
using System;
using UIKit;
using System.Linq;
using CodeHub.DialogElements;
using ReactiveUI;

namespace CodeHub.ViewControllers.Application
{
    public class DefaultStartupViewController : ViewModelCollectionDrivenDialogViewController<DefaultStartupViewModel>
    {
        public DefaultStartupViewController()
        {
            ViewModel = new DefaultStartupViewModel();
            EnableSearch = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BindCollection(ViewModel.StartupViews, x => {
                var e = new StringElement(x);
                e.Clicked.Subscribe(_ => ViewModel.SelectedStartupView = x);
                if (string.Equals(ViewModel.SelectedStartupView, x))
                    e.Accessory = UITableViewCellAccessory.Checkmark;
                return e;
            }, true);

            this.WhenAnyValue(x => x.ViewModel.SelectedStartupView).Subscribe(x =>
            {
                if (Root.Count == 0)
                    return;
                foreach (var m in Root[0].Elements.Cast<StringElement>())
                    m.Accessory = (string.Equals(m.Caption, x)) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            });
        }
    }
}

