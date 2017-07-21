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
        private readonly Window ownerWindow;

        private ImageBase _imgBase;
        private Bitmap _image;

        public Bitmap Image
        {
            get => _image;
            private set => SetProperty(ref _image, value);
        }

        public MainWindowController(Window ownerWindow)
        {
            this.ownerWindow = ownerWindow;
            OpenCommand = new Command(Open);
            SaveCommand = new Command(Save);
            ExportCommand = new Command(Export);
            ExitCommand = new Command(Exit);

            openFileDialog = new OpenFileDialog
            {
                Title = "Open",
                Filter = "Bitmap (*.bmp)|*.bmp|Pidmap (*.pmp)|*.pmp|Both (*.bmp;*.pmp)|*bmp;*.pmp",
                FilterIndex = 1,
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
            var result = openFileDialog.ShowDialog(ownerWindow);

            if (!result.Value) return;

            _imgBase = new ImageBase(openFileDialog.FileName);
            Image = _imgBase.ToBitmap();

            openFileDialog.FileName = "";
        }

        private void Save()
        {
            saveFileDialog.Title = "Save";
            saveFileDialog.Filter = "Bitmap (*.bmp)|*.bmp";
            saveFileDialog.FilterIndex = 1;

            var result = saveFileDialog.ShowDialog(ownerWindow);

            if (!result.Value) return;

            _imgBase.SaveBitmap(saveFileDialog.FileName);

            saveFileDialog.FileName = "";
        }

        private void Export()
        {
            saveFileDialog.Title = "Export";
            saveFileDialog.Filter = "Pidmap (*.pmp)|*.pmp";
            saveFileDialog.FilterIndex = 1;

            var result = saveFileDialog.ShowDialog(ownerWindow);

            if (!result.Value) return;

            _imgBase.SavePidmap(saveFileDialog.FileName);

            saveFileDialog.FileName = "";
        }

        private void Exit()
        {
            var result = MessageBox.Show(ownerWindow, "Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }
    }
}
