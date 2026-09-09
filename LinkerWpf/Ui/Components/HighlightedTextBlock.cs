using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Linker.Theme;

namespace Linker.Ui.Components
{
    public class HighlightedTextBlock : TextBlock
    {
        public new static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HighlightedTextBlock),
                new PropertyMetadata(string.Empty, OnTextOrQueryChanged));

        public static readonly DependencyProperty QueryProperty =
            DependencyProperty.Register(nameof(Query), typeof(string), typeof(HighlightedTextBlock),
                new PropertyMetadata(string.Empty, OnTextOrQueryChanged));

        public new string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Query
        {
            get => (string)GetValue(QueryProperty);
            set => SetValue(QueryProperty, value);
        }

        private static readonly Brush HighlightBackground = new SolidColorBrush(NordColors.Nord13);
        private static readonly Brush HighlightForeground = new SolidColorBrush(NordColors.Nord0);

        private static void OnTextOrQueryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HighlightedTextBlock)d).Rebuild();
        }

        private void Rebuild()
        {
            var text = Text ?? string.Empty;
            var query = Query;
            Inlines.Clear();

            if (string.IsNullOrEmpty(query))
            {
                Inlines.Add(new Run(text));
                return;
            }

            var index = 0;
            while (index < text.Length)
            {
                var matchIndex = text.IndexOf(query, index, StringComparison.OrdinalIgnoreCase);
                if (matchIndex < 0)
                {
                    Inlines.Add(new Run(text.Substring(index)));
                    break;
                }
                if (matchIndex > index)
                {
                    Inlines.Add(new Run(text.Substring(index, matchIndex - index)));
                }
                var run = new Run(text.Substring(matchIndex, query.Length))
                {
                    Background = HighlightBackground,
                    Foreground = HighlightForeground,
                    FontWeight = FontWeights.Bold,
                };
                Inlines.Add(run);
                index = matchIndex + query.Length;
            }
        }
    }
}
