using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ImageViewerMaui
{
    public class ImageViewer : ContentView, IDisposable
    {
        private double _currentScale = 1;
        private double _startScale = 1;
        private double _xOffset = 0;
        private double _yOffset = 0;

        private readonly Image _contentImage;
        private readonly PinchGestureRecognizer _pinchGesture;
        private readonly PanGestureRecognizer _panGesture;
        private readonly TapGestureRecognizer _tapGesture;

        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(ImageViewer), null,
                propertyChanged: (bindable, oldVal, newVal) => ((ImageViewer)bindable)._contentImage.Source = (ImageSource)newVal);

        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly BindableProperty IsBounceEnabledProperty =
            BindableProperty.Create(nameof(IsBounceEnabled), typeof(bool), typeof(ImageViewer), true);

        public bool IsBounceEnabled
        {
            get => (bool)GetValue(IsBounceEnabledProperty);
            set => SetValue(IsBounceEnabledProperty, value);
        }

        public ImageViewer()
        {
            _contentImage = new Image
            {
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            Content = _contentImage;

            _pinchGesture = new PinchGestureRecognizer();
            _panGesture = new PanGestureRecognizer();
            _tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };

            _pinchGesture.PinchUpdated += OnPinchUpdated;
            _panGesture.PanUpdated += OnPanUpdated;
            _tapGesture.Tapped += OnDoubleTapped;

            GestureRecognizers.Add(_pinchGesture);
            GestureRecognizers.Add(_panGesture);
            GestureRecognizers.Add(_tapGesture);
        }

        #region Manejo de Gestos

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    _startScale = Content.Scale;
                    Content.AnchorX = 0.5;
                    Content.AnchorY = 0.5;
                    break;

                case GestureStatus.Running:
                    _currentScale += (e.Scale - 1) * _startScale;
                    _currentScale = Math.Clamp(_currentScale, 1, 8);
                    Content.Scale = _currentScale;
                    break;

                case GestureStatus.Completed:
                    if (Content.Scale < 1.1)
                        _ = ResetImage();
                    break;
            }
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (Content.Scale <= 1) return;

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    double targetX = _xOffset + e.TotalX;
                    double targetY = _yOffset + e.TotalY;

                    double scaledWidth = Content.Width * Content.Scale;
                    double scaledHeight = Content.Height * Content.Scale;
                    double maxTransX = (scaledWidth - Content.Width) / 2;
                    double maxTransY = (scaledHeight - Content.Height) / 2;

                    maxTransX = Math.Max(0, maxTransX);
                    maxTransY = Math.Max(0, maxTransY);

                    Content.TranslationX = Math.Clamp(targetX, -maxTransX, maxTransX);
                    Content.TranslationY = Math.Clamp(targetY, -maxTransY, maxTransY);

                    System.Diagnostics.Debug.WriteLine($"[Pan] Target: ({targetX:F2}, {targetY:F2}) | Clamped: ({Content.TranslationX:F2}, {Content.TranslationY:F2}) | Max: +/-{maxTransX:F2}");
                    break;

                case GestureStatus.Completed:
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    break;
            }
        }

        private async void OnDoubleTapped(object sender, EventArgs e)
        {
            if (Content.Scale > 1)
            {
                await ResetImage();
            }
            else
            {
                _currentScale = 2.5;
                if (IsBounceEnabled)
                {
                    await Content.ScaleTo(_currentScale, 600, GetSpringEasing());
                }
                else
                {
                    await Content.ScaleTo(_currentScale, 250, Easing.CubicOut);
                }
            }
        }

        private Task ResetImage()
        {
            _currentScale = 1;
            _xOffset = 0;
            _yOffset = 0;

            if (!IsBounceEnabled)
            {
                return Task.WhenAll(
                    Content.ScaleTo(1, 250, Easing.CubicOut),
                    Content.TranslateTo(0, 0, 250, Easing.CubicOut)
                );
            }

            var tcs = new TaskCompletionSource<bool>();

            var startScale = Content.Scale;
            var startTransX = Content.TranslationX;
            var startTransY = Content.TranslationY;

            var animation = new Animation();
            var easing = GetSpringEasing();

            animation.Add(0, 1, new Animation(v =>
            {
                var easeVal = easing.Ease(v);
                var targetScale = startScale + (1 - startScale) * easeVal;

                Content.Scale = Math.Max(targetScale, 0.01);
            }));

            animation.Add(0, 1, new Animation(v =>
            {
                var easeVal = easing.Ease(v);
                Content.TranslationX = startTransX + (0 - startTransX) * easeVal;
                Content.TranslationY = startTransY + (0 - startTransY) * easeVal;
            }));

            animation.Commit(this, "ResetImageBounce", length: 800, finished: (v, c) =>
            {
                tcs.SetResult(true);
            });

            return tcs.Task;
        }

        private Easing GetSpringEasing()
        {
            return new Easing(t =>
                Math.Sin(-13 * Math.PI / 2 * (t + 1)) * Math.Pow(2, -10 * t) + 1
            );
        }

        #endregion

        #region Gestión de Memoria

        protected override void OnHandlerChanging(HandlerChangingEventArgs args)
        {
            base.OnHandlerChanging(args);
            if (args.NewHandler == null)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            this.AbortAnimation("ResetImageBounce");
            _contentImage?.CancelAnimations();

            if (_pinchGesture != null) _pinchGesture.PinchUpdated -= OnPinchUpdated;
            if (_panGesture != null) _panGesture.PanUpdated -= OnPanUpdated;
            if (_tapGesture != null) _tapGesture.Tapped -= OnDoubleTapped;

            GestureRecognizers.Clear();

            try
            {
                if (_contentImage is not null)
                {
                    _contentImage.Source = null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                Debug.WriteLine(ex.Source);
            }

            try
            {
                if (Dispatcher is not null)
                {
                    Dispatcher.Dispatch(() =>
                    {
                        try
                        {
                            Content = null;
                        }
                        catch (Exception innerEx)
                        {
                            Debug.WriteLine($"Failed to clear Content on UI thread: {innerEx.Message}");
                        }
                    });
                }
                else
                {
                    try
                    {
                        Content = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to clear Content: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dispatcher failure when clearing Content: {ex.Message}");
            }
        }

#if DEBUG
        ~ImageViewer()
        {
            System.Diagnostics.Debug.WriteLine($"🗑️👻 [Memory] ImageViewer Finalized (Collected).");
        }
#endif

        #endregion
    }
}
