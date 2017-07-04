using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Input.Inking;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace InfinitePainter
{
    public sealed partial class MainPage
    {
        private async Task SaveInkAsync()
        {
            if (inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add("Gif with embedded ISF", new List<string> { ".gif" });

                var file = await savePicker.PickSaveFileAsync();

                if (null != file)
                {
                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        // This single method will get all the strokes and save them to the file
                        await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(stream);
                    }
                }
            }
        }

        private async Task LoadInkAsync()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;

            // filter files to show both gifs (with embedded isf) and isf (ink) files
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".isf");

            var file = await openPicker.PickSingleFileAsync();

            if (null != file)
            {
                using (var stream = await file.OpenSequentialReadAsync())
                {
                    // Just like saving, it's only one method to load the ink into the canvas
                    await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(stream);
                }
            }
        }

        private async Task LoadBackImageAsync()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;

            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".gif");

            var file = await openPicker.PickSingleFileAsync();

            if (null != file)
            {
                BACK_IMAGE = file;
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapImage img = new BitmapImage();
                    img.SetSource(stream);
                    backImage.Source = img;
                }
            }
        }
        
        private async void Export_InkedImage()
        {
            if (BACK_IMAGE == null)
            {
                await SaveInkAsync();
                return;
            }
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });
            savePicker.SuggestedFileName = "Coloring Page";
            StorageFile saveFile = await savePicker.PickSaveFileAsync();
            await Save_InkedImagetoFile(saveFile);
        }
        
        private async Task Save_InkedImagetoStream(IRandomAccessStream stream)
        {
            //var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(((BitmapImage)backImage.Source).UriSource);

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            
            CanvasBitmap image;
            using (var imgStream = await BACK_IMAGE.OpenAsync(FileAccessMode.ReadWrite))
            {
                image = await CanvasBitmap.LoadAsync(device, imgStream);
            }
            //var image = await CanvasBitmap.LoadAsync(device, file.Path);
            //var image = new CanvasBitmap();
            using (var renderTarget = new CanvasRenderTarget(device, (int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, image.Dpi))
            {
                using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
                {
                    ds.Clear(Colors.White);
                    if (image != null)
                        ds.DrawImage(image, new Rect(0, 0, (int)inkCanvas.ActualWidth,
                            (int)inkCanvas.ActualHeight));
                    ds.DrawInk(inkCanvas.InkPresenter.StrokeContainer.GetStrokes());
                }

                await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
            }
        }

        /// Saves coloring page to specified file.
        private async Task Save_InkedImagetoFile(StorageFile saveFile)
        {
            if (saveFile != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(saveFile);

                using (var outStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await Save_InkedImagetoStream(outStream);
                }

                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(saveFile);
            }
        }
    }
}
