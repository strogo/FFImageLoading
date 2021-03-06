﻿using System;
using System.Collections.Generic;
using FFImageLoading.Forms;
using Xamarin.Forms;

namespace FFImageLoading.Svg.Forms
{
#if __IOS__
            [Foundation.Preserve(AllMembers = true)]
#elif __ANDROID__
            [Android.Runtime.Preserve(AllMembers = true)]
#endif

    /// <summary>
    /// SvgCachedImage by Daniel Luberda
    /// </summary>
    [Preserve(AllMembers = true)]
    public class SvgCachedImage : CachedImage
    {
        public static void Init()
        {
        }

        static FFImageLoading.Forms.ImageSourceConverter _imageSourceConverter = new FFImageLoading.Forms.ImageSourceConverter();

        public SvgCachedImage() : base()
        {
            ReplaceStringMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// The source property.
        /// </summary>
        public static new readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(SvgCachedImage), default(ImageSource), BindingMode.OneWay, propertyChanging: OnSourcePropertyChanging);

        static void OnSourcePropertyChanging(BindableObject bindable, object oldValue, object newValue)
        {
            var element = (CachedImage)bindable;

            // HACK for the strange issue when TypeConverter is not respected (somehow FileImageSource is returned !?!?!?!?)
            var fileSource = newValue as FileImageSource;
            if (fileSource?.File != null)
            {
                if (fileSource.File.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                {
                    element.Source = new SvgImageSource(fileSource, 0, 0, true, ((SvgCachedImage)element).ReplaceStringMap);
                    return;
                }
                else if (fileSource.File.IsSvgFileUrl())
                {
                    element.Source = new SvgImageSource(fileSource, 0, 0, true, ((SvgCachedImage)element).ReplaceStringMap);
                    return;
                }
            }

            var uriSource = newValue as UriImageSource;
            if (uriSource?.Uri?.OriginalString != null)
            {
                if (uriSource.Uri.OriginalString.IsSvgDataUrl())
                {
                    element.Source = new SvgImageSource(uriSource, 0, 0, true, ((SvgCachedImage)element).ReplaceStringMap);
                    return;
                }
                else if (uriSource.Uri.OriginalString.IsSvgFileUrl())
                {
                    element.Source = new SvgImageSource(uriSource, 0, 0, true, ((SvgCachedImage)element).ReplaceStringMap);
                    return;
                }
            }

            var dataUrlSource = newValue as DataUrlImageSource;
            if (dataUrlSource?.DataUrl != null)
            {
                if (dataUrlSource.DataUrl.IsSvgDataUrl())
                {
                    element.Source = new SvgImageSource(dataUrlSource, 0, 0, true, ((SvgCachedImage)element).ReplaceStringMap);
                    return;
                }
            }

            var embeddedSource = newValue as EmbeddedResourceImageSource;
            if (embeddedSource?.Uri?.OriginalString != null)
            {
                if (embeddedSource.Uri.OriginalString.IsSvgFileUrl())
                {
                    element.Source = new SvgImageSource(embeddedSource, 0, 0, true, ((SvgCachedImage)element).ReplaceStringMap);
                    return;
                }
            }

            element.Source = newValue as ImageSource;
        }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        [TypeConverter(typeof(SvgImageSourceConverter))]
        public new ImageSource Source
        {
            get
            {
                return (ImageSource)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        /// <summary>
        /// The loading placeholder property.
        /// </summary>
        public static new readonly BindableProperty LoadingPlaceholderProperty = BindableProperty.Create(nameof(LoadingPlaceholder), typeof(ImageSource), typeof(SvgCachedImage), default(ImageSource), propertyChanging: OnLoadingPlaceholderPropertyChanging);

        static void OnLoadingPlaceholderPropertyChanging(BindableObject bindable, object oldValue, object newValue)
        {
            var element = (CachedImage)bindable;
            element.LoadingPlaceholder = newValue as ImageSource;
        }

        /// <summary>
        /// Gets or sets the loading placeholder image.
        /// </summary>
        [TypeConverter(typeof(SvgImageSourceConverter))]
        public new ImageSource LoadingPlaceholder
        {
            get
            {
                return (ImageSource)GetValue(LoadingPlaceholderProperty);
            }
            set
            {
                SetValue(LoadingPlaceholderProperty, value);
            }
        }

        /// <summary>
        /// The error placeholder property.
        /// </summary>
        public static new readonly BindableProperty ErrorPlaceholderProperty = BindableProperty.Create(nameof(ErrorPlaceholder), typeof(ImageSource), typeof(SvgCachedImage), default(ImageSource), propertyChanging: OnErrorPlaceholderPropertyChanging);

        static void OnErrorPlaceholderPropertyChanging(BindableObject bindable, object oldValue, object newValue)
        {
            var element = (CachedImage)bindable;
            element.ErrorPlaceholder = newValue as ImageSource;
        }

        /// <summary>
        /// Gets or sets the error placeholder image.
        /// </summary>
        [TypeConverter(typeof(SvgImageSourceConverter))]
        public new ImageSource ErrorPlaceholder
        {
            get
            {
                return (ImageSource)GetValue(ErrorPlaceholderProperty);
            }
            set
            {
                SetValue(ErrorPlaceholderProperty, value);
            }
        }

        /// <summary>
        /// The error placeholder property.
        /// </summary>
        public static readonly BindableProperty ReplaceStringMapProperty = BindableProperty.Create(nameof(ReplaceStringMap), typeof(Dictionary<string, string>), typeof(SvgCachedImage), default(Dictionary<string, string>), propertyChanged: new BindableProperty.BindingPropertyChangedDelegate(HandleReplaceStringMapPropertyChangedDelegate));

        /// <summary>
        /// Used to define replacement map which will be used to
        /// replace text inside SVG file (eg. changing colors values)
        /// </summary>
        /// <value>The replace string map.</value>
        public Dictionary<string, string> ReplaceStringMap
        {
            get
            {
                return (Dictionary<string, string>)GetValue(ReplaceStringMapProperty);
            }
            set
            {

                SetValue(ReplaceStringMapProperty, value);
            }
        }

        static void HandleReplaceStringMapPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            if (oldValue != newValue)
            {
                var cachedImage = bindable as SvgCachedImage;
                if (cachedImage != null && cachedImage.Source != null)
                {
                    cachedImage.ReloadImage();
                }
            }
        }

        /// <summary>
        /// Setups the on before image loading.
        /// You can add additional logic here to configure image loader settings before loading
        /// </summary>
        /// <param name="imageLoader">Image loader.</param>
        protected override void SetupOnBeforeImageLoading(Work.TaskParameter imageLoader)
        {
            base.SetupOnBeforeImageLoading(imageLoader);

            if (ReplaceStringMap != null)
            {
                var source = imageLoader.CustomDataResolver as Work.IVectorDataResolver;
                if (source != null && source.ReplaceStringMap == null)
                    source.ReplaceStringMap = ReplaceStringMap;

                var loadingSource = imageLoader.CustomLoadingPlaceholderDataResolver as Work.IVectorDataResolver;
                if (loadingSource != null && loadingSource.ReplaceStringMap == null)
                    loadingSource.ReplaceStringMap = ReplaceStringMap;

                var errorSource = imageLoader.CustomErrorPlaceholderDataResolver as Work.IVectorDataResolver;
                if (errorSource != null && errorSource.ReplaceStringMap == null)
                    errorSource.ReplaceStringMap = ReplaceStringMap;
            }
        }
    }
}
