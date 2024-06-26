using System.Globalization;

namespace gunit {
    public static class Arr {
        internal static byte[] GetBytes(ref byte[] Arr, int len, bool reverse = false) {
            var res = new byte[len];
            Array.Copy(Arr, res, len);
            Array.Copy(Arr, len, Arr, 0, Arr.Length - len);
            Array.Resize(ref Arr, Arr.Length - len);
            if (reverse)
                Array.Reverse(res, 0, res.Length);
            return res;
        }

        internal static byte GetByte(ref byte[] Arr) {
            return GetBytes(ref Arr, 1)[0];
        }

        internal static uint GetUInt(ref byte[] Arr, bool reverse = false) {
            var res = GetBytes(ref Arr, 2, reverse);
            return BitConverter.ToUInt16(res, 0);
        }

        internal static int GetInt(ref byte[] Arr, bool reverse = false) {
            var res = GetBytes(ref Arr, 2, reverse);
            return BitConverter.ToInt16(res, 0);
        }

        internal static uint GetUInt32(ref byte[] Arr, bool reverse = false) {
            var res = GetBytes(ref Arr, 4, reverse);
            return BitConverter.ToUInt32(res, 0);
        }

        internal static int GetInt32(ref byte[] Arr, bool reverse = false) {
            var res = GetBytes(ref Arr, 4, reverse);
            return BitConverter.ToInt32(res, 0);
        }

        internal static float GetFloat(ref byte[] Arr, bool reverse = false) {
            var res = GetBytes(ref Arr, 4, reverse);
            return BitConverter.ToSingle(res, 0);
        }

        internal static double GetDouble(ref byte[] Arr, bool reverse = false) {
            var res = GetBytes(ref Arr, 8, reverse);
            return BitConverter.ToDouble(res, 0);
        }

        internal static string GetString(ref byte[] Arr, int len) {
            return gUtils.BytesToString(GetBytes(ref Arr, len));
        }

        internal static T Pop<T>(ref T[] Arr) {
            return Splice(ref Arr, Arr.Length - 1, 1)[0];
        }

        internal static void Push<T>(ref T[] Arr, T Value) {
            Push(ref Arr, new T[] { Value });
        }

        internal static void Push<T>(ref T[] Arr, T[] Value) {
            Array.Resize(ref Arr, Arr.Length + Value.Length);
            for (int i = 0; i < Value.Length; i++)
                Arr[Arr.Length - Value.Length + i] = Value[i];
        }

        internal static T[] Splice<T>(ref T[] Arr, int index) {
            return Splice(ref Arr, index, Arr.Length - index);
        }

        internal static T[] Splice<T>(ref T[] Arr, int index, int length) {
            var res = new T[length];
            Array.Copy(Arr, index, res, 0, length);
            Array.Copy(Arr, index + length, Arr, index, Arr.Length - index - length);
            Array.Resize(ref Arr, Arr.Length - length);
            return res;
        }

        internal static T[] Slice<T>(T[] Arr, int index, int length) {
            var res = new T[length];
            Array.Copy(Arr, index, res, 0, length);
            return res;
        }

        internal static T Shift<T>(ref T[] Arr) {
            return Splice(ref Arr, 0, 1)[0];
        }

        internal static void Unshift<T>(ref T[] Arr, T Value) {
            Unshift(ref Arr, new T[] { Value });
        }

        internal static void Unshift<T>(ref T[] Arr, T[] Value) {
            Array.Resize(ref Arr, Arr.Length + Value.Length);
            Array.Copy(Arr, 0, Arr, Value.Length, Arr.Length - Value.Length);
            Array.Copy(Value, 0, Arr, 0, Value.Length);
        }
    }

    public static partial class Extensions {
        public static string BinToHex(this byte[] arr) {
            var result = "";
            for (var i = 0; i < arr.Length; i++)
                result += arr[i].ToString("X2");
            return result;
        }

        public static byte[] HexToBin(this string s) {
            var result = new byte[s.Length / 2];
            for (var i = 0; i < (s.Length / 2); i++)
                result[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber);
            return result;
        }

        public static double? Median(this double[] arr) {
            if (arr.Length == 0) { return null; }

            Array.Sort(arr);
            return
              (arr[arr.Length / 2] + arr[(arr.Length - 1) / 2]) / 2;
        }
    }
}
