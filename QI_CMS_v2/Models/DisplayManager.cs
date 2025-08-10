using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QI_CMS_v2.Models
{

    public class DisplayInfo
    {
        public string Name { get; set; }
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;
        public double PixelWidth { get; set; } = 0;
        public double PixelHeight { get; set; } = 0;
        public bool IsPrimary { get; set; }

    }

    public class DisplayManager
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME = 32;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmColor;
            public int dmDuplex;
            public int dmYResolution;
            public int dmTTOption;
            public int dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmNup;
            public uint dmDisplayFrequency;
            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        private const int DESKTOPHORZRES = 118;
        private const int DESKTOPVERTRES = 117;
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;

        private void GetDisplayArea()
        {
            MinLeft = 0;
            MinTop = 0;
            MaxRight = 0;
            MaxBottom = 0;
            foreach (var display in Displays)
            {
                MinLeft = Math.Min(MinLeft, display.X);
                MinTop = Math.Min(MinTop, display.Y);
                MaxRight = Math.Max(MaxRight, display.X + display.Width);
                MaxBottom = Math.Max(MaxBottom, display.Y + display.Height);
            }
        }

        private DisplayManager()
        {
            GetCurrentDisplays();
        }

        private static DisplayManager? _instance;
        public static DisplayManager Instance { get => _instance ??= new DisplayManager(); }

        public List<DisplayInfo> Displays { get; set; }

        public double MinLeft { get; set; } = 0;
        public double MinTop { get; set; } = 0;
        public double MaxRight { get; set; } = 0;
        public double MaxBottom { get; set; } = 0;
        public Rect TotalDisplayArea { get => new Rect(new Point(MinLeft, MinTop), new Point(MaxRight, MaxBottom)); }
        public void GetCurrentDisplays()
        {
            List<DisplayInfo> displays = new();
            uint devNum = 0;

            while (true)
            {
                DISPLAY_DEVICE displayDevice = new();
                displayDevice.cb = Marshal.SizeOf(displayDevice);

                if (!EnumDisplayDevices(null, devNum, ref displayDevice, 0))
                    break;

                DEVMODE devMode = new();
                devMode.dmSize = (ushort)Marshal.SizeOf(devMode);

                if (!EnumDisplaySettings(displayDevice.DeviceName, -1, ref devMode))
                {
                    devNum++;
                    continue;
                }

                IntPtr hdc = CreateDC(displayDevice.DeviceName, displayDevice.DeviceName, null, IntPtr.Zero);
                if (hdc == IntPtr.Zero)
                {
                    Console.WriteLine($"[오류] {displayDevice.DeviceName}에 대한 HDC 생성 실패");
                    devNum++;
                    continue;
                }

                double posX = devMode.dmPositionX;
                double posY = devMode.dmPositionY;

                int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                int dpiY = GetDeviceCaps(hdc, LOGPIXELSY);

                // 디스플레이 장치 해상도 불러오기
                int realWidth = GetDeviceCaps(hdc, DESKTOPHORZRES);
                int realHeight = GetDeviceCaps(hdc, DESKTOPVERTRES);

                DeleteDC(hdc);

                // OS에 설정된 디스플레이 해상도(배율 적용) 계산
                double scaleFactorX = dpiX / 96.0;
                double scaleFactorY = dpiY / 96.0;
                int scaledWidth = (int)(realWidth / scaleFactorX);
                int scaledHeight = (int)(realHeight / scaleFactorY);

                displays.Add(new DisplayInfo
                {
                    Name = displayDevice.DeviceName,
                    X = posX,
                    Y = posY,
                    Width = scaledWidth,
                    Height = scaledHeight,
                    PixelWidth = realWidth,
                    PixelHeight = realHeight,
                    IsPrimary = (displayDevice.StateFlags & 0x4) != 0
                });

                devNum++;
            }

            Displays = displays;
            GetDisplayArea();
        }
        

    }

}
