using System;
using Foundation;
using UIKit;
using CodeHub.TableViewCells;
using CodeHub.Core.Utilities;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.DialogElements
{
    public class RepositoryElement : Element
    {       
        private readonly string _name;
        private readonly int _followers;
        private readonly int _forks;
        private readonly string _description;
        private readonly string _owner;
        private readonly GitHubAvatar _avatar;

        public UIColor BackgroundColor { get; set; }

        public bool ShowOwner { get; set; }

        public RepositoryElement(string name, int followers, int forks, string description, string owner, GitHubAvatar avatar)
        {
            _name = name;
            _followers = followers;
            _forks = forks;
            _description = description;
            _owner = owner;
            _avatar = avatar;
            ShowOwner = true;
        }

        public event Action Tapped;
        
        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell(RepositoryCellView.Key) as RepositoryCellView ?? RepositoryCellView.Create();
            cell.ViewModel = new RepositoryItemViewModel(_name, _description, ShowOwner ? _owner : null, _followers, _forks, _avatar);
            return cell;
        }
        
        public override bool Matches(string text)
        {
            var name = _name ?? string.Empty;
            return name.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1;
        }
        
        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            Tapped?.Invoke();
        }
    }
}

