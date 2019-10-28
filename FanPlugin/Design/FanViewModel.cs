using System;
using NationalInstruments.Core;
using NationalInstruments.MocCommon.Design;
using NationalInstruments.Design;
using NationalInstruments.SourceModel;
using FanControl;
using FanPlugin.SourceModel;

namespace FanPlugin.Design
{
    /// <summary>
    /// The View Model is primarily responsible for funneling any model properties on to the view (which it also creates).
    /// It is also responsible for the generation of custom command content.
    /// </summary>
    public class FanViewModel : VisualViewModel
    {
        public FanViewModel(Element model)
            : base(model)
        {
        }

        public override object CreateView()
        {
            var fan = new Fan();
            fan.ValueChanged += OnValueChanged;
            return fan;
        }

        /// <summary>
        /// IMPORTANT!
        /// This is the place where we hook into the event that we had to expose on our control. The purpose of this event handler
        /// is to notice interactive changes on the view, and update the model accordingly. "Transactions" are required for the changes
        /// to the model to be persisted (and available for operations like XAML-generation).
        /// </summary>
        private void OnValueChanged(object sender, EventArgs eventArgs)
        {
            var fan = sender as Fan;
            if (fan.Value != ((FanModel)Model).Value)
            {
                using (var transaction = Model.TransactionManager.BeginTransaction("Update value", TransactionPurpose.UserNonDirtying))
                {
                    ((FanModel)Model).Value = fan.Value;
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// This method updates the view in response to property changes on the model
        /// <summary>
        protected override void SetProperty(NationalInstruments.DynamicProperties.PropertySymbol identifier, object value)
        {
            Fan fan = ProxiedElement as Fan;
            switch (identifier.Name)
            {
                case FanModel.FanSpeedName:
                    fan.FanSpeed = (FanSpeed)value;
                    break;
                case FanModel.ValueName:
                    fan.Value = (bool)value;
                    break;
                default:
                    base.SetProperty(identifier, value);
                    break;
            }
        }

        #region Command Content

        /// <summary>
        /// This method will create the custom ribbon content to represent the FanSpeed editor
        /// </summary>
        public override void CreateCommandContent(ICommandPresentationContext context)
        {
            base.CreateCommandContent(context);
            using (context.AddConfigurationPaneContent())
            {
                using (context.AddGroup(MocCommonCommands.ControlDesignCommand))
                {
                    context.Add(FanCommands.SetFanSpeedCommand);
                }
            }
        }

        #endregion
    }
}
