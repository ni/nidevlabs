using System.Windows;
using System.Windows.Controls;
using NationalInstruments.SourceModel;
using NationalInstruments.Shell;
using ExamplePlugins.ExampleDocument.Model;

namespace ExamplePlugins.ExampleDocument.Shell
{
    /// <summary>
    /// The view for our document.  This uses a WPF text box to enable simple
    /// text editing
    /// </summary>
    public class TextDocumentEditorView : DocumentEditControl
    {
        // The UI element which enables editing of the text
        private TextBox _editBox;

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        public TextDocumentEditorView()
        {
        }

        /// <summary>
        /// Called when the text box looses focus.  We are going to transact the changes to the
        /// text here.
        /// </summary>
        /// <param name="sender">object which lost focus</param>
        /// <param name="routedEventArgs">standard event arguments</param>
        private void HandleLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var definition = EditorInfo.Document.Envoy.ReferenceDefinition as TextDocumentDefinition;
            // Update the text from within a transaction
            using (var transaction = definition.TransactionManager.BeginTransaction("Set Text", TransactionPurpose.User))
            {
                definition.Text = _editBox.Text;
                transaction.Commit();
            }
        }

        /// <summary>
        /// Called when our UI is being initialized.  Here we are adding the text box to
        /// enable editing of the text
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // We only setup the edit box once.  OnApplyTemplate can be called more than once.
            if (_editBox == null)
            {
                var definition = EditorInfo.Document.Envoy.ReferenceDefinition as TextDocumentDefinition;
                _editBox = new TextBox()
                {
                    Focusable = true,
                    AcceptsReturn = true,
                    AcceptsTab = true,
                    Text = definition.Text,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = System.Windows.VerticalAlignment.Stretch
                };
                // We are going to transact the text changes when the editor looses focus.  This is simple
                // but not very robust
                _editBox.LostFocus += HandleLostFocus;
                AddChild(_editBox);
            }
        }
    }
}
