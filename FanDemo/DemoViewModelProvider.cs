using System.ComponentModel.Composition;
using FanDemo;
using NationalInstruments.VI.Design;
using NationalInstruments.Shell;

namespace FanControl
{
    // The purpose of this class is to provide the IDE an explicit association between your Model and
    // ViewModel types.
    [Export(typeof(IProvideViewModels))]
    [ExportProvideViewModels(typeof(VIPanelControl))]
    public class DemoViewModelProvider : ViewModelProvider
    {
        public DemoViewModelProvider()
        {
            AddSupportedModel<FanModel>(e => new FanViewModel(e));
        }
    }
}
