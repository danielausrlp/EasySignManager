using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasySignManager
{
    public partial class Form5 : Form
    {


        private Form1 parent;

        public Form5()
        {
            InitializeComponent();
        }


        public Form5(Form1 prt)
        {
            InitializeComponent();
            parent = prt;
            this.Text = "Einstellungen von " + parent.listBox1.SelectedItem.ToString();
        }

        private void Form5_Closed(object sender, EventArgs e)
        {
            parent.Enabled = true;
        }


    }
}
