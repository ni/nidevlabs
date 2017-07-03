using NationalInstruments.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ExamplePlugins.ExampleDiagram.Design
{
    public class ListBoxItem : System.Windows.Controls.ListBoxItem, ICommandSourceEx
    {
        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ListBoxItem), new FrameworkPropertyMetadata(default(ICommand), OnCommandChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ListBoxItem), new PropertyMetadata(default(object), OnCommandParameterChanged));

        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(ListBoxItem), new PropertyMetadata(default(IInputElement)));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        
        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public ListBoxItem()
        {
            SetBindings();
        }

        private void SetBindings()
        {
            SetBinding(ContentProperty, new Binding("Command.LabelTitle") { Source = this });
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            ExecuteCommand();
            e.Handled = true;
            base.OnMouseDoubleClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                ExecuteCommand();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        internal void ExecuteCommand()
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
