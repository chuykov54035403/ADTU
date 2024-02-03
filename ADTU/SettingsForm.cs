using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADTU
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        //Событие загрузки формы
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            pathTextBox.Text = Properties.Settings.Default.pathToUsers;
        }

        //Кнопка "Сохранить"
        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.pathToUsers = pathTextBox.Text;
                Properties.Settings.Default.Save();
                this.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
    }
}
