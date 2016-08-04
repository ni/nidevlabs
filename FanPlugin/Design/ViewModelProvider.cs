using System.ComponentModel.Composition;
using NationalInstruments.VI.Design;
using NationalInstruments.Shell;
using FanPlugin.SourceModel;

namespace FanPlugin.Design
{
    /// <summary>
    /// The purpose of this class is to provide the IDE an explicit association between your Model and
    /// ViewModel types.
    /// </summary>
    [Export(typeof(IProvideViewModels))]
    [ExportProvideViewModels(typeof(VIPanelControl))]
    public class DemoViewModelProvider : ViewModelProvider
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DemoViewModelProvider()
        {
            AddSupportedModel<FanModel>(e => new FanViewModel(e));
        }
    }
}
