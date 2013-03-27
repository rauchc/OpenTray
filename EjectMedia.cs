using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenTray
{
    public class cEjectMedia
    {
        private const int INVALID_HANDLE_VALUE = -1;
        private const int GENERIC_READ = unchecked((int)0x80000000);
        private const int GENERIC_WRITE = unchecked((int)0x40000000);
        private const int FILE_SHARE_READ = unchecked((int)0x00000001);
        private const int FILE_SHARE_WRITE = unchecked((int)0x00000002);
        private const int OPEN_EXISTING = unchecked((int)3);
        private const int FSCTL_LOCK_VOLUME = unchecked((int)0x00090018);
        private const int FSCTL_DISMOUNT_VOLUME = unchecked((int)0x00090020);
        private const int IOCTL_STORAGE_EJECT_MEDIA = unchecked((int)0x002D4808);
        private const int IOCTL_STORAGE_MEDIA_REMOVAL = unchecked((int)0x002D4804);
        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeviceIoControl(
        System.IntPtr hDevice,
        int dwIoControlCode,
        byte[] lpInBuffer,
        int nInBufferSize,
        byte[] lpOutBuffer,
        int nOutBufferSize,
        out int lpBytesReturned,
        IntPtr lpOverlapped);

        public cEjectMedia()
        {
            //
            // TODO: Fügen Sie hier die Konstruktorlogik hinzu
            //
        }

        public bool EjectMedia(string sPhysicalDrive)
        {
            bool ok = false;
            bool KarteKannEntnommenWerden = false;
            // Schritt 1: Volume öffnen // Laufwerk anpassen! //
            IntPtr h = CreateFile(@"\\.\" + sPhysicalDrive, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            if (h.ToInt32() == -1)
                return false;
            while (true)
            {
                // Schritt 2: Volume sperren
                for (int i = 0; i < 10; i++)
                {
                    int nout = 0;
                    ok = DeviceIoControl(h, FSCTL_LOCK_VOLUME, null, 0, null, 0, out nout, IntPtr.Zero);
                    if (ok)
                        break;
                    Thread.Sleep(500);
                }
                if (!ok)
                    break;
                // Schritt 3: Volume dismounten
                int xout = 0;
                ok = DeviceIoControl(h, FSCTL_DISMOUNT_VOLUME, null, 0, null, 0, out xout, IntPtr.Zero);
                if (!ok)
                    break;
                // ab hier kann die Karte ohne Datenverlust
                // entnommen werden
                KarteKannEntnommenWerden = true;
                // Schritt 4: Prevent Removal Of Volume
                byte[] flg = new byte[1];
                flg[0] = 0; // 0 = false
                int yout = 0;
                ok = DeviceIoControl(h, IOCTL_STORAGE_MEDIA_REMOVAL, flg, 1, null, 0, out yout, IntPtr.Zero);
                if (!ok)
                    break;
                // Schritt 5: Eject Media
                ok = DeviceIoControl(h, IOCTL_STORAGE_EJECT_MEDIA, null, 0, null, 0, out xout, IntPtr.Zero);
                break;
            }
            // Schritt 6: Close Handle
            ok = CloseHandle(h);
            return KarteKannEntnommenWerden;
        }
    }
}
