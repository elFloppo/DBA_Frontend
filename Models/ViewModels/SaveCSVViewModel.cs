using DBA_Frontend.Commands.SaveCSVCommands;
using DBA_Frontend.Controllers;
using DBA_Frontend.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;

namespace DBA_Frontend.Models.ViewModels
{
    public class SaveCSVViewModel : INotifyPropertyChanged
    {
        private IEnumerable<AbonentVM> _abonents { get; }
        private Window _window { get; }
        public SaveCSVViewModel(IEnumerable<AbonentVM> abonents, Window window) 
        {
            _abonents = abonents;
            _window = window;
        }

        private string _folderLocation = null;
        public string FolderLocation
        {
            get { return _folderLocation; }
            set
            {
                _folderLocation = value;
                OnPropertyChanged("FolderLocation");
            }
        }

        private SelectFolderCommand _selectFolderCommand;
        public SelectFolderCommand SelectFolderCommand
        {
            get
            {
                return _selectFolderCommand ??
                    (_selectFolderCommand = new SelectFolderCommand(
                        p =>
                        {
                            var folderBrowserDialog = new FolderBrowserDialog();
                            var dialogResult = folderBrowserDialog.ShowDialog();
                            if (dialogResult == DialogResult.OK)
                                FolderLocation = $"{folderBrowserDialog.SelectedPath}\\";
                        }));
            }
        }

        private SaveCSVCommand _saveCSVCommand;
        public SaveCSVCommand SaveCSVCommand
        {
            get
            {
                return _saveCSVCommand ??
                    (_saveCSVCommand = new SaveCSVCommand(
                        async p =>
                        {
                            if (!Validator.IsSystemPathValid(FolderLocation))
                            {
                                System.Windows.MessageBox.Show($"Ошибка сохранения файла", $"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            var fileName = $"report_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}_{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}.txt";
                            using (var file = File.CreateText($@"{FolderLocation}\{fileName}"))
                            {
                                var csvString = DataToCSVConverter.ConvertDataListToCSV(_abonents);
                                await file.WriteAsync(csvString);
                            }

                            System.Windows.MessageBox.Show($"Файл {fileName}\nуспешно сохранен по пути {FolderLocation}");
                            _window.Close();
                        },
                        p => Validator.IsSystemPathValid(FolderLocation)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
