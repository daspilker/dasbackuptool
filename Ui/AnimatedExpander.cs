using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace DasBackupTool.Ui
{
    public class AnimatedExpander : Expander
    {
        public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", typeof(Duration), typeof(AnimatedExpander), new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200))));

        private Size collapsedSize;
        private Size expandedSize;

        public AnimatedExpander()
        {
            Loaded += AnimatedExpanderLoaded;
            Expanded += new RoutedEventHandler(AnimatedExpanderExpanded);
            Collapsed += new RoutedEventHandler(AnimatedExpanderCollapsed);
        }

        public Duration AnimationDuration
        {
            get { return (Duration)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        private void AnimatedExpanderLoaded(object sender, RoutedEventArgs e)
        {
            UIElement content = (UIElement)Content;
            content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            if (IsExpanded)
            {
                expandedSize = new Size(ActualWidth, ActualHeight);
                collapsedSize = new Size(expandedSize.Width - content.DesiredSize.Width, expandedSize.Height - content.DesiredSize.Height);
            }
            else
            {
                collapsedSize = new Size(ActualWidth, ActualHeight);
                expandedSize = new Size(content.DesiredSize.Width + collapsedSize.Width, content.DesiredSize.Height + collapsedSize.Height);
            }
        }

        private void AnimatedExpanderExpanded(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                if (ExpandDirection == ExpandDirection.Left || ExpandDirection == ExpandDirection.Right)
                {
                    BeginAnimation(WidthProperty, new DoubleAnimation(ActualWidth, expandedSize.Width, AnimationDuration));
                }
                else
                {
                    BeginAnimation(HeightProperty, new DoubleAnimation(ActualHeight, expandedSize.Height, AnimationDuration));
                }
            }
        }

        private void AnimatedExpanderCollapsed(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                if (ExpandDirection == ExpandDirection.Left || ExpandDirection == ExpandDirection.Right)
                {
                    BeginAnimation(WidthProperty, new DoubleAnimation(ActualWidth, collapsedSize.Width, AnimationDuration));
                }
                else
                {
                    BeginAnimation(HeightProperty, new DoubleAnimation(ActualHeight, collapsedSize.Height, AnimationDuration));
                }
            }
        }
    }
}
