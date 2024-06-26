using System.Diagnostics;
using System.Reflection;
using gunit;

namespace MMTrayIcon {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            //only one instance
            if (gUtils.IsOneInstance(allowOtherPath: true) == false) {
                Application.Exit();
                return;
            }

#if !DEBUG
            //junction
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var localappPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appPath = Path.Combine(localappPath, Assembly.GetExecutingAssembly().GetName().Name, "app");
            var dllName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var exeName = dllName.Replace(".dll", ".exe");

            if ((path.StartsWith(localappPath) == false) || (path.Substring(localappPath.Length).StartsWith(@"\Apps\"))) {
                try {
                    if ((File.Exists(Path.Combine(appPath, dllName)) == false) ||
                        //(Assembly.GetExecutingAssembly().GetName().Version.ToString() != Assembly.LoadFile(Path.Combine(appPath, dllName)).GetName().Version.ToString()))
                        (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion != FileVersionInfo.GetVersionInfo(Path.Combine(appPath, dllName)).FileVersion)) {

                        gUtils.CopyFiles(path, appPath, "*", "*.exe;*.dll;*.json");
                    }
                }
                catch { }

                try {
                    var p = new Process();
                    p.StartInfo.FileName = Path.Combine(appPath, exeName);
                    p.StartInfo.Arguments = gUtils.ToBase64(path);
                    p.StartInfo.UseShellExecute = true;
                    p.Start();
                    p.Close();

                    //gUtils.ExecuteCommand("cmd", "/c " + Assembly.GetExecutingAssembly().Location.Replace(verPath, "app").Replace(".dll", ".exe"), 10000, false, out var stdout, out var stderr, true);

                    Application.Exit();

                    return;
                    /*else {
                      if (args.Length == 1) {
                        var verPath = gUtils.FromBase64(args[0]);

                        DeleteDirectory(verPath);
                        Directory.CreateSymbolicLink(verPath, appPath);
                      }
                    } */
                }
                catch { }
            }
#endif

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        /*static void DeleteDirectory(string path) {
          var count = 10;

          while (Directory.Exists(path) && (count > 0)) {
            Thread.Sleep(1000);

            count--;

            try {
              Directory.Delete(path, true);

              count = 0;
            }
            catch { }
          }
        } */
    }
}