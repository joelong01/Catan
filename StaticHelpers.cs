using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Catan10
{
    public static class BindingExtensions

    {
        #region Methods

        public static void UpdateSource(this FrameworkElement element, DependencyProperty property)
        {
            BindingExpression expression = element.GetBindingExpression(property);
            if (expression != null)
            {
                expression.UpdateSource();
            }
        }

        #endregion Methods
    }

    public static class EnumExtensions
    {
        #region Methods

        public static string Description(this Enum instance)
        {
            string output = "";
            Type type = instance.GetType();
            FieldInfo fi = type.GetField(instance.ToString());
            DescriptionAttribute[] attrs = fi.GetCustomAttributes(attributeType: typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Description;
            }
            return output;
        }

        #endregion Methods
    }

    public static class ICollectionExtensions
    {
        #region Methods

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> that)
        {
            if (that == null) return;
            foreach (T t in that)
            {
                collection.Add(t);
            }
        }

        public static T First<T>(this ICollection<T> collection)
        {
            return collection.ElementAt(0);
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }

        public static void Swap<T>(this IList<T> list, int firstIndex, int secondIndex)
        {
            Contract.Requires(list != null);
            Contract.Requires(firstIndex >= 0 && firstIndex < list.Count);
            Contract.Requires(secondIndex >= 0 && secondIndex < list.Count);
            if (firstIndex == secondIndex)
            {
                return;
            }
            T temp = list[firstIndex];
            list[firstIndex] = list[secondIndex];
            list[secondIndex] = temp;
        }

        #endregion Methods
    }

    public static class StaticHelpers
    {
        #region Properties

        public static bool IsInVisualStudioDesignMode => !(Application.Current is App);

        #endregion Properties

        #region Methods

        public static void AddRange<T>(this ObservableCollection<T> oc, IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            foreach (T item in collection)
            {
                oc.Add(item);
            }
        }

        static public async Task<bool> AskUserYesNoQuestion(string question, string button1, string button2)
        {
            bool saidYes = false;

            ContentDialog dlg = new ContentDialog()
            {
                Title = "Catan",
                Content = "\n" + question,
                PrimaryButtonText = button1,
                SecondaryButtonText = button2
            };

            dlg.PrimaryButtonClick += (o, i) =>
           {
               saidYes = true;
           };

            await dlg.ShowAsync();

            return saidYes;
        }

        public static List<T> DestructiveIterator<T>(this List<T> list)
        {
            List<T> copy = new List<T>(list);
            return copy;
        }

        public static Task<Point> DragAsync(UIElement control, PointerRoutedEventArgs origE, IDragAndDropProgress progress = null)
        {
            TaskCompletionSource<Point> taskCompletionSource = new TaskCompletionSource<Point>();
            UIElement mousePositionWindow = Window.Current.Content;
            GeneralTransform gt = Window.Current.Content.TransformToVisual(control);
            UIElement root = Window.Current.Content;

            Point pointMouseDown = gt.TransformPoint(origE.GetCurrentPoint(mousePositionWindow).Position);

            PointerEventHandler pointerMovedHandler = null;
            PointerEventHandler pointerReleasedHandler = null;

            pointerMovedHandler = (object s, PointerRoutedEventArgs e) =>
            {
                Point pt = e.GetCurrentPoint(mousePositionWindow).Position;
                pt = gt.TransformPoint(pt);
                Point delta = new Point
                {
                    X = pt.X - pointMouseDown.X,
                    Y = pt.Y - pointMouseDown.Y
                };

                if (!(control.RenderTransform is CompositeTransform compositeTransform))
                {
                    compositeTransform = new CompositeTransform();
                    control.RenderTransform = compositeTransform;
                }
                compositeTransform.TranslateX += delta.X;
                compositeTransform.TranslateY += delta.Y;
                control.RenderTransform = compositeTransform;
                pointMouseDown = pt;
                if (progress != null)
                {
                    progress.Report(pt);
                }
            };

            pointerReleasedHandler = (object s, PointerRoutedEventArgs e) =>
            {
                UIElement localControl = (UIElement)s;
                localControl.PointerMoved -= pointerMovedHandler;
                localControl.PointerReleased -= pointerReleasedHandler;
                localControl.ReleasePointerCapture(origE.Pointer);
                Point exitPoint = e.GetCurrentPoint(mousePositionWindow).Position;

                taskCompletionSource.SetResult(exitPoint);
            };

            control.CapturePointer(origE.Pointer);
            control.PointerMoved += pointerMovedHandler;
            control.PointerReleased += pointerReleasedHandler;
            return taskCompletionSource.Task;
        }

        public static async Task<StorageFolder> GetSaveFolder([CallerMemberName] string cmb = "", [CallerLineNumber] int cln = 0, [CallerFilePath] string cfp = "")
        {
            // System.Diagnostics.Debug.WriteLine($"GetSaveFolder called.  File: {cfp}, Method: {cmb}, Line Number: {cln}");

            string token = "default";
            StorageFolder folder = null;
            try
            {
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(token))
                {
                    folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                    return folder;
                }
            }
            catch { }

            string content = "After clicking on \"Close\" pick the default location for all your Catan saved state";
            MessageDialog dlg = new MessageDialog(content, "Catan");
            try
            {
                await dlg.ShowAsync();

                FolderPicker picker = new FolderPicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                picker.FileTypeFilter.Add("*");

                folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, folder);
                }
                else
                {
                    folder = ApplicationData.Current.LocalFolder;
                }

                return folder;
            }
            catch (Exception except)
            {
                Debug.WriteLine(except.ToString());
            }

            return null;
        }

        public static async Task<string> GetUserString(string title, string defaultText)
        {
            var inputTextBox = new TextBox { AcceptsReturn = false, Text = defaultText };
            (inputTextBox as FrameworkElement).VerticalAlignment = VerticalAlignment.Bottom;
            var dialog = new ContentDialog
            {
                Content = inputTextBox,
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel"
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }

        public static ResourceType HarborTypeToResourceType(HarborType ht)
        {
            switch (ht)
            {
                case HarborType.Sheep:
                    return ResourceType.Sheep;

                case HarborType.Wood:
                    return ResourceType.Wood;

                case HarborType.Ore:
                    return ResourceType.Ore;

                case HarborType.Wheat:
                    return ResourceType.Wheat;

                case HarborType.Brick:
                    return ResourceType.Brick;

                case HarborType.ThreeForOne:
                case HarborType.None:
                default:
                    break;
            }

            return ResourceType.None;
        }

        public static bool IsNumber(VirtualKey key)
        {
            if ((int)key >= (int)VirtualKey.Number0 && (int)key <= (int)VirtualKey.Number9)
            {
                return true;
            }

            if ((int)key >= (int)VirtualKey.NumberPad0 && (int)key <= (int)VirtualKey.NumberPad9)
            {
                return true;
            }

            return false;
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static T Peek<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                return list.Last();
            }

            return default(T);
        }

        public static T Pop<T>(this List<T> list)
        {
            T t = list.Last();
            list.RemoveAt(list.Count - 1);
            return t;
        }

        public static T Pop<T>(this ObservableCollection<T> list)
        {
            T t = list.Last();
            list.RemoveAt(list.Count - 1);
            return t;
        }

        public static void Push<T>(this ObservableCollection<T> list, T t)
        {
            list.Add(t);
        }

        public static void Push<T>(this List<T> list, T t)
        {
            list.Add(t);
        }

        public static async Task<string> ReadWholeFile(StorageFolder folder, string filename)
        {
            try
            {
                StorageFile file = await folder.GetFileAsync(filename);
                string contents = await FileIO.ReadTextAsync(file);
                return contents;
            }
            catch
            {
                return "";
            }
        }

        static public async Task RunStoryBoard(Storyboard sb, bool callStop = true, double ms = 500, bool setTimeout = true)
        {
            if (setTimeout)
            {
                foreach (Timeline animations in sb.Children)
                {
                    animations.Duration = new Duration(TimeSpan.FromMilliseconds(ms));
                }
            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            void completed(object s, object e) => tcs.TrySetResult(null);
            try
            {
                sb.Completed += completed;
                sb.Begin();
                await tcs.Task;
            }
            finally
            {
                sb.Completed -= completed;
                if (callStop)
                {
                    sb.Stop();
                }
            }
        }

        static public void RunStoryBoardAsync(Storyboard sb, double ms = 500, bool setTimeout = true)
        {
            if (setTimeout)
            {
                foreach (Timeline animations in sb.Children)
                {
                    animations.Duration = new Duration(TimeSpan.FromMilliseconds(ms));
                }
            }

            sb.Begin();
        }

        static public void SetFlipAnimationSpeed(Storyboard sb, double milliseconds)
        {
            foreach (Timeline animation in sb.Children)
            {
                if (animation.Duration != TimeSpan.FromMilliseconds(0))
                {
                    animation.Duration = TimeSpan.FromMilliseconds(milliseconds);
                }

                if (animation.BeginTime != TimeSpan.FromMilliseconds(0))
                {
                    animation.BeginTime = TimeSpan.FromMilliseconds(milliseconds);
                }
            }
        }

        public static double SetupFlipAnimation(bool flipToFaceUp, DoubleAnimation back, DoubleAnimation front, double animationTimeInMs, double startAfter = 0)
        {
            if (flipToFaceUp)
            {
                back.To = -90;
                front.To = 0;
                front.Duration = TimeSpan.FromMilliseconds(animationTimeInMs);
                back.Duration = TimeSpan.FromMilliseconds(animationTimeInMs);
                back.BeginTime = TimeSpan.FromMilliseconds(startAfter);
                front.BeginTime = TimeSpan.FromMilliseconds(startAfter + animationTimeInMs);
            }
            else
            {
                back.To = 0;
                front.To = 90;
                back.Duration = TimeSpan.FromMilliseconds(animationTimeInMs);
                front.Duration = TimeSpan.FromMilliseconds(animationTimeInMs);
                front.BeginTime = TimeSpan.FromMilliseconds(startAfter);
                back.BeginTime = TimeSpan.FromMilliseconds(startAfter + animationTimeInMs);
            }
            return animationTimeInMs;
        }

        public static async Task ShowErrorText(string s, string title)
        {
            MessageDialog dlg = new MessageDialog(s)
            {
                Title = title
            };

            await dlg.ShowAsync();
        }

        public static Task<object> ToTask(this Storyboard storyboard, CancellationTokenSource cancellationTokenSource = null)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.AttachedToParent);

            if (cancellationTokenSource != null)
            {
                // when the task is cancelled,
                // Stop the storyboard
                cancellationTokenSource.Token.Register
                (
                    () =>
                    {
                        storyboard.Stop();
                    }
                );
            }

            void onCompleted(object s, object e)
            {
                storyboard.Completed -= onCompleted;

                tcs.SetResult(null);
            }

            storyboard.Completed += onCompleted;

            // start the storyboard during the conversion.
            storyboard.Begin();

            return tcs.Task;
        }

        public static void TraceMessage(this object o, string toWrite, [CallerMemberName] string cmb = "", [CallerLineNumber] int cln = 0, [CallerFilePath] string cfp = "")
        {
            System.Diagnostics.Debug.WriteLine($"{cfp}({cln}):{toWrite}\t\t[Caller={cmb}]");
        }

        /// <summary>
        ///     This will serialize a IList<> into a string that can be deserialized. You can pass in an arbitrary list of thingies
        ///     and it will serialize the property passed in
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="propName"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        public static bool TryParse<T>(this Enum theEnum, string valueToParse, out T returnValue)
        {
            returnValue = default(T);
            if (int.TryParse(valueToParse, out int intEnumValue))
            {
                if (Enum.IsDefined(typeof(T), intEnumValue))
                {
                    returnValue = (T)(object)intEnumValue;
                    return true;
                }
            }
            return false;
        }

        #endregion Methods

        #region Interfaces

        //
        //  an interface called by the drag and drop code so we can simlulate the DragOver behavior
        public interface IDragAndDropProgress
        {
            #region Methods

            void PointerUp(Point value);

            void Report(Point value);

            #endregion Methods
        }

        #endregion Interfaces

        #region Classes

        public class KeyValuePair
        {
            #region Properties

            public string Key { get; set; }

            public string Value { get; set; }

            #endregion Properties

            #region Constructors + Destructors

            public KeyValuePair(string key, string value)
            {
                Key = key;
                Value = value;
            }

            #endregion Constructors + Destructors
        }

        #endregion Classes
    }

    public class EnumDescription : Attribute
    {
        #region Properties

        public string Description { get; }

        #endregion Properties

        #region Constructors + Destructors

        public EnumDescription(string description)
        {
            Description = description;
        }

        #endregion Constructors + Destructors
    }
}