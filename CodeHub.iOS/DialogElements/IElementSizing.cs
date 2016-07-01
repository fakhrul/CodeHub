using UIKit;
using Foundation;
using System;

namespace CodeHub.DialogElements
{
    public interface IElementSizing 
    {
        nfloat GetHeight (UITableView tableView, NSIndexPath indexPath);
    }
}

