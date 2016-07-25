using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NationalInstruments.Core;
using NationalInstruments.Design;
using NationalInstruments.SourceModel;
using NationalInstruments.Shell;

namespace ExamplePlugins.ExampleToolWindow
{
    /// <summary>
    /// Our Selecting tracking tool window.  This is the visual object that the user can see
    /// It is a WPF User Control whose content is defined by the XAML markup
    /// </summary>
    public partial class SelectionTrackingToolWindow : UserControl
    {
        /// <summary>
        /// A WPF Dependency Property which is used to hold the information about the selected elements to be
        /// displayed in the selection grid
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems", typeof(ObservableCollection<SelectionDisplayInfo>), typeof(SelectionTrackingToolWindow));

        /// <summary>
        /// Our composition host.  The composition host provides access to the rest of the system.  There is one
        /// host per instance of our editor and should never be stored globally.  It manages all of the plug-ins
        /// in the system and well as provide access to the framework objects
        /// </summary>
        //private ICompositionHost _host;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="host">The composition host this window is associated with</param>
        public SelectionTrackingToolWindow(ToolWindowEditSite site)
        {
            // Set up a few things
            //_host = host;
            SelectedItems = new ObservableCollection<SelectionDisplayInfo>();
            DataContext = this;

            // This creates our View.  Standard WPF thing.
            InitializeComponent();

            // Here we are getting the "User interface" framework object and registering
            // for selection and document change notifications
            site.RootEditSite.ActiveDocumentChanged += HandleActiveDocumentChanged;
            site.RootEditSite.SelectedChanged += HandleSelectedChanged;

            // Make sure we are reflecting the currently active document and
            // selection
            UpdateDocumentName(site.RootEditSite.ActiveDocument);
            UpdateSelection(site.RootEditSite.ActiveSelection);
        }

        /// <summary>
        /// Gets and Sets the selected items list. Do this in the standard WPF way
        /// </summary>
        public ObservableCollection<SelectionDisplayInfo> SelectedItems
        {
            get { return GetValue(SelectedItemsProperty) as ObservableCollection<SelectionDisplayInfo>; }
            set {  SetValue(SelectedItemsProperty, value);}
        }

        /// <summary>
        /// Called when the active selection changes
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event data</param>
        private void HandleSelectedChanged(object sender, SelectedChangedEventArgs args)
        {
            UpdateSelection(args.SelectedItems);
        }

        /// <summary>
        /// Called when the active document changes
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event data</param>
        private void HandleActiveDocumentChanged(object sender, ActiveDocumentChangedEventArgs args)
        {
            UpdateDocumentName(args.ActiveDocument);
        }

        /// <summary>
        /// Updates the data we are displaying about the selection when it changes
        /// </summary>
        /// <param name="selection">The new selection</param>
        private void UpdateSelection(IEnumerable<IViewModel> selection)
        {
            // Clear the current selection and handle the null case
            SelectedItems.Clear();
            if (selection == null)
            {
                return;
            }
            foreach (var item in selection)
            {
                // Gather some information about each selected item
                var model = item.Model as Element;
                var viewModel = item as NodeViewModel;
                if (model != null)
                {
                    var info = new SelectionDisplayInfo()
                    {
                        Name = model.Documentation.Name,
                        Type = model.SpecificKind
                    };
                    if (viewModel != null)
                    {
                        var image = RenderData.NineGridToImage(viewModel.IconData, new Size(16,16));
                        info.Image = image;
                    }
                    SelectedItems.Add(info);
                }
            }
        }

        /// <summary>
        /// Updates the displayed information about the active document
        /// </summary>
        /// <param name="document">The new active document, may be null when there is not active document</param>
        private void UpdateDocumentName(Document document)
        {
            if (document == null)
            {
                _documentNameControl.Text = "No Selection";
                _documentTypeControl.Text = "No Selection";
                return;
            }
            var name = document.DocumentName;
            var type = document.Envoy.ModelDefinitionType.ToString();

            _documentNameControl.Text = name;
            _documentTypeControl.Text = type;
        }
    }

    /// <summary>
    /// A helper object which holds the data to display for each selected item
    /// </summary>
    public class SelectionDisplayInfo
    {
        /// <summary>
        /// A picture of the item if it has one
        /// </summary>
        public ImageSource Image { get; set; }

        /// <summary>
        /// The name of the selected item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Specific Kind (type) of the item
        /// </summary>
        public string Type { get; set; }
    }
}
