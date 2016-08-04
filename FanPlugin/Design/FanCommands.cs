using System.Collections.Generic;
using System.Linq;
using FanControl;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.MocCommon.Design;
using NationalInstruments.Controls.Shell;
using NationalInstruments.SourceModel;
using NationalInstruments.Shell;
using FanDemo.SourceModel;

namespace FanDemo
{
    // This class contains the commands which the command bar (and popup menu) utilizes for the fan control. Specifically, there are 
    // four commands. One to represent the "menu open" command, and serve as the parent in a popup menu, and three others to
    // represent each of the FanSpeed settings
    //
    // The 'ShellSelectionRelayCommand' will be the most common type of command one would try to use for using in their ribbon
    // editors. I strongly recommend familiarizing yourself with its interface
    public static class FanCommands
    {
        // The "top level" command
        public static readonly ICommandEx SetFanSpeedCommand = new ShellSelectionRelayCommand(ChangeFanSpeedCombo, CanChangeFanSpeedCombo)
        {
            LabelTitle = "Fan Speed",
            UniqueId = "NI.Fan:SetFanSpeedCommand",
            UIType = UITypeForCommand.ComboBox
        };

        public static readonly ICommandEx SetFanSpeedLowCommand = new ShellSelectionRelayCommand(ChangeTemperatureScale, CanChangeFanSpeed, FanSpeedLowAttach)
        {
            LabelTitle = "Low",
            UniqueId = "NI.FanCommands:SetFanSpeedLowCommand",
            PopupMenuParent = SetFanSpeedCommand // this will inform the system that this command should be parented under the given command in a popup menu
        };

        private static void FanSpeedLowAttach(PlatformVisual visual, ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            // This allows us to pass specific information to a common handler used by multiple commands. Otherwise, we would have redundant code for
            // three different commands
            if (!parameter.QueryService<ControlCommandParameter>().Any())
            {
                parameter.AttachService(new ControlCommandParameter() { Parameter = FanSpeed.Low });
            }
        }

        public static readonly ICommandEx SetFanSpeedMediumCommand = new ShellSelectionRelayCommand(ChangeTemperatureScale, CanChangeFanSpeed, FanSpeedMediumAttach)
        {
            LabelTitle = "Medium",
            UniqueId = "NI.FanCommands:SetFanSpeedMediumCommand",
            PopupMenuParent = SetFanSpeedCommand
        };

        private static void FanSpeedMediumAttach(PlatformVisual visual, ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            if (!parameter.QueryService<ControlCommandParameter>().Any())
            {
                parameter.AttachService(new ControlCommandParameter() { Parameter = FanSpeed.Medium });
            }
        }

        public static readonly ICommandEx SetFanSpeedHighCommand = new ShellSelectionRelayCommand(ChangeTemperatureScale, CanChangeFanSpeed, FanSpeedHighAttach)
        {
            LabelTitle = "High",
            UniqueId = "NI.FanCommands:SetFanSpeedHighCommand",
            PopupMenuParent = SetFanSpeedCommand 
        };

        private static void FanSpeedHighAttach(PlatformVisual visual, ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            if (!parameter.QueryService<ControlCommandParameter>().Any())
            {
                parameter.AttachService(new ControlCommandParameter() { Parameter = FanSpeed.High });
            }
        }

        private static void ChangeFanSpeedCombo(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            var choiceParameter = parameter.QueryService<ChoiceCommandParameter>().FirstOrDefault();
            if (choiceParameter != null)
            {
                var model = ((FanModel) ((FanViewModel) selection.First()).Model);
                using (var transaction = model.TransactionManager.BeginTransaction("Change Spped", TransactionPurpose.User))
                {
                    model.FanSpeed = (FanSpeed)choiceParameter.Chosen;
                    transaction.Commit();
                }
            }
        }

        private static bool CanChangeFanSpeedCombo(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            var choiceParameter = parameter.QueryService<ChoiceCommandParameter>().FirstOrDefault();
            if (choiceParameter != null)
            {
                if (choiceParameter.Choices == null || !choiceParameter.Choices.Any())
                {
                    var choices = new ChoiceCommandParameterChoice[]
                    {
                        new ChoiceCommandParameterChoice(FanSpeed.Low, PlatformImage.NullImage, "Low"),
                        new ChoiceCommandParameterChoice(FanSpeed.Medium, PlatformImage.NullImage, "Medium"),
                        new ChoiceCommandParameterChoice(FanSpeed.High, PlatformImage.NullImage, "High"),
                    };
                    choiceParameter.Choices = choices;
                }
                choiceParameter.Chosen = ((FanModel) ((FanViewModel) selection.First()).Model).FanSpeed;
                return true;
            }
            return false;
        }

        // This will be called when almost any change occurs in the editor (while the command bar with this command is active), and should be used to update
        // the state of the command bar/popup menu editors themselves. For instance, in certain conditions you might want a particular editor disabled, or 
        // visually indicate that it is the option currently selected. For control commands, the two parameters we tend to be primarily interested in are
        // the first two. The 'parameter' parameter is used to pass specific information to the handler to act on. The 'selection' parameter contains
        // all of the models that are part of the current selection (assuming the necessary implementation exists to support multi-select for this model type, 
        // otherwise, this enumerable will contain one element when a single element is selected). Refrain from changing any model state here.
        private static bool CanChangeFanSpeed(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            var controlParameter = parameter.QueryService<ControlCommandParameter>().FirstOrDefault();
            if (controlParameter == null)
            {
                return false;
            }

            var scale = (FanSpeed)controlParameter.Parameter; // We can do this confidently based on our implementations of the AttachToVisualHandlers above
            var selectedValue = selection.GetSelectedValue((FanModel model) => model.FanSpeed);
            ((ICheckableCommandParameter)parameter).IsChecked = selectedValue == scale; // This is required to have the ribbon editor or popup menu visually indicate which option is current selected
            return true; // If false the command associated with this callback would be disabled. For our commands, we never want a disabled option (assuming we got this far)
        }

        // This will be called in response to a user making a selection in the command bar/popup menu, which is were we want to update the model. We have provided
        // some extensions that encapsulate this process which involves transactions.
        private static void ChangeTemperatureScale(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            var fanSpeed = (FanSpeed)parameter.QueryService<ControlCommandParameter>().FirstOrDefault().Parameter;
            var models = selection.GetSelectedModels<FanModel>();
            ITransactionManagerExtensions.TransactOnElements(models, "Change fan speed", model => model.FanSpeed = fanSpeed); // useful extension to easily perform updates on models through transactions
        }
    }
}
