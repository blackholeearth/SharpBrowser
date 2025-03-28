using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpBrowser
{
    public partial class Form2_testStackpanel : Form
    {
        public Form2_testStackpanel()
        {
            InitializeComponent();
        }

        private void Form2_testStackpanel_Load(object sender, EventArgs e)
        {
            //// Assuming your panel is named 'stackLayout1' and extender is 'stackLayoutExtender1'
            //this.stackLayout1.LayoutExtenderProvider = this.SLayE1;
            //this.stackLayout2.LayoutExtenderProvider = this.SLayE1;

            //or
            //we set them in designer .  PropertiesWindow ->
            // stackLayout1.LayoutExtenderProvider = this.SLayE1;


        }
    }
}
