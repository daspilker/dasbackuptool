using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace DasBackupTool.Ui
{
    public class CommandButton:Button
    {
        public CommandButton()
        {
            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = new System.Windows.PropertyPath("Command.Text");
            SetBinding(ContentProperty, binding);
        }
    }
}
