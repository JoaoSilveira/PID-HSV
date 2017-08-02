using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using PID_HSV.Image;
using PID_HSV.Util;

namespace PID_HSV
{
    public class MainWindowController : ModelBase
    {
        public Command OpenCommand { get; }
        public Command SaveCommand { get; }
        public Command ExportCommand { get; }
        public Command ExitCommand { get; }

        private readonly OpenFileDialog openFileDialog;
        private readonly SaveFileDialog saveFileDialog;
        public Window OwnerWindow { get; set; }

        private ImageBase _imgBase;
        private Bitmap _image;
        private HSVOptions _hsvOptions;

        public Bitmap Image
        {
            get => _image;
            private set => SetProperty(ref _image, value);
        }

        public HSVOptions HsvOptions
        {
            get => _hsvOptions;
            set => SetProperty(ref _hsvOptions, value);
        }

        public MainWindowController()
        {
            OpenCommand = new Command(Open);
            SaveCommand = new Command(Save, () => _imgBase != null);
            ExportCommand = new Command(Export, () => _imgBase != null);
            ExitCommand = new Command(Exit);

            openFileDialog = new OpenFileDialog
            {
                Title = "Open",
                Filter = "Bitmap (*.bmp)|*.bmp|Pidmap (*.pmp)|*.pmp|Both (*.bmp;*.pmp)|*bmp;*.pmp",
                FilterIndex = 3,
                RestoreDirectory = true
            };

            saveFileDialog = new SaveFileDialog
            {
                RestoreDirectory = true,
                AddExtension = true
            };
        }

        private void Open()
        {
            var result = openFileDialog.ShowDialog(OwnerWindow);

            if (!result.Value) return;

            try
            {
                _imgBase = new ImageBase(openFileDialog.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show(OwnerWindow,
                    "Um erro ocorreu durante a leitura do arquivo, caso a imagem for um BMP verifique se a mesma é true color (24 bits)",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            HsvOptions = new HSVOptions();
            RegisterHsvEvent();
            Image = _imgBase.ToBitmap();

            openFileDialog.FileName = "";
            SaveCommand.RaiseCanExecuteChanged();
            ExportCommand.RaiseCanExecuteChanged();
        }

        private void Save()
        {
            saveFileDialog.Title = "Save";
            saveFileDialog.Filter = "Bitmap (*.bmp)|*.bmp";
            saveFileDialog.FilterIndex = 1;

            var result = saveFileDialog.ShowDialog(OwnerWindow);

            if (!result.Value) return;

            _imgBase.SaveBitmap(saveFileDialog.FileName, HsvOptions);

            saveFileDialog.FileName = "";
        }

        private void Export()
        {
            saveFileDialog.Title = "Export";
            saveFileDialog.Filter = "Pidmap (*.pmp)|*.pmp";
            saveFileDialog.FilterIndex = 1;

            var result = saveFileDialog.ShowDialog(OwnerWindow);

            if (!result.Value) return;

            _imgBase.SavePidmap(saveFileDialog.FileName, HsvOptions);

            saveFileDialog.FileName = "";
        }

        private void Exit()
        {
            var result = MessageBox.Show(OwnerWindow, "Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void RegisterHsvEvent()
        {
            _hsvOptions.PropertyChanged += (e, o) =>
            {
                Image.CopyBitmap(_imgBase, HsvOptions);
                RaisePropertyChanged(nameof(Image));
            };
        }
    }
}
