using System;
using CodeHub.Core.ViewModels.Issues;
using System.Linq;
using UIKit;
using CodeHub.Utilities;
using CodeHub.DialogElements;
using CodeHub.Views;
using ReactiveUI;

namespace CodeHub.ViewControllers.Issues
{
    public class IssueLabelsViewController : ViewModelCollectionDrivenDialogViewController<IssueLabelsViewModel>
    {
        public IssueLabelsViewController()
        {
            EnableSearch = false;
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Tag.ToEmptyListImage(), "There are no labels."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Buttons.BackButton, UIBarButtonItemStyle.Plain, (s, e) => ViewModel.SaveLabelChoices.Execute(null));

            BindCollection(ViewModel.Labels, x => 
            {
                var e = new LabelElement(x.Name, x.Color);
                e.Clicked.Subscribe(_ =>
                {
                    if (e.Accessory == UITableViewCellAccessory.Checkmark)
                        ViewModel.SelectedLabels.Items.Remove(x);
                    else
                        ViewModel.SelectedLabels.Items.Add(x);
                });

                e.Accessory = ViewModel.SelectedLabels.Contains(x) ? 
                               UITableViewCellAccessory.Checkmark : 
                               UITableViewCellAccessory.None;
                return e;
            });

            ViewModel.BindCollection(x => x.SelectedLabels, true).Subscribe(_ =>
            {
                if (Root.Count == 0)
                    return;

                var elements = Root[0].Elements;
                foreach (var el in elements.Cast<LabelElement>())
                {
                    var element = el;
                    el.Accessory = ViewModel.SelectedLabels.Any(y => string.Equals(y.Name, element.Name, System.StringComparison.OrdinalIgnoreCase)) ? 
                                   UITableViewCellAccessory.Checkmark : 
                                   UITableViewCellAccessory.None;
                }
            });

            this.WhenAnyValue(x => x.ViewModel.IsSaving).SubscribeStatus("Saving...");
        }
    }
}

