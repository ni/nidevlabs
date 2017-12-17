using NationalInstruments.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExamplePlugins.ExampleDiagram.Shell
{
    /// <summary>
    /// Wrapper for the ListBox that simplifies the control by only allowing single selection.
    /// </summary>
    public partial class ListBox : UserControl, ICommandSourceEx
    {
        public static readonly DependencyProperty SelectedItemProperty = 
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(ListBox), new FrameworkPropertyMetadata(default(object), OnSelectedItemChanged));

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public ItemCollection Items => PART_ListBox.Items;

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listBox = (ListBox)d;
            listBox.PART_ListBox.SelectedItem = e.NewValue;
        }

        #region ICommandSourceEx implementation

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
        #endregion

        public ListBox()
        {
            InitializeComponent();
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem != PART_ListBox.SelectedItem)
            {
                SelectedItem = PART_ListBox.SelectedItem;
            }
        }
    }
}
