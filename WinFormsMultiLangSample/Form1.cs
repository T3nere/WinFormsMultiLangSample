using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsMultiLangSample.Resources;

namespace WinFormsMultiLangSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // CurrentUICultureに応じて自動的にen/jaが切り替わる
            BtnJapanese.Text = Strings.BtnJapanese;
            BtnEnglish.Text = Strings.BtnEnglish;
            label1.Text = Strings.label1_text;
            label2.Text = Strings.label2_text;
        }

        private void BtnEnglish_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Language = "en";
            Properties.Settings.Default.Save();

            MessageBox.Show(Strings.MsgRestartRequired);
        }

        private void BtnJapanese_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Language = "ja";
            Properties.Settings.Default.Save();

            MessageBox.Show(Strings.MsgRestartRequired);
        }
    }
}
