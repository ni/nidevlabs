using System.ComponentModel.Composition;
using System.Windows.Media;
using NationalInstruments.Core;
using NationalInstruments.Shell;
using NationalInstruments.Composition;

namespace ExamplePlugins.ExampleToolWindow
{
    /// <summary>
    /// Factory class for our Selection Tracking Tool
    /// Performs registration with the framework with all of this metadata
    /// And factories new instances on demand
    /// </summary>
    [Export(typeof(IToolWindowType))]
    [ExportMetadata("UniqueID", "{71AB518C-F4EF-447B-9587-676AD5C45243}")]
    [Name("(Example) Selection Tracker")]
    [ExportMetadata("SmallImagePath", "")]
    [ExportMetadata("LargeImagePath", "")]
    [ExportMetadata("DefaultCreationTime", ToolWindowCreationTime.Startup)]
    [ExportMetadata("DefaultCreationMode", ToolWindowCreationMode.Unpinned)]
    [ExportMetadata("DefaultLocation", ToolWindowLocation.Bottom)]
    [ExportMetadata("AssociatedDocumentType", "")]
    [ExportMetadata("Weight", 0.5)]
    [ExportMetadata("ForceOpenPinned", false)]
    public class SelectionTrackingToolWindowType : ToolWindowType
    {
       public override IToolWindowViewModel CreateToolWindow(ToolWindowEditSite editSite)
        {
            return new SelectionTrackingViewModel(editSite);
        }
    }


    public class SelectionTrackingViewModel : IToolWindowViewModel
    {
        private ToolWindowEditSite _editSite;

        public SelectionTrackingViewModel(ToolWindowEditSite site)
        {
            _editSite = site;
        }

        public object Model
        {
            get { return null; }
        }

        public IViewModel ParentViewModel
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public object TryFindResource(object key)
        {
            return null;
        }

        public PlatformVisual View
        {
            get { return new SelectionTrackingToolWindow(_editSite); }
        }

        public string Name
        {
            get
            {
                return "EXAMPLE Selection Tracker";
            }
        }

        public ImageSource SmallImage
        {
            get
            {
                return null;
            }
        }

        public QueryResult<T> QueryService<T>() where T : class
        {
            return new QueryResult<T>();
        }

        public void Initialize(IToolWindowTypeInfo info)
        {
        }

        /// <summary>
        /// Standard INotifyPropertyChanged property changed event.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
