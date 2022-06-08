using PnFDesktop.Classes;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// Implements an ListBox for displaying PointAndFigureBoxes in the NetworkView UI.
    /// </summary>
    public class PointAndFigureColumnItemsControl : ListBox
    {

        public PointAndFigureColumnItemsControl()
        {
            Focusable = false;
        }


        #region Private Methods

        /// <summary>
        /// Find the PointAndFigureColumnItem UI element that has the specified data context.
        /// Return null if no such PointAndFigureColumnItem exists.
        /// </summary>
        internal PointAndFigureColumnItem FindAssociatedColumnItem(object pointAndFigureColumnDataContext)
        {
            return (PointAndFigureColumnItem)this.ItemContainerGenerator.ContainerFromItem(pointAndFigureColumnDataContext);
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item. 
        /// </summary>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new PointAndFigureColumnItem();
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container. 
        /// </summary>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is PointAndFigureColumnItem;
        }

        #endregion Private Methods
    }
}
