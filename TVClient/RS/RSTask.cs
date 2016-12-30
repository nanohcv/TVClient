using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVClient.RS
{
    public class RSTask
    {
        public bool IsChecked { get; set; }
        public bool CheckboxEnabled { get; private set; }
        public string Cmd { get; private set; }
        public string LocalizedName { get; private set; }

        public RSTask(bool checkBoxEnabled, string cmd, string localizedName)
        {
            IsChecked = false;
            CheckboxEnabled = checkBoxEnabled;
            Cmd = cmd;
            LocalizedName = localizedName;
        }
    }
}
