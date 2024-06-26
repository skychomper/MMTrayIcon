using MMTrayIcon.Models;

namespace MMTrayIcon {
    public partial class PopupForm : Form {
        public PopupForm() {
            InitializeComponent();

            textBox1.Cursor = Cursors.Arrow;
            textBox1.GotFocus += textBox1_GotFocus;

            Visible = false;
            ShowInTaskbar = false;
            TopMost = true;
        }

        public void Show(string img, string title, string message) {
            PopupWebView.Source = new Uri($"https://{MainForm.HostNames[0]}/api/v4/users/{img}/image");
            label1.Text = title;
            textBox1.Text = message;

            BackColor = MainForm.darkMode ? PopupColor.DarkBackColor : PopupColor.LightBackColor;
            label1.ForeColor = MainForm.darkMode ? PopupColor.LightForeColorTitle : PopupColor.DarkForeColorTitle;
            textBox1.BackColor = BackColor;
            textBox1.ForeColor = MainForm.darkMode ? PopupColor.LightForeColorMessage : PopupColor.DarkForeColorMessage;

            Show();

            timer1.Interval = 7000;
            timer1.Enabled = true;
        }

        protected override void WndProc(ref Message m) {
            //WM_PARENTNOTIFY && (WM_LBUTTONDOWN || WM_RBUTTONDOWN)
            if ((m.Msg == 0x210) && ((m.WParam.ToInt32() == 0x201) || (m.WParam.ToInt32() == 0x204))) {
                Hide();

                timer1.Enabled = false;

                if (m.WParam.ToInt32() == 0x201) {
                    timer1.Interval = 100;
                    timer1.Enabled = true;
                }
            }

            base.WndProc(ref m);
        }

        private void PopupForm_Load(object sender, EventArgs e) {
            PopupWebView.EnsureCoreWebView2Async();
        }

        private void PopupForm_Shown(object sender, EventArgs e) {
            var area = Screen.GetWorkingArea(this);
            Location = new Point(area.Right - Size.Width - 10, area.Bottom - Size.Height - 10);
        }

        private void timer1_Tick(object sender, EventArgs e) {
            timer1.Enabled = false;

            Hide();

            if (timer1.Interval == 100)
                MainForm.self.открытьToolStripMenuItem_Click(this, EventArgs.Empty);
        }

        private void textBox1_GotFocus(object sender, EventArgs e) {
            label2.Focus();
        }

        private void PopupWebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e) {
            PopupWebView.ExecuteScriptAsync("document.querySelector('img').style.pointerEvents = 'none';");
        }
    }
}
