﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Storage;
using Catrobat.IDE.Core.Services.Storage;

namespace Catrobat.IDE.Phone.Services.Storage
{
    public class ResourcesPhone : IResourceLoader
    {
        private readonly List<Stream> _openedStreams = new List<Stream>();

        public Stream OpenResourceStream(ResourceScope resourceScope, string uri)
        {
            var projectPath = "";

            switch (resourceScope)
            {
                case ResourceScope.Core:
                    {
                        projectPath = "Catrobat.IDE.Core";
                        var path = Path.Combine(projectPath, uri);
                        path = path.Replace("\\", "/");
                        path = path.Replace("/", ".");
                        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                        _openedStreams.Add(stream);
                        return stream;
                    }
                case ResourceScope.IdePhone:
                {
                    projectPath = "";// "/Catrobat.IDE.Phone;component";
                        var resourceUri = new Uri(projectPath + uri, UriKind.Relative);
                        var resource = Application.GetResourceStream(resourceUri);

                        if (resource != null)
                        {
                            var stream = resource.Stream;
                            _openedStreams.Add(stream);
                            return stream;
                        }

                        return null;
                    }
                case ResourceScope.TestsPhone:
                    {
                        projectPath = "";
                        var resourceUri = new Uri(projectPath + uri, UriKind.Relative);
                        var resource = Application.GetResourceStream(resourceUri);

                        if (resource != null)
                        {
                            var stream = resource.Stream;
                            _openedStreams.Add(stream);
                            return stream;
                        }

                        return null;
                    }
                case ResourceScope.Resources:
                    {
                        projectPath = "Content/Resources/";
                        var resourceUri = new Uri(projectPath + uri, UriKind.Relative);
                        var resource = Application.GetResourceStream(resourceUri);

                        if (resource != null)
                        {
                            var stream = resource.Stream;
                            _openedStreams.Add(stream);
                            return stream;
                        }

                        return null;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("project");
                    }
            }
        }

        public object LoadImage(ResourceScope resourceScope, string path)
        {
            if (resourceScope != ResourceScope.IdePhone)
                throw new NotImplementedException("Only ResourceScope.IdePhone is implemented");

            var image = new BitmapImage();
            var stream = OpenResourceStream(resourceScope, path);
            if (stream != null)
            {
                image.SetSource(stream);
                return image;
            }
            else
            {
                return new BitmapImage(new Uri(path, UriKind.Relative));
                //return null;
            }
            
        }

        public Task<Stream> OpenResourceStreamAsync(ResourceScope resourceScope, string uri)
        {
            return Task.Run(() => OpenResourceStream(resourceScope, uri));
        }

        public Task<object> LoadImageAsync(ResourceScope resourceScope, string path)
        {
            return Task.Run(() => LoadImage(resourceScope, path));
        }

        public void Dispose()
        {
            foreach (var stream in _openedStreams)
            {
                stream.Dispose();
            }
        }
    }
}