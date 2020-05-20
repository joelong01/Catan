using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void GridPositionChangedHandler(string name, GridPosition gridPosition);

    [ContentProperty(Name = "Child")]
    public sealed partial class DragableGridCtrl : UserControl
    {
        public event GridPositionChangedHandler OnGridPositionChanged;
        public DragableGridCtrl()
        {
            this.InitializeComponent();

        }
        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(DragableGridCtrl), new PropertyMetadata(new PlayerModel()));
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(DragableGridCtrl), new PropertyMetadata("This is the Caption"));
        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(nameof(Child), typeof(UIElement), typeof(DragableGridCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty GridPositionProperty = DependencyProperty.Register("GridPosition", typeof(GridPosition), typeof(DragableGridCtrl), new PropertyMetadata(new GridPosition(0, 0, 1.0, 1.0), GridPositionChanged));
        public GridPosition GridPosition
        {
            get => (GridPosition)GetValue(GridPositionProperty);
            set => SetValue(GridPositionProperty, value);
        }
        private static void GridPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as DragableGridCtrl;
            depPropClass?.SetGridPosition((GridPosition)e.OldValue, (GridPosition)e.NewValue);
        }
        private void SetGridPosition(GridPosition oldPosition, GridPosition newPosition)
        {
            oldPosition.PropertyChanged -= GridPosition_Changed;
            newPosition.PropertyChanged += GridPosition_Changed;
            AnimateMove.Begin();
        }

        private void GridPosition_Changed(object sender, PropertyChangedEventArgs e)
        {
            AnimateMove.Begin();
        }

        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {

            UIElement uiElement = sender as UIElement;
            int zIndex = Canvas.GetZIndex(uiElement);
            if (e.GetCurrentPoint(uiElement).Position.Y < 30)
            {
                Canvas.SetZIndex(uiElement, zIndex + 1000);

                if (sender.GetType() == typeof(Grid))
                {
                    Grid grid = sender as Grid;
                    Point point = await StaticHelpers.DragAsync(grid, e);
                    NotifyPositionChanged();
                }

                Canvas.SetZIndex(uiElement, zIndex);
            }

        }

        private void NotifyPositionChanged()
        {
            //
            //  the drag operation happens in "Visual Space", not "Animation Space" - so we need to update our animation to say where the grid really is

            GridPosition.TranslateX = ((CompositeTransform)LayoutRoot.RenderTransform).TranslateX;
            GridPosition.TranslateY = ((CompositeTransform)LayoutRoot.RenderTransform).TranslateY;

            OnGridPositionChanged?.Invoke(userControl.Name, GridPosition);
           
        }

        private void OnGrowOrShrinkControls(object sender, RoutedEventArgs e)
        {
            
            if (GridPosition.ScaleX != 1.0)
            {
                GridPosition.ScaleX = 1.0;
                GridPosition.ScaleY = 1.0;
            }
            else
            {
                GridPosition.ScaleX = 0.5;
                GridPosition.ScaleY = 0.5;
            }
            AnimateMove.Begin();
            
            NotifyPositionChanged();

        }

        
        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

            Border border = sender as Border;

            Point position = e.GetCurrentPoint(border).Position;
            double grabSize = 5;
            bool mouseCaptured = false;
            PointerEventHandler pointerMoved = null;
            PointerEventHandler pointerExited = null;
            PointerEventHandler pointerPressed = null;
            PointerEventHandler pointerReleased = null;
            bool sizeableX = false;
            bool sizeableY = false;
            double originalWidth = border.Width;
            double originalHeight = border.Height;
            Point pointMouseDown = position;
            CompositeTransform ct = border.RenderTransform as CompositeTransform;


            pointerPressed = (object s, PointerRoutedEventArgs eMove) =>
            {
                this.TraceMessage("PointerPressed");
                border.PointerReleased += pointerReleased;
                if (sizeableX == true)
                {
                    mouseCaptured = true;
                    border.CapturePointer(e.Pointer);
                    this.TraceMessage("resize X");
                }
                if (sizeableY == true)
                {
                    mouseCaptured = true;
                    border.CapturePointer(e.Pointer);
                    this.TraceMessage("resize Y");
                }
            };

            pointerReleased = (object s, PointerRoutedEventArgs eMove) =>
            {
                this.TraceMessage($"PointerReleased");
                border.PointerPressed -= pointerPressed;
                border.PointerReleased -= pointerReleased;
                border.PointerMoved -= pointerMoved;
                if (mouseCaptured)
                {
                    border.ReleasePointerCapture(e.Pointer);
                }
            };

            pointerMoved = (object s, PointerRoutedEventArgs eMove) =>
            {

                position = eMove.GetCurrentPoint(border).Position;
                if (mouseCaptured)
                {
                    double ratioX = position.X / originalWidth;
                    double ratioY = position.Y / originalHeight;
                    this.TraceMessage($"pointerMoved: {position} RatioX:{ratioX} RatioY:{ratioY}");

                    //
                    //  find how much the mouse has moved and resize the window as appropriate..I think this should be a trasnform, not a width

                    //
                    //  this is the money clause -- resize the window!
                    if (sizeableX)
                    {
                        if (ratioX > .5)
                        {
                            ct.ScaleX = ratioX;
                        }
                    }
                    if (sizeableY)
                    {
                        if (ratioY > .5)
                        {
                            ct.ScaleY = ratioY;
                        }
                    }

                    return;
                }

                if (position.Y < grabSize || position.Y > border.Height - grabSize)
                {
                    sizeableY = true;
                    sizeableX = false;
                    Window.Current.CoreWindow.PointerCursor =
                        new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeNorthSouth, 1);
                }
                else if (position.X < grabSize || position.X > border.Width - grabSize)
                {
                    sizeableX = true;
                    sizeableY = false;
                    Window.Current.CoreWindow.PointerCursor =
                        new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeWestEast, 1);
                }

                else
                {
                    sizeableX = false;
                    sizeableY = false;
                    Window.Current.CoreWindow.PointerCursor =
                                  new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
                }


            };

            pointerExited = (s, eExit) =>
            {
                this.TraceMessage($"pointerMoved");
                Window.Current.CoreWindow.PointerCursor =
                    new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
                border.PointerMoved -= pointerMoved;
                border.PointerExited -= pointerExited;
                border.PointerPressed -= pointerPressed;
            };

            border.PointerMoved += pointerMoved;
            border.PointerExited += pointerExited;
            border.PointerPressed += pointerPressed;

        }

    }
}
