using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using System.Windows;

namespace InfinitePainter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //History
        private Stack<InkStroke> HISTORY_STROKES;

        //Backround image
        private StorageFile BACK_IMAGE;

        // Stroke selection tool.
        private Polyline lasso;
        // Stroke selection area.
        private Rect boundingRect;
        //

        public MainPage()
        {
            this.InitializeComponent();
            InitialInk();
            InitialMenu();
            HISTORY_STROKES = new Stack<InkStroke>();

        }

        private void InitialInk()
        {

            InkPresenter myInkPresenter = this.inkCanvas.InkPresenter;
            myInkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Mouse;
            InkDrawingAttributes myAttributes = myInkPresenter.CopyDefaultDrawingAttributes();
            myAttributes.DrawAsHighlighter = false;
            myAttributes.IgnorePressure = false;
            myAttributes.FitToCurve = true;
            myInkPresenter.UpdateDefaultDrawingAttributes(myAttributes);

            // 
            inkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction =
                InkInputRightDragAction.LeaveUnprocessed;
            // Listen for unprocessed pointer events from modified input.
            // The input is used to provide selection functionality.
            inkCanvas.InkPresenter.UnprocessedInput.PointerPressed +=
                UnprocessedInput_PointerPressed;
            inkCanvas.InkPresenter.UnprocessedInput.PointerMoved +=
                UnprocessedInput_PointerMoved;
            inkCanvas.InkPresenter.UnprocessedInput.PointerReleased +=
                UnprocessedInput_PointerReleased;

            // Listen for new ink or erase strokes to clean up selection UI.
            inkCanvas.InkPresenter.StrokeInput.StrokeStarted +=
                StrokeInput_StrokeStarted;
            inkCanvas.InkPresenter.StrokesErased +=
                InkPresenter_StrokesErased;
        }

        private void OnMenuItemClick(object sender, ItemClickEventArgs e)
        {
            var menuItem = e.ClickedItem as MenuItem;
            Symbol icon = menuItem.Icon;
            switch (icon)
            {
                case Symbol.Document:
                    LoadInkAsync();
                    break;
                case Symbol.BrowsePhotos:
                    LoadBackImageAsync();
                    break;
                case Symbol.Save:
                    SaveInkAsync();
                    break;
                case Symbol.Send:
                    Export_InkedImage();
                    break;
                default:
                    break;
            }
            // close the pane
            hamburgerMenuControl.IsPaneOpen = false;
        }

        private void BackImage_Opened(object sender, RoutedEventArgs e)
        {
            inkCanvas.Height = backImage.ActualHeight;
            inkCanvas.Width = backImage.ActualWidth;
            selectionCanvas.Height = backImage.ActualHeight;
            selectionCanvas.Width = backImage.ActualWidth;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            foreach(var s in strokes)
            {
                if(s.Selected == true)
                {
                    inkCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
                    return;
                }

            }
            foreach (InkStroke s in strokes)
                s.Selected = true;
            inkCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            Copy_Click(sender, e);
            Delete_Click(sender, e);
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.InkPresenter.StrokeContainer.CanPasteFromClipboard())
                inkCanvas.InkPresenter.StrokeContainer.PasteFromClipboard(new Point(0, 0));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (strokes.Count > 0)
            {
                foreach (var s in strokes)
                {
                    if (s.Selected == true)
                    {
                        inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                        return;
                    }

                }

                foreach (InkStroke s in strokes){
                    s.Selected = true;
                }
                inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            }

        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            myScrollViewer.ChangeView(myScrollViewer.HorizontalOffset, myScrollViewer.VerticalOffset, myScrollViewer.ZoomFactor + 0.2f);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            myScrollViewer.ChangeView(myScrollViewer.HorizontalOffset, myScrollViewer.VerticalOffset, myScrollViewer.ZoomFactor - 0.2f);

        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<InkStroke> strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (strokes.Count > 0)
            {
                HISTORY_STROKES.Push(strokes[strokes.Count - 1].Clone());
                strokes[strokes.Count - 1].Selected = true;
                inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if(HISTORY_STROKES.Count > 0)
            {
                inkCanvas.InkPresenter.StrokeContainer.AddStroke(HISTORY_STROKES.Pop());
            }
        }

    }
}
