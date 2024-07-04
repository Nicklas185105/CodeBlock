using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;

namespace CodeBlock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isUpdating;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        /*
         <xml>
            <test>hello</test>
        </xml>
         */
        
        private void CodeEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdating)
                return;

            isUpdating = true;

            // Get the entire text of the RichTextBox
            var textRange = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd);
            string xmlText = textRange.Text;

            // Clear existing formatting
            textRange.ClearAllProperties();

            // Highlight XML syntax
            HighlightSyntax(xmlText);

            isUpdating = false;
        }

        private void HighlightSyntax(string xmlText)
        {
            var tagPattern = @"(<(/?\w+)(?:[^>]*?)>|\s*<(/?\w+)(?:[^>]*?)>)(?=\s*|$)";
            var attrPattern = @"(\w+)=[""']([^""']+)[""']";
            var elementNamePattern = @"<\/?(\w+)";

            // Find all tags in the text
            var tagMatches = Regex.Matches(xmlText, tagPattern, RegexOptions.Singleline);
            foreach (Match match in tagMatches)
            {
                var tagRange = FindTextRange(CodeEditor.Document.ContentStart, match.Index, match.Length);
                if (tagRange != null)
                {
                    tagRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);

                    // Find all attributes in the tag
                    var attrMatches = Regex.Matches(match.Value, attrPattern);
                    foreach (Match attrMatch in attrMatches)
                    {
                        var attrNameRange = FindTextRange(tagRange.Start, attrMatch.Index, attrMatch.Groups[1].Length);
                        if (attrNameRange != null)
                        {
                            attrNameRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                        }

                        var attrValueRange = FindTextRange(tagRange.Start, attrMatch.Groups[2].Index + 2, attrMatch.Groups[2].Length); // Adjust index to account for =" prefix
                        if (attrValueRange != null)
                        {
                            attrValueRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                        }
                    }
                }
            }

            // Highlight XML element names
            var elementNameMatches = Regex.Matches(xmlText, elementNamePattern, RegexOptions.Singleline);
            foreach (Match match in elementNameMatches)
            {
                var elementNameRange = FindTextRange(CodeEditor.Document.ContentStart, match.Index, match.Length);
                if (elementNameRange != null)
                {
                    elementNameRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                }
            }
        }

        private void HighlightElementNames(string xmlText)
        {
            var elementNamePattern = @"<\/?(\w+)";
            var matches = Regex.Matches(xmlText, elementNamePattern, RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                var elementNameRange = FindTextRange(CodeEditor.Document.ContentStart, match.Index, match.Length);
                if (elementNameRange != null)
                {
                    // Highlight the XML element names
                    elementNameRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                }
            }
        }

        private TextRange FindTextRange(TextPointer start, int offset, int length)
        {
            TextPointer startPointer = GetTextPointerAtOffset(start, offset);
            TextPointer endPointer = GetTextPointerAtOffset(startPointer, length);

            if (startPointer == null || endPointer == null)
                return null;

            return new TextRange(startPointer, endPointer);
        }

        private TextPointer GetTextPointerAtOffset(TextPointer start, int offset)
        {
            TextPointer current = start;
            int currentOffset = 0;

            while (current != null && currentOffset < offset)
            {
                if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    int count = current.GetTextRunLength(LogicalDirection.Forward);
                    if (currentOffset + count >= offset)
                    {
                        return current.GetPositionAtOffset(offset - currentOffset);
                    }

                    currentOffset += count;
                    current = current.GetPositionAtOffset(count);
                }
                else
                {
                    current = current.GetNextContextPosition(LogicalDirection.Forward);
                }
            }

            return current;
        }
    }
}