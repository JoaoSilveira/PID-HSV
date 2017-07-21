using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using PID_HSV.Converter;

namespace PID_HSV.Image
{
    public class ImageBase
    {
        #region Bitmap Structures
        [StructLayout(LayoutKind.Explicit)]
        private struct FileHeader
        {
            [FieldOffset(0)]
            public ushort Type;

            [FieldOffset(2)]
            public uint Size;

            [FieldOffset(6)]
            public ushort Reserved1;

            [FieldOffset(8)]
            public ushort Reserved2;

            [FieldOffset(10)]
            public uint Offset;

            public bool IsBitmap => Type == BitmapType;

            public bool IsPidmap => Type == PidmapType;

            public const ushort BitmapType = 0x4d42;

            public const ushort PidmapType = 0x4d50;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InfoHeader
        {
            [FieldOffset(0)]
            public uint HeaderSize;

            [FieldOffset(4)]
            public uint Width;

            [FieldOffset(8)]
            public uint Height;

            [FieldOffset(12)]
            public ushort Planes;

            [FieldOffset(14)]
            public ushort BitCount;

            [FieldOffset(16)]
            public uint Compression;

            [FieldOffset(20)]
            public uint SizeImage;

            [FieldOffset(24)]
            public uint XPixelsPerMeter;

            [FieldOffset(28)]
            public uint YPixelsPerMeter;

            [FieldOffset(32)]
            public uint ColorsUsed;

            [FieldOffset(36)]
            public uint ColorsImportant;
        }
        #endregion

        private FileHeader _fileHeader;
        private InfoHeader _infoHeader;
        private IntPtr bytes;

        public int Width => (int)_infoHeader.Width;

        public int Height => (int)_infoHeader.Height;

        public int LineStride { get; private set; }

        public IntPtr Buffer => bytes;

        public ImageBase(string filename)
        {
            Load(filename);

            if (_fileHeader.IsBitmap)
                ConvertToHSV();
        }

        public void SavePidmap(string filename)
        {
            Save(filename, e => e);
        }

        public void SaveBitmap(string filename)
        {
            _fileHeader.Type = FileHeader.BitmapType;
            Save(filename, RGBToHSVConverter.ConvertBack);
            _fileHeader.Type = FileHeader.PidmapType;
        }

        private void Save(string filename, Func<byte[], byte[]> convert)
        {
            using (var stream = File.OpenWrite(filename))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(TypeToByte(_fileHeader));
                    bw.BaseStream.Position = 14;
                    bw.Write(TypeToByte(_infoHeader));

                    stream.Position = 54;

                    unsafe
                    {
                        var ptr = (byte*)bytes.ToPointer();
                        var widthInBytes = Width * 3;
                        var padding = new byte[LineStride - widthInBytes];

                        for (var i = 0; i < Height; i++)
                        {
                            var line = ptr + i * LineStride;
                            for (var j = 0; j < widthInBytes; j += 3)
                            {
                                var bytes = convert(new[] { line[j], line[j + 1], line[j + 2] });
                                stream.Write(bytes, 0, bytes.Length);
                            }
                            stream.Write(padding, 0, padding.Length);
                        }
                    }
                }
            }
        }

        private void ConvertToHSV()
        {
            _fileHeader.Type = FileHeader.PidmapType;
            Convert(RGBToHSVConverter.Convert);
        }

        private void Convert(Func<byte[], byte[]> func)
        {
            unsafe
            {
                var pointer = (byte*)bytes.ToPointer();
                var widthInBytes = Width * 3;

                Parallel.For(0, Height, y =>
                {
                    var line = pointer + LineStride * y;

                    for (var x = 0; x < widthInBytes; x += 3)
                    {
                        var bytes = func(new[] { line[x], line[x + 1], line[x + 2] });

                        line[x] = bytes[0];
                        line[x + 1] = bytes[1];
                        line[x + 2] = bytes[2];
                    }
                });
            }
        }

        private void Load(string filename)
        {
            using (var stream = new BinaryReader(File.OpenRead(filename)))
            {
                _fileHeader = ByteToType<FileHeader>(stream);
                stream.BaseStream.Position = 14;
                _infoHeader = ByteToType<InfoHeader>(stream);

                if (!_fileHeader.IsPidmap && !_fileHeader.IsBitmap)
                    throw new ArgumentException("Invalid Image Header");

                LineStride = Width * 3;

                LineStride += (4 - LineStride % 4) % 4;

                stream.BaseStream.Position = _fileHeader.Offset;

                bytes = GCHandle.Alloc(stream.ReadBytes((int)_infoHeader.SizeImage), GCHandleType.Pinned).AddrOfPinnedObject();
            }
        }

        private static T ByteToType<T>(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }

        private static byte[] TypeToByte<T>(T type)
        {
            unsafe
            {
                var size = sizeof(InfoHeader);

                var bytes = new byte[size];
                var arr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(type, arr, true);
                Marshal.Copy(arr, bytes, 0, size);

                return bytes;
            }
        }
    }
}