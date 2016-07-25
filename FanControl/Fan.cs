using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FanControl
{
    // This control is simply a toggle button skinned to look like a fan, that gives the user
    // the option to set the speed of the fan to three different settings

    public class Fan : ToggleButton
    {
        static Fan()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Fan), new FrameworkPropertyMetadata(typeof(Fan)));
        }

        private FrameworkElement _fanPart;
        private Storyboard _storyboard;
        private DoubleAnimation _rotateAnimation;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _fanPart = GetTemplateChild("PART_Fan") as FrameworkElement;
            _fanPart.RenderTransformOrigin = new Point(.5, .5);
            PreviewMouseLeftButtonDown += FanPartOnMouseLeftButtonDown;
            Click += OnClick;
            _storyboard = new Storyboard();
            Resources.Add("Storyboard", _storyboard);
            _rotateAnimation = new DoubleAnimation { From = 0, To = 360 };
            Storyboard.SetTarget(_rotateAnimation, _fanPart);
            Storyboard.SetTargetProperty(_rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            _storyboard.Children.Add(_rotateAnimation);
            UpdateFanSpeedAnimation(FanSpeed);
        }

        private void UpdateFanSpeedAnimation(FanSpeed speed)
        {
            ((Storyboard)Resources["Storyboard"]).Stop();
            switch (speed)
            {
                case FanSpeed.Low:
                    _rotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
                    _rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
                    break;
                case FanSpeed.Medium:
                    _rotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(.1));
                    _rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
                    break;
                case FanSpeed.High:
                    _rotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(.01));
                    _rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
                    break;
            }

            if (Value)
            {
                ((Storyboard)Resources["Storyboard"]).Begin();
            }
        }

        private void OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            _fanPart.RenderTransform = new RotateTransform();
            if (IsChecked == true)
            {
                Value = true;
                ((Storyboard)Resources["Storyboard"]).Begin();
            }
            else
            {
                Value = false;
                ((Storyboard)Resources["Storyboard"]).Stop();
            }
        }

        private void FanPartOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ((Storyboard)Resources["Storyboard"]).Stop();
            _fanPart.RenderTransform = new ScaleTransform(.9, .9, .5, .5);
        }

        public FanSpeed FanSpeed
        {
            get { return (FanSpeed)GetValue(FanSpeedProperty); }
            set { SetValue(FanSpeedProperty, value); }
        }

        public static readonly DependencyProperty FanSpeedProperty =
            DependencyProperty.Register("FanSpeed", typeof(FanSpeed), typeof(Fan), new PropertyMetadata(FanSpeed.Low, new PropertyChangedCallback(OnFanSpeedChanged)));

        private static void OnFanSpeedChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var fan = sender as Fan;
            fan.UpdateFanSpeedAnimation(fan.FanSpeed);
        }

        public bool Value
        {
            get { return (bool)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(bool), typeof(Fan), new PropertyMetadata(false, new PropertyChangedCallback(OnValueChanged)));

        #region LV-required code (for now)

        private static void OnValueChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var fan = sender as Fan;
            if (fan != null && fan._valueChanged != null)
            {
                fan._valueChanged(fan, EventArgs.Empty);
            }
        }
        
        private EventHandler _valueChanged;
        public event EventHandler ValueChanged
        {
            add { _valueChanged += value; }
            remove { _valueChanged -= value; }
        }

        #endregion
    }

    public enum FanSpeed
    {
        Low,
        Medium,
        High
    }
}
