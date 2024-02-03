using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ADTU
{
    public delegate void UserHandler(string message);
    public delegate void FailHandler(string message);
    internal class User
    {
        public readonly string userName;
        //Путь до папки 1CEStart пользователя
        private string pathTo1CEStart;
        //Путь до каталога с резервной копией
        private string pathToBackupDirectory;
        //Путь до файла ibases.v8i
        private string pathToListDataBases;
        public User(string userName, string pathToUsersDirectory)
        {
            this.userName = userName;
            SetPaths(pathToUsersDirectory);
        }

        UserHandler processedUser;
        FailHandler throwFail;
        public void RegisterUserHandler(UserHandler del)
        {
            processedUser = del;
        }

        public void RegisterFailHandler(FailHandler del)
        {
            throwFail = del;
        }

        //Метод чтения файла ibases.v8i
        public string ReadDataBase()
        {
            string connectionList = "";
            try
            {
                var connected = File.ReadLines(this.pathToListDataBases).SelectMany(n => n.StartsWith("Connect") ? new[] { n } : new string[] { }).ToList();
                foreach (string connect in connected)
                {
                    connectionList += $"{connect}\n\n";
                }
                return connectionList;
            }
            catch (Exception exception)
            {
                throwFail?.Invoke(exception.ToString());
                return connectionList;
            }
        }

        //Метод установки путей для пользователя
        private void SetPaths(string pathToUsersDirectory)
        {
            //Путь до каталога 1CEStart
            pathTo1CEStart = $"{pathToUsersDirectory}\\{this.userName}\\AppData\\Roaming\\1C\\1CEStart";
            //Путь до каталога с резервной копией
            pathToBackupDirectory = $"{pathTo1CEStart}\\ibases_backup";
            //Путь до файла ibases.v8i
            pathToListDataBases = $"{pathTo1CEStart}\\ibases.v8i";
        }
        
        //Метод резервного копирования файла ibases.v8i
        private void BackupFileBeforeChanges()
        {
            //Создаем папку ibases_backup если ее нет и копируем в нее файл ibases.v8i перед внесением изменений.
            if (!Directory.Exists(pathToBackupDirectory)) Directory.CreateDirectory(pathToBackupDirectory);
            File.Copy(pathToListDataBases, $"{pathToBackupDirectory}\\ibases.v8i", overwrite: true);
        }

        //Метод восстановления файла резервной копии
        public void RestoreBackupFile()
        {
            try 
            {
                File.Copy($"{pathToBackupDirectory}\\ibases.v8i", pathToListDataBases, overwrite: true);
                processedUser?.Invoke(this.userName);
            }
            catch (Exception exception)
            {
                throwFail?.Invoke(exception.ToString());
            }
        }

        //Метод добавления новой базы данных
        public void AddDataBase(string dataBase)
        {
            try
            {
                //Делаем резервную копию файла ibases.v8i
                BackupFileBeforeChanges();
                //Читаем файл ibases.v8i и записываем в него новую базу данных
                StreamWriter ibases = new StreamWriter(pathToListDataBases, append: true);
                ibases.WriteLine(dataBase);
                ibases.Close();
                processedUser?.Invoke(this.userName);
            }
            catch(Exception exception)
            {
                throwFail?.Invoke(exception.ToString());
            }
        }

        //Метод удаления базы данных
        public void DeleteDataBase(string dataBase)
        {
            try
            {
                //Делаем резервную копию файла ibases.v8i
                BackupFileBeforeChanges();
                var dataBaseAfterConvert = Regex.Split(dataBase, "\r\n|\r|\n");
                //Выясняем количество строк в TextBox(количество строк которые необходимо удалить)
                int countRowsToDelete = dataBaseAfterConvert.Length;
                //Создаем спискок, построчно читаем и записываем в него файл ibases.v8i
                List<string> ibasesStringList = new List<string>(File.ReadAllLines(pathToListDataBases));
                //Находим интересующую нас базу по первой строчке из TextBox(Первой строчкой идет название базы данных в файле ibases.v8i)
                int targetIndex = ibasesStringList.FindIndex(x => x.Equals(dataBaseAfterConvert[0]));
                //Создаем список и записываем в него индексы удаляемых строк
                List<int> indexesToBeDeleted = new List<int>();
                for (int i = 0; i < countRowsToDelete; i++)
                {
                    indexesToBeDeleted.Add(targetIndex);
                    targetIndex++;
                }
                //Перебираем список удаляемых строк, сортируем и удаляем строки индекс которым равен индексам списка indexesToBeDeleted
                foreach (var i in indexesToBeDeleted.OrderByDescending(e => e))
                {
                    ibasesStringList.RemoveAt(i);
                }
                //Перезаписываем файл ibases.v8i из списка ibasesStringList
                File.WriteAllLines(pathToListDataBases, ibasesStringList);
                processedUser?.Invoke(this.userName);
            }
            catch (Exception exception)
            {
                throwFail?.Invoke(exception.ToString());
            }
        }
    }
}
