using System;
using System.Drawing;
using System.Windows.Forms;

namespace Algebra
{
    public partial class inputbox : Form
    {
        public inputbox()
        {
            InitializeComponent();
            FormClosing += Inputbox_FormClosing;
        }

        private void Inputbox_FormClosing(object sender, FormClosingEventArgs e)
        {
            main.Enabled = true;

        }

        public  Project project;
        public  Main main;
        public  string name;
        float r;
        private void OK_Click(object sender, EventArgs e)
        {
            if (float.TryParse(value.Text, out r))
            {
                addcercle();
            }

        }

        void addcercle()
        {
            Cercle cer = new Cercle();
            cer.Properties.Name = project.GenerateName();
            cer.O = name;
            cer.fix = true;
            cer.value = r;
            project.cercles.Add(cer);
            Close();
            main.Enabled = true;
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            main.Enabled = true;
            Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
