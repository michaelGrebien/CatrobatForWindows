﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Catrobat.IDE.Core;
using Catrobat.IDE.Core.Models;
using Catrobat.IDE.Core.Resources.Localization;
using Catrobat.IDE.Core.Services;
using Catrobat.IDE.Core.Services.Storage;
using Catrobat.IDE.Core.UI.PortableUI;
using Catrobat.IDE.Core.ViewModels;
using Catrobat.IDE.Core.ViewModels.Editor.Looks;
using GalaSoft.MvvmLight.Messaging;

namespace Catrobat.IDE.WindowsShared.Services
{

    public class PictureServiceWindowsShared : IPictureService
    {

        private Core.Models.Program _program;
        private Look _lookToEdit;

        public string ImageFileExtensionPrefix
        {
            get { return "catrobat_ide_"; }
        }

        public void ChoosePictureFromLibraryAsync()
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            foreach (var extension in SupportedImageFileTypes)
                openPicker.FileTypeFilter.Add(extension);

            StorageFile file;

            try
            {
                ServiceLocator.DispatcherService.RunOnMainThread(
                    openPicker.PickSingleFileAndContinue);
            }
            catch (Exception)
            {
                Debugger.Break();
                throw;
            }
        }

        public void TakePictureAsync()
        {
            //CameraCaptureUI dialog = new CameraCaptureUI();
            //Size aspectRatio = new Size(16, 9);
            //dialog.PhotoSettings.CroppedAspectRatio = aspectRatio;

            //StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);



            //var cam = new CameraCaptureUI();
            //var file = await cam.CaptureFileAsync(CameraCaptureUIMode.Photo);

            //if (file != null)
            //{
            //    var fileStream = await file.OpenAsync(FileAccessMode.Read);
            //    var memoryStream = new MemoryStream();
            //    fileStream.AsStreamForRead().CopyTo(memoryStream);

            //    fileStream.Seek(0);
            //    var imagetobind = new BitmapImage();
            //    imagetobind.SetSource(fileStream);
            //    var writeableBitmap = new WriteableBitmap(imagetobind.PixelWidth, imagetobind.PixelHeight);
            //    await writeableBitmap.FromStream(memoryStream.AsRandomAccessStream());
            //    memoryStream.Seek(0, SeekOrigin.Begin);
            //    var portableImage = new PortableImage(writeableBitmap)
            //    {
            //        Width = writeableBitmap.PixelWidth,
            //        Height = writeableBitmap.PixelHeight,
            //        EncodedData = memoryStream
            //    };

            //    return new PictureServiceResult
            //    {
            //        Status = PictureServiceStatus.Success,
            //        Image = portableImage
            //    };
            //}
            //else
            //{
            //    return new PictureServiceResult
            //    {
            //        Status = PictureServiceStatus.Cancelled
            //    };
            //}
        }

        public IEnumerable<string> SupportedImageFileTypes
        {
            get
            {
                var supportedTypes = new List<string>();
                supportedTypes.AddRange(StorageConstants.SupportedImageFileTypes);
                supportedTypes.Add(StorageConstants.PaintImageImportFileExtension);
                return supportedTypes;
            }
        }

        public async Task DrawPictureAsync(Core.Models.Program program = null, Look lookToEdit = null)
        {
            _program = program;
            _lookToEdit = lookToEdit;

            var localFolder = ApplicationData.Current.LocalFolder;

            if (program != null && lookToEdit != null)
            {
                using (var storage = StorageSystem.GetStorage())
                {
                    if (await storage.FileExistsAsync(StorageConstants.TempPaintImagePath))
                        await storage.DeleteFileAsync(StorageConstants.TempPaintImagePath);

                    var filePath = Path.Combine(program.BasePath,
                        StorageConstants.ProgramLooksPath, lookToEdit.FileName);

                    await storage.CopyFileAsync(filePath,
                        StorageConstants.TempPaintImagePath);
                }
            }
            else
            {
                //throw new NotImplementedException("Create empty image here");
                // TODO: create empty image with the same dimensions as the devices width and heigt


                var imageWidth = ServiceLocator.SystemInformationService.ScreenWidth;
                var imageHeight = ServiceLocator.SystemInformationService.ScreenHeight;

                using (var storage = StorageSystem.GetStorage())
                {
                    if (await storage.FileExistsAsync(StorageConstants.TempPaintImagePath))
                        await storage.DeleteFileAsync(StorageConstants.TempPaintImagePath);

                    var stream = await storage.OpenFileAsync(StorageConstants.TempPaintImagePath,
                        StorageFileMode.Create, StorageFileAccess.Write);

                    var encoder = await BitmapEncoder.CreateAsync(
                        BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream());

                    var pixels = new byte[imageWidth * imageHeight * 4];

                    for (var pixelStart = 0; pixelStart < pixels.Length; pixelStart += 4)
                    {
                        pixels[pixelStart + 0] = 0x00; // Full transparent
                        pixels[pixelStart + 1] = 0x00;
                        pixels[pixelStart + 2] = 0x00;
                        pixels[pixelStart + 3] = 0x00;
                    }

                    encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight,
                        (uint)imageWidth, (uint)imageHeight, 96, 96, pixels);
                    await encoder.FlushAsync();
                }
            }

            var paintTempFolderPath = Path.GetDirectoryName(StorageConstants.TempPaintImagePath);
            var paintTempFolderName = Path.GetFileName(StorageConstants.TempPaintImagePath);
            var paintTempFolder = await localFolder.CreateFolderAsync(paintTempFolderPath, CreationCollisionOption.OpenIfExists);
            var file = await paintTempFolder.GetFileAsync(paintTempFolderName);


            var options = new Windows.System.LauncherOptions
            {
                DisplayApplicationPicker = false
            };

            try
            {
                bool success = await Windows.System.Launcher.
                    LaunchFileAsync(file, options);
                if (success)
                {
                    // File launch OK
                }
                else
                {
                    // File launch failed
                }
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
        }

        public async void RecievedFiles(IEnumerable<object> files)
        {
            var fileArray = files as object[] ?? files.ToArray();

            if (fileArray.Length == 0)
            {
                ServiceLocator.DispatcherService.RunOnMainThread(() =>
                    ServiceLocator.NavigationService.NavigateTo<NewLookSourceSelectionViewModel>());
            }

            var file = (StorageFile)fileArray[0];
            var fileStream = await file.OpenReadAsync();
            var memoryStream = new MemoryStream();
            fileStream.AsStreamForRead().CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var imagetobind = new BitmapImage();
            await imagetobind.SetSourceAsync(memoryStream.AsRandomAccessStream());

            var writeableBitmap = new WriteableBitmap(imagetobind.PixelWidth, imagetobind.PixelHeight);
            await writeableBitmap.FromStream(memoryStream.AsRandomAccessStream());
            memoryStream.Seek(0, SeekOrigin.Begin);
            var portableImage = new PortableImage(writeableBitmap)
            {
                Width = writeableBitmap.PixelWidth,
                Height = writeableBitmap.PixelHeight,
                EncodedData = memoryStream
            };

            if (_lookToEdit == null)
            {
                if (portableImage != null) // TODO: check if image is ok
                {
                    var message = new GenericMessage<PortableImage>(portableImage);
                    Messenger.Default.Send(message, ViewModelMessagingToken.LookImageListener);

                    ServiceLocator.DispatcherService.RunOnMainThread(() =>
                        ServiceLocator.NavigationService.NavigateTo<LookNameChooserViewModel>());
                }
                else
                {
                    ServiceLocator.NotifictionService.ShowMessageBox(AppResources.Editor_MessageBoxWrongImageFormatHeader,
                        AppResources.Editor_MessageBoxWrongImageFormatText, delegate { /* no action */ }, MessageBoxOptions.Ok);
                }
            }
            else
            {
                using (var storage = StorageSystem.GetStorage())
                {
                    var filePath = Path.Combine(_program.BasePath,
                        StorageConstants.ProgramLooksPath, _lookToEdit.FileName);

                    await storage.DeleteImageAsync(filePath);

                    var lookFileStream = await storage.OpenFileAsync(filePath,
                        StorageFileMode.Create, StorageFileAccess.Write);

                    await (await file.OpenReadAsync()).AsStream().CopyToAsync(lookFileStream);

                    lookFileStream.Dispose();

                    await storage.TryCreateThumbnailAsync(filePath);

                    _lookToEdit.Image = await storage.LoadImageThumbnailAsync(filePath);
                }

                _lookToEdit = null;
            }
        }
    }
}