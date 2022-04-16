using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections;
using PnFDesktop.Controls;
using PnFDesktop.ViewModels;

namespace PnFDesktop.Controls
{
    /// <summary>
    /// Defines the current state of the mouse handling logic.
    /// </summary>
    public enum MouseHandlingMode
    {
        /// <summary>
        /// Not in any special mode.
        /// </summary>
        None,

        /// <summary>
        /// Panning has been initiated and will commence when the use drags the cursor further than the threshold distance.
        /// </summary>
        Panning,

        /// <summary>
        /// The user is left-mouse-button-dragging to pan the viewport.
        /// </summary>
        DragPanning,

        /// <summary>
        /// The user is holding down shift and left-clicking or right-clicking to zoom in or out.
        /// </summary>
        Zooming,

        /// <summary>
        /// The user is holding down shift and left-mouse-button-dragging to select a region to zoom to.
        /// </summary>
        DragZooming,
    }

    public partial class PointAndFigureChartView
    {
        public class ZoomSetting
        {
            internal Rect PrevZoomRect { get; set; }
            internal double PrevZoomScale { get; set; }
        }

        private Stack<ZoomSetting> zoomStack = new Stack<ZoomSetting>(50);

        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;

        /// <summary>
        /// The point that was clicked relative to the ZoomAndPanControl.
        /// </summary>
        private Point origZoomAndPanControlMouseDownPoint;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point origContentMouseDownPoint;

        /// <summary>
        /// Records which mouse button clicked during mouse dragging.
        /// </summary>
        private MouseButton mouseButtonDown;

        /// <summary>
        /// Event raised on mouse down in the NetworkView.
        /// </summary> 
        private void PointAndFigureChart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PointAndFigureChartControl.Focus();
            Keyboard.Focus(PointAndFigureChartControl);

            mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(ZoomAndPanControl);
            origContentMouseDownPoint = e.GetPosition(PointAndFigureChartControl);

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                (e.ChangedButton == MouseButton.Left ||
                 e.ChangedButton == MouseButton.Right))
            {
                // Shift + left- or right-down initiates zooming mode.
                mouseHandlingMode = MouseHandlingMode.Zooming;
            }
            //else if (mouseButtonDown == MouseButton.Left &&
            //         (Keyboard.Modifiers & ModifierKeys.Control) == 0) {
            else if (mouseButtonDown == MouseButton.Middle)
            {
                //
                // Initiate panning, when control is not held down.
                // When control is held down left dragging is used for drag selection.
                // After panning has been initiated the user must drag further than the threshold value to actually start drag panning.
                //
                mouseHandlingMode = MouseHandlingMode.Panning;
            }

            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                // Capture the mouse so that we eventually receive the mouse up event.
                PointAndFigureChartControl.CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse up in the NetworkView.
        /// </summary>
        private void PointAndFigureChart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                if (mouseHandlingMode == MouseHandlingMode.Panning)
                {
                    //
                    // Panning was initiated but dragging was abandoned before the mouse
                    // cursor was dragged further than the threshold distance.
                    // This means that this basically just a regular left mouse click.
                    // Because it was a mouse click in empty space we need to clear the current selection.
                    //
                }
                else if (mouseHandlingMode == MouseHandlingMode.Zooming)
                {
                    if (mouseButtonDown == MouseButton.Left)
                    {
                        // Shift + left-click zooms in on the content.
                        ZoomIn(origContentMouseDownPoint);
                    }
                    else if (mouseButtonDown == MouseButton.Right)
                    {
                        // Shift + left-click zooms out from the content.
                        ZoomOut(origContentMouseDownPoint);
                    }
                }
                else if (mouseHandlingMode == MouseHandlingMode.DragZooming)
                {
                    // When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.
                    ApplyDragZoomRect();
                }

                //
                // Reenable clearing of selection when empty space is clicked.
                // This is disabled when drag panning is in progress.
                //
                PointAndFigureChartControl.IsClearSelectionOnEmptySpaceClickEnabled = true;

                //
                // Reset the override cursor.
                // This is set to a special cursor while drag panning is in progress.
                //
                Mouse.OverrideCursor = null;

                PointAndFigureChartControl.ReleaseMouseCapture();
                mouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse move in the NetworkView.
        /// </summary>
        private void PointAndFigureChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseHandlingMode == MouseHandlingMode.Panning)
            {
                Point curZoomAndPanControlMousePoint = e.GetPosition(ZoomAndPanControl);
                Vector dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
                double dragThreshold = 10;
                if (Math.Abs(dragOffset.X) > dragThreshold ||
                    Math.Abs(dragOffset.Y) > dragThreshold)
                {
                    //
                    // The user has dragged the cursor further than the threshold distance, initiate
                    // drag panning.
                    //
                    mouseHandlingMode = MouseHandlingMode.DragPanning;
                    PointAndFigureChartControl.IsClearSelectionOnEmptySpaceClickEnabled = false;
                    Mouse.OverrideCursor = Cursors.ScrollAll;
                }

                e.Handled = true;
            }
            else if (mouseHandlingMode == MouseHandlingMode.DragPanning)
            {
                //
                // The user is left-dragging the mouse.
                // Pan the viewport by the appropriate amount.
                //
                Point curContentMousePoint = e.GetPosition(PointAndFigureChartControl);
                Vector dragOffset = curContentMousePoint - origContentMouseDownPoint;

                ZoomAndPanControl.ContentOffsetX -= dragOffset.X;
                ZoomAndPanControl.ContentOffsetY -= dragOffset.Y;

                e.Handled = true;
            }
            else if (mouseHandlingMode == MouseHandlingMode.Zooming)
            {
                Point curZoomAndPanControlMousePoint = e.GetPosition(ZoomAndPanControl);
                Vector dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
                double dragThreshold = 10;
                if (mouseButtonDown == MouseButton.Left &&
                    (Math.Abs(dragOffset.X) > dragThreshold ||
                    Math.Abs(dragOffset.Y) > dragThreshold))
                {
                    //
                    // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                    // initiate drag zooming mode where the user can drag out a rectangle to select the area
                    // to zoom in on.
                    //
                    mouseHandlingMode = MouseHandlingMode.DragZooming;
                    Point curContentMousePoint = e.GetPosition(PointAndFigureChartControl);
                    InitDragZoomRect(origContentMouseDownPoint, curContentMousePoint);
                }

                e.Handled = true;
            }
            else if (mouseHandlingMode == MouseHandlingMode.DragZooming)
            {
                //
                // When in drag zooming mode continously update the position of the rectangle
                // that the user is dragging out.
                //
                Point curContentMousePoint = e.GetPosition(PointAndFigureChartControl);
                SetDragZoomRect(origContentMouseDownPoint, curContentMousePoint);

                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised by rotating the mouse wheel.
        /// </summary>
        private void PointAndFigureChart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                Point curContentMousePoint = e.GetPosition(PointAndFigureChartControl);
                ZoomIn(curContentMousePoint);
            }
            else if (e.Delta < 0)
            {
                Point curContentMousePoint = e.GetPosition(PointAndFigureChartControl);
                ZoomOut(curContentMousePoint);
            }
        }

        /// <summary>
        /// Event raised when the user has double clicked in the zoom and pan control.
        /// </summary>
        private void PointAndFigureChart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                Point doubleClickPoint = e.GetPosition(PointAndFigureChartControl);
                ZoomAndPanControl.AnimatedSnapTo(doubleClickPoint);
            }
        }

        /// <summary>
        /// The 'ZoomIn' command (bound to the plus key) was executed.
        /// </summary>
        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var o = PointAndFigureChartControl.SelectedColumn;

            ZoomIn(new Point(ZoomAndPanControl.ContentZoomFocusX, ZoomAndPanControl.ContentZoomFocusY));
        }

        /// <summary>
        /// The 'ZoomOut' command (bound to the minus key) was executed.
        /// </summary>
        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOut(new Point(ZoomAndPanControl.ContentZoomFocusX, ZoomAndPanControl.ContentZoomFocusY));
        }

        /// <summary>
        /// The 'JumpBackToPrevZoom' command was executed.
        /// </summary>
        private void JumpBackToPrevZoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            JumpBackToPrevZoom();
        }

        /// <summary>
        /// Determines whether the 'JumpBackToPrevZoom' command can be executed.
        /// </summary>
        private void JumpBackToPrevZoom_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (zoomStack.Count > 0);
        }

        /// <summary>
        /// The 'Fill' command was executed.
        /// </summary>
        private void FitContent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IList columns = null;
            IList annotations = null;

                columns = _viewModel.Columns;
                //TODO:  Include annotations
                // annotations = _viewModel.Annotations;

                if (columns.Count == 0) // && annotations.Count == 0)
                {
                    return;
                }

            SavePrevZoomRect();

            Rect actualContentRect = DetermineAreaOfColumns(columns, annotations);

            //
            // Inflate the content rect by a fraction of the actual size of the total content area.
            // This puts a nice border around the content we are fitting to the viewport.
            //
            actualContentRect.Inflate(PointAndFigureChartControl.ActualWidth / 40, PointAndFigureChartControl.ActualHeight / 40);

            ZoomAndPanControl.AnimatedZoomTo(actualContentRect);
        }

        /// <summary>
        /// Determine the area covered by the specified list of columns.
        /// </summary>
        private Rect DetermineAreaOfColumns(IList columns, IList annotations)
        {
            //if (columns.Count > 0)
            //{
                PointAndFigureColumnViewModel firstColumn = (PointAndFigureColumnViewModel)columns[0];
                Rect actualContentRect = new Rect(firstColumn.X, firstColumn.Y, firstColumn.Size.Width, firstColumn.Size.Height);

                for (int i = 1; i < columns.Count; ++i)
                {
                    PointAndFigureColumnViewModel column = (PointAndFigureColumnViewModel)columns[i];
                    Rect columnRect = new Rect(column.X, column.Y, column.Size.Width, column.Size.Height);
                    actualContentRect = Rect.Union(actualContentRect, columnRect);
                }
                //TODO:  Check annotations
                //if (annotations != null)
                //{
                //    for (int i = 0; i < annotations.Count; ++i)
                //    {
                //        AnnotationDesignerViewModel annotation = (AnnotationDesignerViewModel)annotations[i];
                //        double scale = annotation.Scale;
                //        Rect columnRect = new Rect(annotation.X, annotation.Y, annotation.Size.Width * scale, annotation.Size.Height * scale);
                //        actualContentRect = Rect.Union(actualContentRect, columnRect);
                //    }
                //}

                return actualContentRect;
            //}
            //else
            //{
            //    // Only got annotations in the drawing
            //    AnnotationDesignerViewModel firstAnnotation = (AnnotationDesignerViewModel)annotations[0];
            //    double scale = firstAnnotation.Scale;
            //    Rect actualContentRect = new Rect(firstAnnotation.X, firstAnnotation.Y, firstAnnotation.Size.Width * scale, firstAnnotation.Size.Height * scale);
            //    for (int i = 1; i < annotations.Count; ++i)
            //    {
            //        AnnotationDesignerViewModel annotation = (AnnotationDesignerViewModel)annotations[i];
            //        scale = annotation.Scale;
            //        Rect columnRect = new Rect(annotation.X, annotation.Y, annotation.Size.Width * scale, annotation.Size.Height * scale);
            //        actualContentRect = Rect.Union(actualContentRect, columnRect);
            //    }
            //    return actualContentRect;
            //}
        }

        /// <summary>
        /// The 'Fill' command was executed.
        /// </summary>
        private void Fill_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SavePrevZoomRect();

            ZoomAndPanControl.AnimatedScaleToFit();
        }

        /// <summary>
        /// The 'Refresh' command was executed.
        /// </summary>
        private void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // this.PointAndFigureChart.GetBindingExpression(NetworkView.ConnectionsSourceProperty).UpdateTarget();
            this.PointAndFigureChartControl.GetBindingExpression(Controls.PointAndFigureChartControl.ColumnsSourceProperty).UpdateTarget();
        }

        /// <summary>
        /// The 'OneHundredPercent' command was executed.
        /// </summary>
        private void OneHundredPercent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SavePrevZoomRect();

            ZoomAndPanControl.AnimatedZoomTo(1.0);
        }

        /// <summary>
        /// Jump back to the previous zoom level.
        /// </summary>
        private void JumpBackToPrevZoom()
        {
            if (zoomStack.Count > 0)
            {
                ZoomSetting prevZoom = zoomStack.Pop();
                ZoomAndPanControl.AnimatedZoomTo(prevZoom.PrevZoomScale, prevZoom.PrevZoomRect);
            }
        }

        /// <summary>
        /// Zoom the viewport out, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomOut(Point contentZoomCenter)
        {
            SavePrevZoomRect();
            ZoomAndPanControl.ZoomAboutPoint(ZoomAndPanControl.ContentScale - 0.025, contentZoomCenter);
        }

        /// <summary>
        /// Zoom the viewport in, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomIn(Point contentZoomCenter)
        {
            SavePrevZoomRect();
            ZoomAndPanControl.ZoomAboutPoint(ZoomAndPanControl.ContentScale + 0.025, contentZoomCenter);
        }

        /// <summary>
        /// Initialize the rectangle that the use is dragging out.
        /// </summary>
        private void InitDragZoomRect(Point pt1, Point pt2)
        {
            SetDragZoomRect(pt1, pt2);

            dragZoomCanvas.Visibility = Visibility.Visible;
            dragZoomBorder.Opacity = 0.5;
        }

        /// <summary>
        /// Update the position and size of the rectangle that user is dragging out.
        /// </summary>
        private void SetDragZoomRect(Point pt1, Point pt2)
        {
            double x, y, width, height;

            //
            // Deterine x,y,width and height of the rect inverting the points if necessary.
            // 

            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            //
            // Update the coordinates of the rectangle that is being dragged out by the user.
            // The we offset and rescale to convert from content coordinates.
            //
            Canvas.SetLeft(dragZoomBorder, x);
            Canvas.SetTop(dragZoomBorder, y);
            dragZoomBorder.Width = width;
            dragZoomBorder.Height = height;
        }

        /// <summary>
        /// When the user has finished dragging out the rectangle the zoom operation is applied.
        /// </summary>
        private void ApplyDragZoomRect()
        {
            //
            // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
            //
            SavePrevZoomRect();

            //
            // Retreive the rectangle that the user draggged out and zoom in on it.
            //
            double contentX = Canvas.GetLeft(dragZoomBorder);
            double contentY = Canvas.GetTop(dragZoomBorder);
            double contentWidth = dragZoomBorder.Width;
            double contentHeight = dragZoomBorder.Height;
            ZoomAndPanControl.AnimatedZoomTo(new Rect(contentX, contentY, contentWidth, contentHeight));

            FadeOutDragZoomRect();
        }

        //
        // Fade out the drag zoom rectangle.
        //
        private void FadeOutDragZoomRect()
        {
            AnimationHelper.StartAnimation(dragZoomBorder, Border.OpacityProperty, 0.0, 0.1,
                delegate (object sender, EventArgs e)
                {
                    dragZoomCanvas.Visibility = Visibility.Collapsed;
                });
        }

        //
        // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
        //
        private void SavePrevZoomRect()
        {
            // Note: This code will keep growing and growing the stack. If performance becomes an issue
            // will need to implement custom limited size stack.
            //prevZoomRect = new Rect(ZoomAndPanControl.ContentOffsetX, ZoomAndPanControl.ContentOffsetY, ZoomAndPanControl.ContentViewportWidth, ZoomAndPanControl.ContentViewportHeight);
            //prevZoomScale = ZoomAndPanControl.ContentScale;
            //prevZoomRectSet = true;
            zoomStack.Push(new ZoomSetting()
            {
                PrevZoomRect = new Rect(ZoomAndPanControl.ContentOffsetX, ZoomAndPanControl.ContentOffsetY, ZoomAndPanControl.ContentViewportWidth, ZoomAndPanControl.ContentViewportHeight),
                PrevZoomScale = ZoomAndPanControl.ContentScale
            });
        }

    }
}
