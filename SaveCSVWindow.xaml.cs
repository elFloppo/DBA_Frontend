using DBA_Frontend.Controllers;
using DBA_Frontend.Models.ViewModels;
using DBA_Frontend.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace DBA_Frontend
{
    public partial class SaveCSVWindow : Window
    {
        public IEnumerable<AbonentVM> abonents;
        public string _filePath = null;

        public SaveCSVWindow()
        {
            InitializeComponent();
        }

        private void OpenFolderBrowserDialogBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            var dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                FileLocationTextBox.Text = $"{folderBrowserDialog.SelectedPath}\\";
                SaveCSVBtn.IsEnabled = Validator.IsSystemPathValid(FileLocationTextBox.Text);
            }
        }

        private async void SaveCSVBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Validator.IsSystemPathValid(FileLocationTextBox.Text))
            {
                System.Windows.MessageBox.Show($"Ошибка сохранения файла", $"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var fileName = $"report_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}_{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}.txt";
            using (var file = File.CreateText($@"{FileLocationTextBox.Text}\{fileName}"))
            {
                var csvString = DataToCSVConverter.ConvertDataListToCSV(abonents);
                await file.WriteAsync(csvString);
            }

            System.Windows.MessageBox.Show($"Файл {fileName}\nуспешно сохранен по пути {FileLocationTextBox.Text}");
            Close();
        }

        private void FileLocationTextBox_TextChanged(object sender, TextChangedEventArgs e) { SaveCSVBtn.IsEnabled = Validator.IsSystemPathValid(FileLocationTextBox.Text); }
    }
}
