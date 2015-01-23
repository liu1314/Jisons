
using System.Windows;
using System.Windows.Media.Animation;

namespace TVM.WPF.Library.Animation
{
    public enum Equations
    {
        Default,
        Linear,
        QuadEaseOut,
        QuadEaseIn,
        ExpoEaseOut,
        ExpoEaseIn,
        CubicEaseOut,
        CubicEaseIn,
        QuartEaseOut,
        QuartEaseIn,
        QuintEaseOut,
        QuintEaseIn,
        CircEaseOut,
        CircEaseIn,
        SineEaseOut,
        SineEaseIn,
        ElasticEaseOut,
        ElasticEaseIn,
        BounceEaseOut,
        BounceEaseIn,
        BackEaseOut,
        BackEaseIn
    }
    /// <summary>
    /// Animates the value of a double property between two target values using 
    /// Robert Penner's easing equations for interpolation over a specified Duration.
    /// </summary>
    /// <example>
    /// <code>
    /// // C#
    /// PennerDoubleAnimation anim = new PennerDoubleAnimation();
    /// anim.Type = PennerDoubleAnimation.Equations.Linear;
    /// anim.From = 1;
    /// anim.To = 0;
    /// myControl.BeginAnimation( OpacityProperty, anim );
    /// 
    /// // XAML
    /// <Storyboard x:Key="AnimateXamlRect">
    ///  <animation:PennerDoubleAnimation 
    ///    Storyboard.TargetName="myControl" 
    ///    Storyboard.TargetProperty="(Canvas.Left)"
    ///    From="0" 
    ///    To="600" 
    ///    Equations="BackEaseOut" 
    ///    Duration="00:00:05" />
    /// </Storyboard>
    /// 
    /// <Control.Triggers>
    ///   <EventTrigger RoutedEvent="FrameworkElement.Loaded">
    ///     <BeginStoryboard Storyboard="{StaticResource AnimateXamlRect}"/>
    ///   </EventTrigger>
    /// </Control.Triggers>
    /// </code>
    /// </example>
    public class PennerDoubleAnimation : DoubleAnimation
    {
        #region Fields
        private Interpolator _interpolator;
        private delegate double Interpolator(double from, double to, double t);
        #endregion
        #region Dependency Properties
        public static readonly DependencyProperty EquationProperty =
            DependencyProperty.Register(
                "Equation", typeof(Equations), typeof(PennerDoubleAnimation),
                new PropertyMetadata(new PropertyChangedCallback(OnEquationChanged)));
        #endregion
        #region Constructors
        public PennerDoubleAnimation()
        {
        }
        public PennerDoubleAnimation(Equations type, double from, double to)
        {
            Equation = type;
            From = from;
            To = to;
        }
        public PennerDoubleAnimation(Equations type, double from, double to, Duration duration)
        {
            Equation = type;
            From = from;
            To = to;
            Duration = duration;
        }
        #endregion
        #region Abstract Member Implementations
        protected override double GetCurrentValueCore(double startValue, double targetValue, AnimationClock clock)
        {
            try
            {
                double actualProgress = clock.CurrentProgress.HasValue ? clock.CurrentProgress.Value : 1.0;
                double from = From.HasValue ? From.Value : startValue;
                double to = To.HasValue ? To.Value : targetValue;
                return _interpolator(from, to, actualProgress);
            }
            catch
            {
                return From.HasValue ? From.Value : startValue;
            }
        }
        private static Interpolator GetInterpolator(Equations equation)
        {
            switch (equation)
            {
                case Equations.Linear:
                    return PennerInterpolator.Linear;
                case Equations.QuadEaseOut:
                    return PennerInterpolator.QuadEaseOut;
                case Equations.QuadEaseIn:
                    return PennerInterpolator.QuadEaseIn;
                case Equations.ExpoEaseOut:
                    return PennerInterpolator.ExpoEaseOut;
                case Equations.ExpoEaseIn:
                    return PennerInterpolator.ExpoEaseIn;
                case Equations.CubicEaseOut:
                    return PennerInterpolator.CubicEaseOut;
                case Equations.CubicEaseIn:
                    return PennerInterpolator.CubicEaseIn;
                case Equations.QuartEaseOut:
                    return PennerInterpolator.QuarticEaseOut;
                case Equations.QuartEaseIn:
                    return PennerInterpolator.QuarticEaseIn;
                case Equations.QuintEaseOut:
                    return PennerInterpolator.QuinticEaseOut;
                case Equations.QuintEaseIn:
                    return PennerInterpolator.QuinticEaseIn;
                case Equations.CircEaseOut:
                    return PennerInterpolator.CircularEaseOut;
                case Equations.CircEaseIn:
                    return PennerInterpolator.CircularEaseIn;
                case Equations.SineEaseOut:
                    return PennerInterpolator.SineEaseOut;
                case Equations.SineEaseIn:
                    return PennerInterpolator.SineEaseIn;
                case Equations.ElasticEaseOut:
                    return PennerInterpolator.ElasticEaseOut;
                case Equations.ElasticEaseIn:
                    return PennerInterpolator.ElasticEaseIn;
                case Equations.BounceEaseOut:
                    return PennerInterpolator.BounceEaseOut;
                case Equations.BounceEaseIn:
                    return PennerInterpolator.BounceEaseIn;

                case Equations.BackEaseOut:
                    return PennerInterpolator.BackEaseOut;
                case Equations.BackEaseIn:
                    return PennerInterpolator.BackEaseIn;
            }
            return PennerInterpolator.Linear;
        }
        protected override Freezable CreateInstanceCore()
        {
            return new PennerDoubleAnimation();
        }
        #endregion
        #region Properties
        /// <summary>
        /// The easing equation to use.
        /// </summary>
        public Equations Equation
        {
            get { return (Equations)GetValue(EquationProperty); }
            set { SetValue(EquationProperty, value); }
        }
        #endregion
        /// <summary>
        /// Enumeration of all easing equations.
        /// </summary>
        private static void OnEquationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PennerDoubleAnimation anim = d as PennerDoubleAnimation;
            anim._interpolator = GetInterpolator((Equations)e.NewValue);
        }
    }
}