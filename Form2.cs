using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LANTran
{
    public partial class Form2 : Form
    {
       
        //private Talker talker;
        //private RichTextBox textBox;
        public Form2()
        {
            InitializeComponent();
            
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            Form1 f1=(Form1)this.Owner;
            f1.sethost(textBox1.Text);
            f1.Connect();
           
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
