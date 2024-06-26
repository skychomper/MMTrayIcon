namespace gunit {
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.IO;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Diagnostics;
    using System.Net;
    using System.IO.Compression;
    using Microsoft.Win32;
    using System.Collections.Generic;
    using System.Linq;
    using System.Dynamic;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using System.Net.Sockets;
    using System.Data;
    using System.Management;

    public partial class gUtils {
        public string LastAppErrorString;
        public enum LogRotate { None, Monthly };

        private static DateTime tz(DateTime dt) {
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FromSerializedString("Russian Standard Time;180;(GMT+03:00) Moscow, St. Petersburg, Volgograd (RTZ 2);Russia TZ 2 Standard Time;Russia TZ 2 Daylight Time;[01:01:0001;12:31:2010;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];][01:01:2011;12:31:2011;60;[0;02:00:00;3;5;0;];[0;00:00:00;1;1;6;];][01:01:2014;12:31:2014;60;[0;00:00:00;1;1;3;];[0;02:00:00;10;5;0;];];"));
        }

        public static string IncludeSlash(string path) {
            path = path.Default();

            if ((path.Contains("/")) && (path.Last() != '/'))
                path += "/";
            else
              if ((path.Contains("\\")) && (path.Last() != '\\'))
                path += "\\";

            return path;
        }

        private static void LogRotateFile(string logfile, int num) {
            if (File.Exists(logfile + "." + num)) {
                LogRotateFile(logfile, num + 1);

                if (File.Exists(logfile + "." + (num + 1)) == false)
                    File.Move(logfile + "." + num, logfile + "." + (num + 1));
            }
        }

        public static void AppendLog(string logfile, string s, bool appendtime = true, LogRotate rotate = LogRotate.None, string prefix = null) {
            byte[] buff;
            try {
                s = s.Replace("\r", "");
                prefix = prefix.Default();

                var lines = gUtils.Split(s, "\n");
                s = "";
                foreach (var q in lines)
                    s += (appendtime ? DateTime.Now.ToString("yyy-MM-dd HH:mm:ss") : "") + " " + (prefix != "" ? prefix + " " : "") + q + "\r\n";

                buff = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(s));

                //rotate

                if (rotate != LogRotate.None) {
                    if (File.Exists(logfile)) {
                        var dt = new FileInfo(logfile).LastWriteTime;

                        if ((rotate == LogRotate.Monthly) && (DateTime.Now.Month != dt.Month)) {
                            LogRotateFile(logfile, 1);

                            if (File.Exists(logfile + ".1") == false)
                                File.Move(logfile, logfile + ".1");
                        }
                    }
                }

                //write

                using (FileStream fs = new FileStream(logfile, FileMode.Append)) {
                    fs.Write(buff, 0, buff.Length);
                    fs.Close();
                }
            }
            catch { }
        }

        public static void AppenLogTextBox(TextBox textBox, string s, bool appendtime = true) {
            s = s.Replace("\r", "");

            var lines = gUtils.Split(s, "\n");
            s = "";
            foreach (var q in lines)
                s += (appendtime ? DateTime.Now.ToString("yyy-MM-dd HH:mm:ss") : "") + " " + q + "\r\n";

            textBox.AppendText(s);

            Application.DoEvents();
        }

        public static string BytesToString(byte[] arr, string srcenc) {
            if (srcenc == "")
                srcenc = "windows-1251";
            return Encoding.GetEncoding(srcenc).GetString(arr);
        }

        public static string BytesToString(byte[] arr) {
            return Encoding.GetEncoding("windows-1251").GetString(arr);
        }

        public static byte[] StringToBytes(string s, string srcenc) {
            if (srcenc == "")
                srcenc = "windows-1251";
            return Encoding.GetEncoding(srcenc).GetBytes(s);
        }

        public static byte[] StringToBytes(string s) {
            return Encoding.GetEncoding("windows-1251").GetBytes(s);
        }

        public static string BytesToStringUnicode(byte[] arr) {
            return Encoding.UTF8.GetString(arr);
        }

        public static byte[] StringToBytesUnicode(string s) {
            return Encoding.UTF8.GetBytes(s);
        }

        public static string BinToHex(byte[] arr) {
            return arr.BinToHex();
        }

        public static byte[] HexToBin(string s) {
            return s.HexToBin();
        }

        public static string[] Split(string Str, string Delimeter) {
            if (Str.Default() == "")
                return new string[0];

            string[] del = new string[1];
            del[0] = Delimeter;
            return Str.Split(del, StringSplitOptions.None);
        }

        public static string Join(string Delimeter, string[] Arr) {
            if ((Arr == null) || (Arr.Length == 0))
                return "";

            return String.Join(Delimeter, Arr);
        }

        public static string DecodePassword(string pass, byte salt) {
            int i1;
            byte[] buff;
            if (pass == "") { return ""; }

            buff = HexToBin(pass);

            for (i1 = 0; i1 <= buff.Length - 1; i1++) {
                buff[i1] = (byte)(buff[i1] ^ salt);
            }

            return BytesToString(buff, "");
        }

        public static string EncodePassword(string pass, byte salt) {
            int i1;
            byte[] buff;
            if (pass == "") { return ""; };

            buff = StringToBytes(pass, "");

            for (i1 = 0; i1 <= buff.Length - 1; i1++) {
                buff[i1] = (byte)(buff[i1] ^ salt);
            }

            return BinToHex(buff);
        }

        public static string Fetch(ref string Input, string Delimeter, bool del) {
            string result;
            int lpos;
            if (Input == "" || Input == null) { return ""; }

            if (Delimeter.Length < 1) {
                return Input;
            }

            lpos = Input.IndexOf(Delimeter);

            if (lpos == -1) {
                result = Input;
                if (del) {
                    Input = "";
                }
            }
            else {
                result = Input.Substring(0, lpos);
                if (del) {
                    //this is faster than delete(ainput, 1, lpos + length(adelim) - 1);
                    Input = Input.Substring(lpos + Delimeter.Length);
                }
            }
            return result;
        }

        public static string Fetch(string Input, string Delimeter) {
            string result;
            int lpos;
            if (Input == "" || Input == null) { return ""; }

            if (Delimeter.Length < 1) {
                return Input;
            }

            lpos = Input.IndexOf(Delimeter);

            if (lpos == -1) {
                result = Input;
            }
            else {
                result = Input.Substring(0, lpos);
            }
            return result;
        }

        public static float ReadParamFloat(string value, float defvalue) {
            try { return Convert.ToSingle(value); }
            catch { return defvalue; }
        }

        public static float ReadParamFloat(string value, float defvalue, NumberFormatInfo format) {
            try { return Convert.ToSingle(value, format); }
            catch { return defvalue; }
        }

        public static double ReadParamFloat(string value, double defvalue) {
            try { return Convert.ToDouble(value); }
            catch { return defvalue; }
        }

        public static double ReadParamFloat(string value, double defvalue, NumberFormatInfo format) {
            try { return Convert.ToDouble(value, format); }
            catch { return defvalue; }
        }

        public static int ReadParamInt(string value, int defvalue) {
            try { return Convert.ToInt32(value); }
            catch { return defvalue; }
        }

        public static Int64 ReadParamInt64(string value, Int64 defvalue) {
            try { return Convert.ToInt64(value); }
            catch { return defvalue; }
        }

        public static short ReadParamShort(string value, short defvalue) {
            try { return Convert.ToInt16(value); }
            catch { return defvalue; }
        }

        public static DateTime ReadParamDateTime(string value, DateTime defvalue) {
            try { return Convert.ToDateTime(value ?? ""); }
            catch { return defvalue; }
        }

        public static DateTime UnixTimeToDateTime(double unixTimeStamp) {
            return
              (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(unixTimeStamp).ToLocalTime();
        }

        public static int DateTimeToUnixTime(DateTime dateTime) {
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))).Ticks;
            return (int)(unixTimeStampInTicks / TimeSpan.TicksPerSecond);
        }

        public static DateTime JavaTimeToDateTime(double javaTimeStamp) {
            return
              (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(javaTimeStamp).ToLocalTime();
        }

        public static string FromBase64(string s) {
            return BytesToStringUnicode(Convert.FromBase64String(s));
        }

        public static string FromBase64(string s, string srcenc) {
            return BytesToString(Convert.FromBase64String(s), srcenc);
        }

        public static string ToBase64(string s) {
            return Convert.ToBase64String(StringToBytesUnicode(s));
        }

        public static string ToBase64(string s, string srcenc) {
            return Convert.ToBase64String(StringToBytes(s, srcenc));
        }

        public static string MD5(string input, Encoding enc = null) {
            var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = (enc ?? Encoding.UTF8).GetBytes(input);
            return md5.ComputeHash(bytes).BinToHex();
        }

        public static string CreateGUID() {
            return System.Guid.NewGuid().ToString();
        }

        public static string CertGetSubjectParam(string subject, string param) {
            var arr = CertGetSubjectArr(subject);

            for (var i = 0; i < arr.Length; i++)
                if ((param != "") && (arr[i].Default().Length > param.Length) && (arr[i].Default().Substring(0, param.Length + 1) == param + "="))
                    return NormalizeQuotes(arr[i].Default().Substring(param.Length + 1));

            return "";
        }

        public static string[] CertGetKeyUsagesArr(string keyusages) {
            var arr = gUtils.Split(keyusages, ",");

            for (var i = 0; i < arr.Length; i++) {
                arr[i] = arr[i].Trim();
                arr[i] = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\OID\EncodingType 0\CryptDllFindOIDInfo\" + arr[i] + "!7", "Name", "OID." + arr[i]) + " (" + arr[i] + ")";
            }

            return arr;
        }

        public static string[] CertGetSubjectArr(string subject) {
            var arr = gUtils.Split(subject, ", ");

            var i1 = 0;
            while (i1 < arr.Length) {
                if (Regex.IsMatch(arr[i1], @"[0-9\.A-ZА-ЯЁ]+=") == false) {
                    arr[i1 - 1] = arr[i1 - 1] + ", " + arr[i1];

                    for (var i2 = i1; i2 < arr.Length - 1; i2++)
                        arr[i2] = arr[i2 + 1];

                    Array.Resize(ref arr, arr.Length - 1);
                }
                else {
                    i1++;
                }
            }

            return arr;
        }

        private static void UpdateRequestHeaders(string[] headers, HttpWebRequest r) {
            foreach (var h in headers) {
                var value = h;
                var name = Fetch(ref value, ": ", true).ToLower();

                if (name == "host")
                    r.Host = value;
                else
                if (name == "referer")
                    r.Referer = value;
                else
                if (name == "accept")
                    r.Accept = value;
                else
                if (name == "user-agent")
                    r.UserAgent = value;
                else
                if (name == "connection")
                    r.Connection = value;
                else
                    r.Headers.Add(h);
            }

            if (r.Accept == null)
                r.Accept = "text/html,text/xml,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            if (r.Headers["Accept-Encoding"] == null)
                r.Headers.Add("Accept-Encoding: gzip, deflate, sdch");

            if (r.Headers["Accept-Language"] == null)
                r.Headers.Add("Accept-Language: en-US,en;q=0.8,ru;q=0.6,zh-CN;q=0.4,zh;q=0.2,es;q=0.2");

            if (r.Headers["Cache-Control"] == null)
                r.Headers.Add("Cache-Control: no-cache");

            if (r.Headers["Pragma"] == null)
                r.Headers.Add("Pragma: no-cache");
        }

        public static void GetFile(string url, string file, string[] inheaders, out HttpWebResponse res, string proxy, bool redirect = true, int timeout = 100000) {
            var r = CreateWebRequest(url, inheaders, proxy, redirect);

            try {
                res = (HttpWebResponse)r.GetResponse();
            }
            catch (WebException ex) {
                res = (HttpWebResponse)ex.Response;
            }

            try {
                using (var s = res.GetResponseStream())
                using (var fs = new FileStream(file, FileMode.Create)) {
                    var arr = new byte[10000];
                    if (res.ContentLength > 0)
                        Array.Resize(ref arr, (int)res.ContentLength);

                    int read = 0;
                    while ((read = s.Read(arr, 0, arr.Length)) > 0) {
                        fs.Write(arr, 0, read);
                    }
                }
            }
            finally {
                res.Close();
            }
        }

        public static string GetUrl(string url, string[] inheaders, out HttpWebResponse res, Encoding enc, string proxy, bool redirect = true, int timeout = 100000) {
            return
              GetWebResponse(CreateWebRequest(url, inheaders, proxy, redirect), out res, enc);
        }

        public static string PostUrl(string url, string data, string contenttype, string[] inheaders, out HttpWebResponse res, Encoding enc, string proxy, bool redirect = true, int timeout = 100000) {
            var r = CreateWebRequest(url, inheaders, proxy, redirect);

            r.Method = "POST";
            if ((contenttype ?? "") == "")
                r.ContentType = "application/x-www-form-urlencoded";
            else
                r.ContentType = contenttype;

            byte[] buffer = enc.GetBytes(data);
            r.ContentLength = buffer.Length;

            using (var s = r.GetRequestStream())
                s.Write(buffer, 0, buffer.Length);

            return
              GetWebResponse(r, out res, enc);
        }

        private static HttpWebRequest CreateWebRequest(string url, string[] inheaders, string proxy, bool redirect = true, int timeout = 100000) {
            var r = (HttpWebRequest)WebRequest.Create(url);

            if (proxy.Default().Replace(":", "") != "")
                r.Proxy = new WebProxy(Fetch(ref proxy, ":", true), Convert.ToInt32(proxy));

            UpdateRequestHeaders(inheaders ?? new string[0], r);

            r.AllowAutoRedirect = redirect;
            r.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            r.Timeout = timeout;
            r.ReadWriteTimeout = timeout;

            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            return r;
        }

        private static string GetWebResponse(HttpWebRequest r, out HttpWebResponse res, Encoding enc) {
            try {
                res = (HttpWebResponse)r.GetResponse();
            }
            catch (WebException ex) {
                res = (HttpWebResponse)ex.Response;

                if (res == null)
                    throw ex;
            }

            try {
                using (var s = res.GetResponseStream())
                using (var reader = new StreamReader(s, enc ?? Encoding.GetEncoding(res.CharacterSet)))
                    return reader.ReadToEnd();
            }
            finally {
                res.Close();
            }
        }

        private static Stream GetResponseStream(HttpWebResponse res) {
            var s = res.GetResponseStream();

            foreach (var enc in Split(res.ContentEncoding.ToLower(), ", ")) {
                if (enc == "gzip")
                    s = new GZipStream(s, CompressionMode.Decompress);

                if (enc == "deflate")
                    s = new DeflateStream(s, CompressionMode.Decompress);
            }

            return s;
        }

        private string GetRedirectURL(string res, string host) {
            string url = "";
            Match m = Regex.Match(res, "Location: (.+?)\\r\\n");
            if (m.Success) {
                url = m.Groups[1].Value;
                if (url.IndexOf(host) >= 0)
                    url = url.Substring(url.IndexOf(host) + host.Length);
            }
            return url;
        }

        public static string ParseCookie(string cookie1, string cookie2) {
            if (cookie2.Default() == "")
                return cookie1;

            var uri = new System.Uri("http://some.site");
            var cc = new CookieContainer();

            foreach (var cookie in Split(cookie1, "; ")) {
                cc.SetCookies(uri, cookie);
            }

            foreach (var cookie in Split((cookie2 ?? "").Replace(", ", " "), ",")) {
                var newcookie = cookie;
                cc.SetCookies(uri, Fetch(ref newcookie, ";", false));
            }

            return cc.GetCookieHeader(uri);
        }

        public static string UpdateCookie(string cookie, string newcookie) {
            var name = gUtils.Fetch(ref newcookie, "=", true);
            var value = newcookie;

            var cookies = (cookie != "") ? (gUtils.Split(cookie, "; ")) : (new string[0]);
            cookie = "";
            bool b = false;

            for (int i = 0; i < cookies.Length; i++) {
                var name2 = gUtils.Fetch(ref cookies[i], "=", true);
                var value2 = cookies[i];

                if (name == name2) {
                    value2 = value;
                    b = true;
                }

                cookies[i] = name2 + "=" + value2;
            }

            if (b == false)
                Arr.Push(ref cookies, name + "=" + value);

            return gUtils.Join("; ", cookies);
        }

        public static string[] RegexMatch(string input, string pattern, RegexOptions options = RegexOptions.None) {
            var res = new string[0];
            var m = Regex.Match(input.Default(), pattern, options);

            while (m.Success) {
                foreach (Group g in m.Groups)
                    Arr.Push(ref res, g.Value);

                m = m.NextMatch();
            }

            return res;
        }

        public static string RegexMatchLastGroup(string input, string pattern, bool last = false) {
            var res = RegexMatchLastGroupArr(input, pattern);

            if (res.Length > 0)
                return last ? res[res.Length - 1] : res[0];

            return "";
        }

        static public string[] RegexMatchLastGroupArr(string input, string pattern, RegexOptions options = RegexOptions.None) {
            var res = new string[0];

            var m = Regex.Match(input, pattern, options);
            while (m.Success) {
                Arr.Push(ref res, m.Groups[m.Groups.Count - 1].Value);
                m = m.NextMatch();
            }

            return res;
        }

        public static string RegexMatchFirst(string input, string pattern) {
            return RegexMatchLastGroup(input, pattern, false);
        }

        public static string RegexMatchLast(string input, string pattern) {
            return RegexMatchLastGroup(input, pattern, true);
        }

        public static string HtmlEncode(string s) {
            string res = "";
            foreach (char ch in s.ToCharArray())
                res += String.Format("&#{0};", Convert.ToInt16(ch));
            return res;
        }

        public void GzipDecompress(string infile, string outfile) {
            using (FileStream inFile = System.IO.File.OpenRead(infile)) {
                using (FileStream outFile = System.IO.File.Create(outfile)) {
                    using (GZipStream Decompress = new GZipStream(inFile, CompressionMode.Decompress)) {
                        var buff = new byte[Decompress.Length];
                        int readcount = Decompress.Read(buff, 0, buff.Length);
                        outFile.Write(buff, 0, readcount);
                    }
                }
            }
        }

        static public byte RandomByte() {
            var buf = new byte[1];
            var rand = new RNGCryptoServiceProvider(new CspParameters());
            rand.GetBytes(buf);
            rand.Dispose();
            return buf[0];
        }

        static public string GetRandomHex(int length) {
            var res = "";
            var b = new byte[1];
            for (int i = 0; i < length; i++) {
                b[0] = RandomByte();
                res += BinToHex(b);
            }

            return res;
        }

        static public void DeleteFile(string filename) {
            if (File.Exists(filename)) {
                File.SetAttributes(filename, FileAttributes.Normal);
                File.Delete(filename);
            }
        }

        static public string ReadTextFile(string filename, Encoding encoding) {
            return File.ReadAllText(filename, encoding);
        }

        static public void WriteTextFile(string filename, string text, bool append, Encoding encoding) {
            using (var outfile = new StreamWriter(filename, append, encoding))
                outfile.Write(text);
        }

        public static int ExecuteCommand(string command, string args, int timeout, bool oem, out string stdout, out string stderr, bool useshell = false, bool nowindow = true) {
            string _stdout = "";
            string _stderr = "";

            Process p = new Process();
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = useshell;

            if (useshell == false) {
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                p.OutputDataReceived += ((object sender, DataReceivedEventArgs e) => { _stdout += e.Data + "\n"; });
                p.ErrorDataReceived += ((object sender, DataReceivedEventArgs e) => { _stderr += e.Data + "\n"; });
            }

            p.StartInfo.CreateNoWindow = nowindow;
            p.Start();

            if (useshell == false) {
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            p.WaitForExit(timeout);

            var exitcode = -1;
            stdout = _stdout;
            stderr = _stderr;

            try {
                exitcode = p.ExitCode;
            }
            catch { }

            try {
                p.Kill();
            }
            catch { }

            p.Close();

            if (oem) {
                stdout = BytesToString(StringToBytes(stdout), "cp866");
                stderr = BytesToString(StringToBytes(stderr), "cp866");
            }

            return exitcode;
        }

        public static string TrimSpaces(string s) {
            int len = 0;
            do {
                len = s.Length;
                s = s.Replace("  ", " ");
            } while (s.Length < len);
            return s;
        }

        public static string EncodeKey(string key, string password, byte salt) {
            if (password.Length < key.Length) { return ""; }

            var keybuff = StringToBytes(key, "");
            var passbuff = StringToBytes(password, "");

            int i = 0;
            for (int i2 = 0; i2 < passbuff.Length; i2++) {
                keybuff[i] = (byte)(keybuff[i] ^ passbuff[i2]);
                i = (i < keybuff.Length - 1) ? (i + 1) : (0);
            }

            for (i = 0; i < keybuff.Length; i++)
                keybuff[i] = (byte)(keybuff[i] ^ salt);

            return BinToHex(keybuff);
        }

        public static bool IsEmail(string Email) {
            string strRegex = @"^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$";
            if (Regex.IsMatch(Email, strRegex))
                return (true);
            else
                return (false);
        }

        public static bool IsUrl(string Url) {
            string strRegex = "^(https?://)?"
            + "(([0-9a-z_!~*'().&=+$%-]+: )?[0-9a-z_!~*'().&=+$%-]+@)?" //user@ 
            + @"(([0-9]{1,3}\.){3}[0-9]{1,3}" // IP- 199.194.52.184 
            + "|" // allows either IP or domain 
            + @"([0-9a-z_!~*'()-]+\.)*" // tertiary domain(s)- www. 
            + @"([0-9a-z][0-9a-z-]{0,61})?[0-9a-z]\." // second level domain 
            + "[a-z]{2,6})" // first level domain- .com or .museum 
            + "(:[0-9]{1,4})?" // port number- :80 
            + "((/?)|" // a slash isn't required if there is no file name 
            + "(/[0-9a-zA-F_!~*'().;?:@&=+$,%#-]+)+/?)$";
            if (Regex.IsMatch(Url, strRegex, RegexOptions.IgnoreCase))
                return (true);
            else
                return (false);
        }

        private static string FormatNum10(char num, bool allownil = false) {
            if (num == '9') { return "девять"; }
            if (num == '8') { return "восемь"; }
            if (num == '7') { return "семь"; }
            if (num == '6') { return "шесть"; }
            if (num == '5') { return "пять"; }
            if (num == '4') { return "четыре"; }
            if (num == '3') { return "три"; }
            if (num == '2') { return "два"; }
            if (num == '1') { return "один"; }
            if ((allownil) && (num == '0')) { return "ноль"; }

            return "";
        }

        private static string FormatNum100(string num, bool allownil = false) {
            if (num.Length > 1) {
                if (num[0] == '9') { return "девяносто " + FormatNum10(num[1]); }
                if (num[0] == '8') { return "восемьдесят " + FormatNum10(num[1]); }
                if (num[0] == '7') { return "семьдесят " + FormatNum10(num[1]); }
                if (num[0] == '6') { return "шестьдесят " + FormatNum10(num[1]); }
                if (num[0] == '5') { return "пятьдесят " + FormatNum10(num[1]); }
                if (num[0] == '4') { return "сорок " + FormatNum10(num[1]); }
                if (num[0] == '3') { return "тридцать " + FormatNum10(num[1]); }
                if (num[0] == '2') { return "двадцать " + FormatNum10(num[1]); }
                if (num[0] == '1') {
                    if (num == "19") { return "девятнадцать"; }
                    if (num == "18") { return "восемнадцать"; }
                    if (num == "17") { return "семнадцать"; }
                    if (num == "16") { return "шестнадцать"; }
                    if (num == "15") { return "пятнадцать"; }
                    if (num == "14") { return "четырнадцать"; }
                    if (num == "13") { return "тринадцать"; }
                    if (num == "12") { return "двенадцать"; }
                    if (num == "11") { return "одиннадцать"; }
                    if (num == "10") { return "десять"; }
                }
                if (num[0] == '0') { return FormatNum10(num[1]); }
            }

            return FormatNum10(num[0], allownil);
        }

        private static string FormatNum1000(string num, bool allownil = false) {
            if (num.Length > 2) {
                if (num[0] == '9') { return "девятьсот " + FormatNum100(num.Substring(1)); }
                if (num[0] == '8') { return "восемьсот " + FormatNum100(num.Substring(1)); }
                if (num[0] == '7') { return "семьсот " + FormatNum100(num.Substring(1)); }
                if (num[0] == '6') { return "шестьсот " + FormatNum100(num.Substring(1)); }
                if (num[0] == '5') { return "пятьсот " + FormatNum100(num.Substring(1)); }
                if (num[0] == '4') { return "четыреста " + FormatNum100(num.Substring(1)); }
                if (num[0] == '3') { return "триста " + FormatNum100(num.Substring(1)); }
                if (num[0] == '2') { return "двести " + FormatNum100(num.Substring(1)); }
                if (num[0] == '1') { return "сто " + FormatNum100(num.Substring(1)); }
            }

            return FormatNum100(num, allownil);
        }

        private static string FormatNum1000000(string num) {
            if (num.Length > 1) {
                num = FormatNum1000(num, false) + " тысяч";
            }
            else {
                if (num != "0")
                    num = FormatNum1000(num, false) + " тысяч";
                else
                    num = "";
                num = num
                  .Replace("четыре тысяч", "четыре тысячи")
                  .Replace("три тысяч", "три тысячи")
                  .Replace("два тысяч", "две тысячи")
                  .Replace("один тысяч", "одна тысяча");
            }

            return num;
        }

        public static string FormatNum(double num) {
            var res =
              (FormatNum1000000(Math.Truncate(num / 1000).ToString()) + " " +
               FormatNum1000((num - Math.Floor(num / 1000) * 1000).ToString(), Math.Truncate(num / 1000) <= 0))
              .Trim();

            return
              res.Substring(0, 1).ToUpper() + res.Substring(1);
        }

        public static string FormatRubles(double num) {
            if ((num % 100 > 10) && (num % 100 < 20))
                return "рублей";
            if (num % 10 == 1)
                return "рубль";
            if ((num % 10 > 0) && (num % 10 < 5))
                return "рубля";
            return "рублей";
        }

        public static string FormatKopecks(double num) {
            if ((num % 100 > 10) && (num % 100 < 20))
                return "копеек";
            if (num % 10 == 1)
                return "копейка";
            if ((num % 10 > 0) && (num % 10 < 5))
                return "копейки";
            return "копеек";
        }

        public static object GetObjectProperty(object o, string propname) {
            return o.GetType().GetProperty(propname).GetValue(o, null);
        }

        public static void CopyObjectProperties(object from, object to, string[] exclude = null) {
            foreach (var p in from.GetType().GetProperties())
                if ((to.GetType().GetProperty(p.Name) != null) && ((exclude == null) || (exclude.Contains(p.Name) == false)))
                    to.GetType().GetProperty(p.Name).SetValue(to, from.GetType().GetProperty(p.Name).GetValue(from, null), null);
        }

        public static void NullEmptyObjectProperties(object o, string[] exclude = null) {
            foreach (var p in o.GetType().GetProperties())
                if ((p.PropertyType.FullName.Contains("System.String")) && (o.GetType().GetProperty(p.Name) != null) && ((exclude == null) || (exclude.Contains(p.Name) == false))) {
                    var value = ((string)o.GetType().GetProperty(p.Name).GetValue(o, null)).Default();
                    o.GetType().GetProperty(p.Name).SetValue(o, value != "" ? value : null, null);
                }
        }

        public static string FormatObjectProperties(object o, Dictionary<string, string> props) {
            var res = new string[0];

            props = props.ToDictionary(x => x.Key.ToUpper(), x => x.Value);

            foreach (var p in o.GetType().GetProperties())
                if ((o.GetType().GetProperty(p.Name) != null) && (props.Keys.Contains(p.Name.ToUpper()))) {
                    if ((p.PropertyType.FullName.Contains("System.String")) || (p.PropertyType.FullName.Contains("System.Int")))
                        Arr.Push(ref res, props[p.Name.ToUpper()] + " " + GetObjectProperty(o, p.Name));
                    if (p.PropertyType.FullName.Contains("System.DateTime"))
                        Arr.Push(ref res, props[p.Name.ToUpper()] + " " + (GetObjectProperty(o, p.Name) != null ? ((DateTime)GetObjectProperty(o, p.Name)).ToString("dd.MM.yyyy") : ""));
                    if (p.PropertyType.FullName.Contains("System.Byte"))
                        Arr.Push(ref res, props[p.Name.ToUpper()] + " " + (((byte?)GetObjectProperty(o, p.Name) ?? 0) != 0 ? "Да" : "Нет"));
                }

            return Join(", ", res);
        }

        public static string NormalizeQuotes(string s) {
            s = s.Replace("\"\"", "||");
            s = s.Replace("\"", "");
            s = s.Replace("||", "\"");
            return s;
        }

        public static string UnQuote(string s) {
            return
              Regex.Unescape(s).Trim(new char[] { '"', '\'', '\n' });
        }

        public static string NormalizeXml(string s) {
            s = s.Replace("\n", "");
            foreach (var ns in gUtils.RegexMatchLastGroupArr(s, "xmlns:(.+?)=")) {
                s = s.Replace("xmlns:" + ns, "xmlns").Replace("<" + ns + ":", "<").Replace("</" + ns + ":", "</");
                foreach (var attr in gUtils.RegexMatchLastGroupArr(s, ns + ":(.+?)="))
                    s = s.Replace(ns + ":" + attr, attr);
            }
            s = Regex.Replace(s, "xmlns=\"(.+?)\"", "");
            s = Regex.Replace(s, "\\s+>", ">");
            return s;
        }

        public static string NormalizePhone(string phones) {
            var res = new string[0];

            var lastarea = "";
            var lastlength = 0;

            var validAreas = new string[] { "301", "302", "302", "341", "342", "343", "345", "346", "347", "349", "351", "352", "353", "381", "382", "383", "384", "385", "388", "390", "391", "394", "395", "401", "411", "413", "415", "416", "421", "423", "424", "426", "427", "471", "472", "473", "474", "475", "481", "482", "483", "484", "485", "486", "487", "491", "492", "493", "494", "495", "495", "496", "811", "812", "813", "814", "814", "815", "816", "817", "818", "818", "820", "821", "831", "833", "834", "835", "836", "841", "842", "843", "844", "844", "845", "846", "847", "848", "851", "855", "861", "862", "863", "863", "865", "866", "867", "871", "872", "873", "877", "878", "879" };

            foreach (var ph in phones.Default().Split(new char[] { ',', ';' })) {
                var s = ph.Default().Replace("+", "").Replace("доб", "+");
                s = Regex.Replace(s, @"[^\d\+]", "");

                if (s == "") { continue; }

                if (s.Length == 6) { s = "2" + s; }
                if (s.Length == 7) { s = "473" + s; }
                if (s.Length == 8) { s = "47" + s; }
                if ((s.Length == 9) && (validAreas.Contains(s.Substring(0, 3)))) { s += "0"; }
                if ((s.Length == 9) && (validAreas.Contains(s.Substring(0, 3)) == false)) { s = "0" + s; }
                if ((s.Length == 11) && (s[0] == '8')) { s = s.Substring(1); }

                var area = "";
                var phone = "";
                var dop = "";
                var isphone = true;

                for (var i = s.Length - 1; i >= 0; i--) {
                    if (isphone) {
                        if ((phone.Length == 6) && (s[i] != '2') && (s[i] != '3')) {
                            area = phone.Substring(0, 1);
                            phone = phone.Substring(1);

                            isphone = false;
                        }
                        else
                        if (s[i] == '+') {
                            dop = phone;
                            phone = "";
                        }
                        else {
                            phone = s[i] + phone;

                            if (phone.Length == 7) {
                                isphone = false;
                                continue;
                            }
                        }
                    }

                    if (isphone == false) {
                        if (area.Length + phone.Length < 10)
                            area = s[i] + area;
                    }
                }

                if ((area != "") && (area.Length >= 3) && ((area[0] == '9') || (area.Substring(0, 3) == "495"))) {
                    phone = area.Substring(3) + phone;
                    area = area.Substring(0, 3);
                }

                if (area == "073") { area = "473"; }
                area = area != "" ? area : phone.Length == lastlength ? lastarea : "";

                lastlength = phone.Length;

                phone = ("(" + area + ") ").Replace("() ", "") + phone + (dop != "" ? " доб. " + dop : "");

                if (res.Contains(phone) == false)
                    Arr.Push(ref res, phone);

                lastarea = area;
            }

            return gUtils.Join(", ", res);
        }

        public static string NormalizeMail(string mails) {
            var res = new string[0];

            foreach (var m in mails.Default().ToLower().Split(new char[] { ',', ';' })) {
                var s = gUtils.RegexMatchFirst(m.Default(), @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");

                if (s == "") { continue; }

                if (res.Contains(s) == false)
                    Arr.Push(ref res, s);
            }

            return gUtils.Join(", ", res);
        }

        public static string CertOIDtoNames(string s) {
            s = s.Replace("OID.", "");
            s = s.Replace("2.5.4.9=", "STREET=");
            s = s.Replace("2.5.4.3=", "CN=");
            s = s.Replace("2.5.4.42=", "G=");
            s = s.Replace("2.5.4.4=", "SN=");
            s = s.Replace("2.5.4.6=", "C=");
            s = s.Replace("2.5.4.7=", "L=");
            s = s.Replace("2.5.4.8=", "S=");
            s = s.Replace("2.5.4.10=", "O=");
            s = s.Replace("2.5.4.11=", "OU=");
            s = s.Replace("2.5.4.12=", "T=");
            s = s.Replace("1.2.840.113549.1.9.1=", "E=");
            s = s.Replace("1.2.840.113549.1.9.2=", "UN=");
            s = s.Replace("1.2.643.3.131.1.1=", "ИНН=");
            s = s.Replace("1.2.643.3.141.1.1=", "РНС ФСС=");
            s = s.Replace("1.2.643.3.141.1.2=", "КП ФСС=");
            s = s.Replace("1.2.643.100.1=", "ОГРН=");
            s = s.Replace("1.2.643.100.3=", "СНИЛС=");
            s = s.Replace("1.2.643.100.4=", "ИНН ЮЛ=");
            s = s.Replace("1.2.643.100.5=", "ОГРНИП=");

            return s;
        }

        public static string CertNamestoOID(string s) {
            s = s.Replace("STREET=", "2.5.4.9=");
            s = s.Replace("CN=", "2.5.4.3=");
            s = s.Replace("G=", "2.5.4.42=");
            s = s.Replace("SN=", "2.5.4.4=");
            s = s.Replace("C=", "2.5.4.6=");
            s = s.Replace("L=", "2.5.4.7=");
            s = s.Replace("S=", "2.5.4.8=");
            s = s.Replace("O=", "2.5.4.10=");
            s = s.Replace("OU=", "2.5.4.11=");
            s = s.Replace("T=", "2.5.4.12=");
            s = s.Replace("E=", "1.2.840.113549.1.9.1=");
            s = s.Replace("UN=", "1.2.840.113549.1.9.2=");
            s = s.Replace("ИНН=", "1.2.643.3.131.1.1=");
            s = s.Replace("РНС ФСС=", "1.2.643.3.141.1.1=");
            s = s.Replace("КП ФСС=", "1.2.643.3.141.1.2=");
            s = s.Replace("ОГРН=", "1.2.643.100.1=");
            s = s.Replace("СНИЛС=", "1.2.643.100.3=");
            s = s.Replace("ИНН ЮЛ=", "1.2.643.100.4=");
            s = s.Replace("ОГРНИП=", "1.2.643.100.5=");

            return s;
        }

        public static bool IsOneInstance(bool allowOtherOwner = true, bool allowOtherPath = false) {
            var me = Process.GetCurrentProcess();

            string? meuser = null;
            string? medomain = null;

            if (allowOtherOwner)
                GetProcessOwner(me, out meuser, out medomain);

            foreach (var process in Process.GetProcessesByName(me.ProcessName.Replace(".vshost", "")))
                try {
                    if (process.Id == me.Id) { continue; }

                    string? user = null;
                    string? domain = null;

                    if (allowOtherOwner)
                        GetProcessOwner(process, out user, out domain);

                    if (((me.MainModule.FileName == process.MainModule.FileName) && ((allowOtherOwner == false) || (user == meuser))) ||
                        ((me.MainModule.FileName != process.MainModule.FileName) && (allowOtherPath == false)))
                        return false;
                }
                catch { }

            return true;
        }

        public static bool GetProcessOwner(Process process, out string user, out string domain) {
            user = null;
            domain = null;

            var query = new SelectQuery("Win32_Process", "Name = '" + process.MainModule.ModuleName + "'", new[] { "Handle", "ProcessId" });

            using (var searcher = new ManagementObjectSearcher(query))
            using (var processes = searcher.Get())
                foreach (ManagementObject p in processes)
                    if ((uint)p["ProcessId"] == process.Id) {
                        var outParameters = new object[2];

                        if ((uint)p.InvokeMethod("GetOwner", outParameters) == 0) {
                            user = (string)outParameters[0];
                            domain = (string)outParameters[1];

                            return true;
                        }
                    }

            return false;
        }

        public static double Eval(string expr) {
            return
              Convert.ToDouble((new DataTable()).Compute(expr, ""));
        }

        public static void ClearDynamic(dynamic data) {
            var dict = data as IDictionary<string, object>;
            var deleteit = new List<string>();
            foreach (var key in dict.Keys)
                if (dict[key] == null)
                    deleteit.Add(key);
            foreach (var key in deleteit)
                dict.Remove(key);
        }

        public static string CyrToLat(string s) {
            var cyrlat = new Dictionary<char, char>() {
                { 'а', 'a' }, { 'б', '6' }, { 'е', 'e' }, { 'з', '3' }, { 'и', 'n' }, { 'к', 'k' }, { 'м', 'm' }, { 'н', 'h' }, { 'о', 'o' }, { 'р', 'p' }, { 'с', 'c' }, { 'т', 't' }, { 'у', 'y' }, { 'х', 'x' }, { 'ч', '4' }, { 'э', '3' },
                { 'А', 'A' }, { 'Б', '6' }, { 'В', 'B' }, { 'Е', 'E' }, { 'З', '3' }, { 'И', 'N' }, { 'К', 'K' }, { 'М', 'M' }, { 'Н', 'H' }, { 'О', 'O' }, { 'Р', 'P' }, { 'С', 'C' }, { 'Т', 'T' }, { 'У', 'Y' }, { 'Х', 'X' }, { 'Ч', '4' }, { 'Э', '3' }
            };

            foreach (var cl in cyrlat)
                s = s.Replace(cl.Key, cl.Value);

            return s;
        }

        public static void CopyFiles(string sourcePath, string targetPath, string searchPatternDirs, string searchPatternFiles, bool recursively = true) {
            //create all of the directories
            foreach (var pattern in gUtils.Split(searchPatternDirs, ";"))
                foreach (var dirPath in Directory.GetDirectories(sourcePath, pattern, recursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

            //copy all the files & replaces any files with the same name
            foreach (var pattern in gUtils.Split(searchPatternDirs, ";"))
                foreach (string newPath in Directory.GetFiles(sourcePath, pattern, recursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }

    public static partial class Extensions {
        public static dynamic ToDynamic<T>(this T obj) {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var propertyInfo in typeof(T).GetProperties()) {
                var currentValue = propertyInfo.GetValue(obj, null);
                expando.Add(propertyInfo.Name, currentValue);
            }
            return expando as ExpandoObject;
        }

        public static string NullIfEmpty(this string s, bool trim = true) {
            return (s != null) && (trim ? (s ?? "").Trim() != "" : (s ?? "") != "") ? s : null;
        }

        public static string Default(this string? s, bool trim = true) {
            return trim ? (s ?? "").Trim() : (s ?? "");
        }

        public static DateTime Default(this DateTime? dt) {
            return dt != null ? dt.Value : new DateTime(0);
        }

        public static bool Default(this bool? b) {
            return b ?? false;
        }

        public static string DefaultShortDate(this DateTime? dt) {
            return dt != null ? dt.Value.ToShortDateString() : "";
        }

        public static bool Alive(this TcpClient tcp) {
            try {
                if ((tcp.Connected == false) || (tcp.Client.Connected) == false)
                    return false;

                if (tcp.Client.Poll(0, SelectMode.SelectRead)) {
                    var buff = new byte[1];
                    if (tcp.Client.Receive(buff, SocketFlags.Peek) == 0)
                        return false;
                }
            }
            catch {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod() {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }
    }
}
