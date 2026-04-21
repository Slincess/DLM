using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DLM
{
    public partial class LinkButton : UserControl
    {

        Button linkOpenButton;
        private Buttoninfo _thisButtonInfo;
        public Buttoninfo thisButtonInfo
        {
            get { return _thisButtonInfo; }
            set
            {
                _thisButtonInfo = value;
                _thisButtonInfo.isOnlineindicator = isOnlineindicator;
            }
        }

        public LinkButton()
        {
            InitializeComponent();
            linkOpenButton = LinkOpenButton;
        }

        

        public Button GetEditButton() { return (Button)EditButton; }
        public Button GetLinkButton() { return (Button)(LinkOpenButton); }

        public void Edit(string link,string name)
        {
            linkOpenButton.Tag = link;
            thisButtonInfo.Links = link;
            if (!string.IsNullOrEmpty(name))
            {
                linkOpenButton.Content = name;
                thisButtonInfo.Name = name;
            }
        }
        public void Edit(string link)
        {
            linkOpenButton.Tag = link;
            thisButtonInfo.Links = link;
        }

    }
}
