using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Linker.Util
{
    public static class SmoothScroll
    {
        private const double WheelDeltaPerRow = 120;
        private const double TimeConstantMs = 60;

        private static readonly HashSet<ScrollState> Active = new HashSet<ScrollState>();

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(SmoothScroll),
                new PropertyMetadata(false, OnEnabledChanged));

        private static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached("State", typeof(ScrollState), typeof(SmoothScroll),
                new PropertyMetadata(null));

        public static void SetEnabled(DependencyObject element, bool value) => element.SetValue(EnabledProperty, value);
        public static bool GetEnabled(DependencyObject element) => (bool)element.GetValue(EnabledProperty);

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement element)) return;
            if (!(d is ScrollViewer || d is ListBox || d is TextBox)) return;

            element.PreviewMouseWheel -= OnWheel;
            if ((bool)e.NewValue) element.PreviewMouseWheel += OnWheel;
        }

        private static void OnWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = sender as ScrollViewer ?? FindDescendant<ScrollViewer>(sender as DependencyObject);
            if (viewer == null || e.Delta == 0 || viewer.ScrollableHeight <= 0) return;

            e.Handled = true;
            var state = GetState(viewer);
            var rowHeight = GetRowHeight(viewer, state);
            var step = -e.Delta / WheelDeltaPerRow * rowHeight;

            if (!state.Running)
            {
                state.Current = viewer.VerticalOffset;
                state.Target = state.Current;
                state.LastTicks = Stopwatch.GetTimestamp();
                state.Running = true;
                Active.Add(state);
                CompositionTarget.Rendering += OnRendering;
            }

            state.Target = Clamp(state.Target + step, viewer.ScrollableHeight);
        }

        private static void OnRendering(object sender, EventArgs e)
        {
            var now = Stopwatch.GetTimestamp();
            foreach (var state in Active.ToArray())
            {
                var viewer = state.Viewer;
                state.Target = Clamp(state.Target, viewer.ScrollableHeight);

                var elapsedMs = (now - state.LastTicks) * 1000.0 / Stopwatch.Frequency;
                state.LastTicks = now;
                var alpha = 1 - Math.Exp(-elapsedMs / TimeConstantMs);
                var next = state.Current + (state.Target - state.Current) * alpha;

                if (Math.Abs(state.Target - next) < 0.5)
                {
                    viewer.ScrollToVerticalOffset(state.Target);
                    state.Running = false;
                    Active.Remove(state);
                }
                else
                {
                    state.Current = next;
                    viewer.ScrollToVerticalOffset(next);
                }
            }

            if (Active.Count == 0) CompositionTarget.Rendering -= OnRendering;
        }

        private static ScrollState GetState(ScrollViewer viewer)
        {
            var state = (ScrollState)viewer.GetValue(StateProperty);
            if (state != null) return state;
            state = new ScrollState { Viewer = viewer };
            viewer.SetValue(StateProperty, state);
            return state;
        }

        private static double GetRowHeight(ScrollViewer viewer, ScrollState state)
        {
            if (state.RowHeight > 0) return state.RowHeight;
            var row = FindRow(viewer, typeof(ListBoxItem)) ?? FindRow(viewer, typeof(ContentPresenter));
            var height = row?.ActualHeight ?? 0;
            if (height <= 0 || height > viewer.ViewportHeight / 2) height = 48;
            state.RowHeight = height;
            return height;
        }

        private static FrameworkElement FindRow(DependencyObject root, Type type)
        {
            if (root == null) return null;
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (type.IsInstanceOfType(child) && child is FrameworkElement element && element.ActualHeight > 0)
                    return element;
                var nested = FindRow(child, type);
                if (nested != null) return nested;
            }
            return null;
        }

        private static double Clamp(double value, double max)
        {
            return Math.Max(0, Math.Min(max, value));
        }

        private static T FindDescendant<T>(DependencyObject root) where T : DependencyObject
        {
            if (root == null) return null;
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match) return match;
                var nested = FindDescendant<T>(child);
                if (nested != null) return nested;
            }
            return null;
        }

        private sealed class ScrollState
        {
            public ScrollViewer Viewer;
            public double Current;
            public double Target;
            public double RowHeight;
            public long LastTicks;
            public bool Running;
        }
    }
}
