using System;
using System.Diagnostics;
using Windows.Storage.Streams;

namespace Ble.Test.Utility
{
    public static class Utilities
    {
        public static string BtleAddress2String(ulong ulongData)
        {
            return Ulong2String(ulongData, 6);
        }

        public static string Ulong2String(ulong ulongData, byte len = 8)
        {
            var byteArray = BitConverter.GetBytes(ulongData);

            // A Bluetooth–enabled device address is a unique, 48–bit (6 bytes) address
            Array.Resize(ref byteArray, len);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteArray);
            }

            return ByteArray2String(byteArray);
        }

        public static string ByteArray2String(byte[] byteArray, bool isLen = false)
        {
            var message = String.Empty;
            if (isLen)
            {
                message = "[" + byteArray.Length + "] => ";
            }

            for (var i = 0; i < byteArray.Length; i++)
            {
                message += byteArray[i].ToString("X2");

                if (i != byteArray.Length - 1)
                {
                    message += "-";
                }
            }

            return message;
        }

        internal static void VibratePhone()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
            {
                //VibrationDevice vibrationDevice = VibrationDevice.GetDefault();
                //vibrationDevice.Vibrate(TimeSpan.FromSeconds(seconds));
            }
        }

        public static byte[] ReadBuffer(IBuffer buffer)
        {
            byte[] bytes = new byte[buffer.Length];
            DataReader.FromBuffer(buffer).ReadBytes(bytes);
            return bytes;
        }

        public static byte[] Xor(byte[] data1, byte[] data2, int len)
        {
            Debug.Assert(data1 != null && data1.Length >= len);
            Debug.Assert(data2 != null && data2.Length >= len);

            byte[] response = new byte[len];

            for (int i = 0; i < len; i++)
            {
                response[i] = (byte)(data1[i] ^ data2[i]);
            }

            return response;
        }

        public static string ReadIBuffer2Str(IBuffer buffer, bool isLen = false)
        {
            byte[] bytes = ReadBuffer(buffer);
            string strData = ByteArray2String(bytes, isLen);
            return strData;
        }

    }
}

