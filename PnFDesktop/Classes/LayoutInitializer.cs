using PnFDesktop.ViewModels;
using System.Linq;
using Xceed.Wpf.AvalonDock.Layout;

namespace PnFDesktop.Classes
{
    class LayoutInitializer : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            //AD wants to add the anchorable into destinationContainer
            //just for test provide a new anchorablepane 
            //if the pane is floating let the manager go ahead
            LayoutAnchorablePane destPane = destinationContainer as LayoutAnchorablePane;
            if (destPane != null &&
                destPane.FindParent<LayoutFloatingWindow>() != null)
                return false;


            PaneViewModel paneViewModel = anchorableToShow.Content as PaneViewModel;
            bool successfullyPlaced = false;
            if (paneViewModel != null)
            {
                switch (paneViewModel.InitialPaneLocation)
                {
                    case InitialPaneLocation.Bottom:
                        var bottomPane = layout.BottomSide.Descendents().OfType<LayoutAnchorGroup>().FirstOrDefault();
                        if (bottomPane != null)
                        {
                            bottomPane.Children.Add(anchorableToShow);
                            successfullyPlaced = true;
                        }
                        break;
                    case InitialPaneLocation.Right:
                        var rightPane = layout.RightSide.Descendents().OfType<LayoutAnchorGroup>().FirstOrDefault();
                        if (rightPane != null)
                        {
                            rightPane.Children.Add(anchorableToShow);
                            successfullyPlaced = true;
                        }
                        break;
                    default:
                        // Default to LeftPane
                        var leftPane = layout.LeftSide.Descendents().OfType<LayoutAnchorGroup>().FirstOrDefault();
                        if (leftPane != null)
                        {
                            leftPane.Children.Add(anchorableToShow);
                            successfullyPlaced = true;
                        }
                        break;
                }
            }

            return successfullyPlaced;

        }


        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }


        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {

        }

    }
}
