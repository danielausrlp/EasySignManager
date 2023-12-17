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
    public partial class Form4 : Form
    {

        Form2 parent;

        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }

        private void Form4_Closed(object sender, EventArgs e)
        {
            parent.Enabled = true;
        }

        public Form4(Form2 prt)
        {
            InitializeComponent();
            parent = prt;
        }

    }
}
