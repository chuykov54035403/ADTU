using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ADTU
{
    public partial class MainForm : Form
    {
        //Переменная, которая хранит значение "Все ли пользователи выбраны"
        bool IsAllItemsSelected = false;

        //Переменная, которая хранит количество пользователей
        int itemsCount;

        //Переменная, которая хранит количество выбранных пользователей
        int checkedItemsCount;

        //Добавляем переменную для работы ProgressBar
        int progressBarValue = 0;

        //Путь до папки Users
        string pathToUsers;

        void PrintTheProcessedUser(string processedUser)
        {
            guna2HtmlLabel4.Text = processedUser;
        }
        void PrintTheFail(string fail)
        {
            MessageBox.Show(fail);
        }

        //Инициализация формы
        public MainForm()
        {
            InitializeComponent();
        }

        //Событие загрузки формы
        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadForm();
        }

        //Кнопка "Добавить"
        private void addDataBaseButton_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show($"Вы уверены, что хотите добавить базу данных:\n\"{dataBaseTextBox.Text}\" пользователям:\n{DisplaySelectedUsers()}?", "Внимание!!!", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    ResetProgressBar();
                    ClearEmptyRowsInTheDatabase();
                    //Перебор выбранных пользователей, создание экземпляра класса User и добавление базы данных из dataBaseTextBox 
                    foreach (var userName in userCheckedListBox.CheckedItems)
                    {
                        User user = new User(userName.ToString(), pathToUsers);
                        user.RegisterUserHandler(PrintTheProcessedUser);
                        user.RegisterFailHandler(PrintTheFail);
                        user.AddDataBase(dataBaseTextBox.Text);
                        progressBar.Value = progressBarValue;
                        progressBarValue++;
                    }
                    guna2HtmlLabel4.Text = "Готово";
                }
                catch (Exception exception)
                {
                    guna2HtmlLabel4.Text = "Ошибка";
                    MessageBox.Show(exception.ToString());
                }
            }
            else if (dialogResult == DialogResult.No)
            {

            }
        }

        //Кнопка "Удалить"
        private void deleteDataBaseButton_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show($"Вы уверены, что хотите удалить базу данных:\n\"{dataBaseTextBox.Text}\"\nу пользователей:\n{DisplaySelectedUsers()}?", "Внимание!!!", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    ClearEmptyRowsInTheDatabase();
                    //Перебор выбранных пользователей, создание экземпляра класса User и добавление базы данных из dataBaseTextBox 
                    foreach (var userName in userCheckedListBox.CheckedItems)
                    {
                        User user = new User(userName.ToString(), pathToUsers);
                        user.RegisterUserHandler(PrintTheProcessedUser);
                        user.RegisterFailHandler(PrintTheFail);
                        user.DeleteDataBase(dataBaseTextBox.Text);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                }
            }
            else if (dialogResult == DialogResult.No)
            {

            }
        }

        //Кнопка "Выделить/Снять все"
        private void selectAllDeselectAllButton_Click(object sender, EventArgs e)
        {
            if (IsAllItemsSelected == false)
            {
                for (int i = 0; i < itemsCount; i++)
                {
                    userCheckedListBox.SetItemChecked(i, true);
                }
                IsAllItemsSelected = true;
                selectAllDeselectAllButton.Text = "Снять все";
            }
            else 
            {
                for (int i = 0; i < itemsCount; i++)
                {
                    userCheckedListBox.SetItemChecked(i, false);
                }
                IsAllItemsSelected = false;
                selectAllDeselectAllButton.Text = "Выделить все";
            }
        }

        //Кнопка "О программе"
        private void openInfoFormButton_Click(object sender, EventArgs e)
        {
            InfoForm infoForm = new InfoForm();
            infoForm.ShowDialog();
        }

        //Событие, которое убирает выделение на выбранном пользователе в userCheckedListBox 
        private void userCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            userCheckedListBox.ClearSelected();
        }

        //Кнопка "Вставить текст из буфера обмена"
        private void getTextFromTheClipboardButton_Click(object sender, EventArgs e)
        {
            dataBaseTextBox.Text = Clipboard.GetText();
        }

        //Кнопка "Восстановить"
        private void restoreBackupFileButtonButton_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show($"Вы уверены, что хотите восстановить файл резервной копии пользователям:\n{DisplaySelectedUsers()}?", "Внимание!!!", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    ResetProgressBar();
                    foreach (var userName in userCheckedListBox.CheckedItems)
                    {
                        User user = new User(userName.ToString(), pathToUsers);
                        user.RegisterUserHandler(PrintTheProcessedUser);
                        user.RegisterFailHandler(PrintTheFail);
                        user.RestoreBackupFile();
                        progressBar.Value = progressBarValue;
                        progressBarValue++;
                    }
                    guna2HtmlLabel4.Text = "Готово";
                }
                catch (Exception exception)
                {
                    guna2HtmlLabel4.Text = "Ошибка";
                    MessageBox.Show(exception.ToString());
                }
            }
            else if (dialogResult == DialogResult.No)
            {

            }
        }

        //Кнопка "Посмотреть базы данных"
        private void readDataBaseButton_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var userName in userCheckedListBox.CheckedItems)
                {
                    User user = new User(userName.ToString(), pathToUsers);
                    user.RegisterFailHandler(PrintTheFail);
                    MessageBox.Show(user.ReadDataBase(), userName.ToString());
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        //Кнопка "Настройки"
        private void settingsButton_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
            LoadForm();
        }

        //Метод удаления пустых строк из dataBaseTextBox
        void ClearEmptyRowsInTheDatabase()
        {
            //Очистка пустых строк в начале TextBox
            dataBaseTextBox.Text = dataBaseTextBox.Text.TrimStart();
            //Очистка пустых строк в конце TextBox
            dataBaseTextBox.Text = dataBaseTextBox.Text.TrimEnd();
        }

        //Метод вывода выбранных пользователей
        string DisplaySelectedUsers()
        {
            string userList = "";
            foreach (var userName in userCheckedListBox.CheckedItems)
            {
                userList += $"{userName.ToString()}\n";
            }
            return userList;
        }

        //Метод сброса ProgressBar
        void ResetProgressBar()
        {
            //Очищаем ProgressBar
            progressBarValue = 1;
            progressBar.Value = progressBarValue;
            //Количество выделенных пользователей
            checkedItemsCount = userCheckedListBox.CheckedItems.Count;
            //Максимум для ProgressBar1
            progressBar.Maximum = checkedItemsCount;
        }
        
        //Метод для загрузки формы
        void LoadForm()
        {
            try
            {
                userCheckedListBox.Items.Clear();
                pathToUsers = Properties.Settings.Default.pathToUsers;
                //Перебор и добавление пользователей в список
                DirectoryInfo dir = new DirectoryInfo(pathToUsers);
                foreach (var item in dir.GetDirectories())
                {
                    if (item.Name != "Default" & item.Name != "All Users" & item.Name != "Public" & item.Name != "Default User" & item.Name != "Все пользователи")
                    {
                        this.userCheckedListBox.Items.Add(item.Name);
                    }
                }
                itemsCount = userCheckedListBox.Items.Count;
                guna2HtmlLabel4.Text = "";
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
    }
}
