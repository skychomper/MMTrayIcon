using System.Media;
using System.Reflection;
using System.Xml;
using gunit;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using MMTrayIcon.Models;
using Newtonsoft.Json;

namespace MMTrayIcon {
    public partial class MainForm : Form {
        //список доменов, при клике на ссылки с которыми не будет открываться внешнее приложение (браузер)
        //первое в списке значение используется как адрес подключения
        public static readonly string[] HostNames = { "mm.mydomain.ru", "passport.mydomain.ru", "identity.mydomain.ru", "auth.mydomain.ru" };
        //ИД команды
        private const string TeamID = "my_team_id";

        public static MainForm self = null!;
        private string appversion;
        private XmlDocument xml = new XmlDocument();
        private XmlElement? options;
        private FormWindowState lastState = FormWindowState.Normal;
        public static bool darkMode = false;
        private bool newLineCtrlEnter = false;
        private bool minimizeTray = false;
        private bool minimizeClose = false;
        private bool showPopup = false;
        private bool blinkTray = false;
        private Icon iconMain, iconRed, iconOn, iconOff;
        private IconState iconState = IconState.Main;
        private SoundPlayer soundMessage;
        private int lastUnreadCount = 0;
        private int lastPostTextboxCount = 0;
        private string me = "", lastHistory = "", lastChannel = "", lastMessage = "";
        private bool controlPressed = false;
        private PopupForm popupForm = new PopupForm();
        private Dictionary<string, Post> activity = new Dictionary<string, Post>();
        private Dictionary<string, Post> history = new Dictionary<string, Post>();
        private Dictionary<string, Channel>? channels = new Dictionary<string, Channel>();
        private Dictionary<string, string> users = new Dictionary<string, string>();

        public void GetOptions() {
            try {
                xml.Load(Path.ChangeExtension(Application.ExecutablePath, "xml"));
            }
            catch (Exception ex) {
                //AppendLog("ERROR: Ошибка чтения файла настроек: " + ex.Message);
            }

            var doc = xml.DocumentElement;
            if (doc == null) {
                var doctype = xml.CreateDocumentFragment();
                doctype.InnerXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
                xml.AppendChild(doctype);
                doc = xml.CreateElement("root");
                xml.AppendChild(doc);
            }

            options = doc.SelectSingleNode("options") as XmlElement;
            if (options == null) {
                options = xml.CreateElement("options");
                doc.AppendChild(options);
            }

            if (options.HasAttribute("darkMode") == false) {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    if (key?.GetValue("AppsUseLightTheme") is int theme)
                        darkMode = theme == 0;
            }
            else
                darkMode = options.GetAttribute("darkMode") != "";

            newLineCtrlEnter = GetBoolOption("newLineCtrlEnter", false);
            minimizeTray = GetBoolOption("minimizeTray", true);
            minimizeClose = GetBoolOption("minimizeClose", false);
            showPopup = GetBoolOption("showPopup", true);
            blinkTray = GetBoolOption("blinkTray", true);

            //apply

            lastState = options.GetAttribute("maximized") != "" ? FormWindowState.Maximized : FormWindowState.Normal;
            ((ToolStripMenuItem)((ToolStripItemCollection)((ToolStripDropDownItem)contextMenuStrip1.Items[1]).DropDownItems)[0]).Checked = darkMode;
            ((ToolStripMenuItem)((ToolStripItemCollection)((ToolStripDropDownItem)contextMenuStrip1.Items[1]).DropDownItems)[1]).Checked = newLineCtrlEnter;
            ((ToolStripMenuItem)((ToolStripItemCollection)((ToolStripDropDownItem)contextMenuStrip1.Items[1]).DropDownItems)[2]).Checked = minimizeTray;
            ((ToolStripMenuItem)((ToolStripItemCollection)((ToolStripDropDownItem)contextMenuStrip1.Items[1]).DropDownItems)[3]).Checked = minimizeClose;
            ((ToolStripMenuItem)((ToolStripItemCollection)((ToolStripDropDownItem)contextMenuStrip1.Items[1]).DropDownItems)[4]).Checked = showPopup;
            ((ToolStripMenuItem)((ToolStripItemCollection)((ToolStripDropDownItem)contextMenuStrip1.Items[1]).DropDownItems)[5]).Checked = blinkTray;
        }

        private bool GetBoolOption(string attr, bool defaultValue) {
            return
              defaultValue ?
                !options.HasAttribute(attr) || options.GetAttribute(attr) != "" :
                options.HasAttribute(attr) && options.GetAttribute(attr) != "";
        }

        public void SaveOptions() {
            try {
                SaveXML();
                GetOptions();
            }
            catch { }
        }

        public void SaveXML() {
            try {
                xml.Save(Path.ChangeExtension(Application.ExecutablePath, "xml"));
            }
            catch { }
        }

        private async void ClickUnread() {
            {
                var count = gUtils.ReadParamInt(gUtils.UnQuote(await webView21.ExecuteScriptAsync(@"document.querySelectorAll('.unread.active').length")), 0);
                if (count > 0) {
                    await webView21.ExecuteScriptAsync("document.querySelector('.unread.active .SidebarLink').click();");

                    return;
                }
            }

            foreach (var first in activity.OrderBy(x => x.Value.update_at).ToList()) {
                activity.Remove(first.Key);

                var count = gUtils.ReadParamInt(gUtils.UnQuote(await webView21.ExecuteScriptAsync(@"document.querySelectorAll('.unread #sidebarItem_" + first.Key + "').length")), 0);
                if (count > 0) {
                    await webView21.ExecuteScriptAsync("document.querySelector('.unread #sidebarItem_" + first.Key + "').click();");

                    return;
                }
            }

            await webView21.ExecuteScriptAsync("document.querySelector('.unread .SidebarLink').click();");
        }

        private async void ClickSidebarThreads() {
            await webView21.ExecuteScriptAsync("document.querySelector('.SidebarLink').click();");
        }

        public MainForm() {
            self = this;

            InitializeComponent();

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Resources.IconMain)))
                iconMain = new Icon(ms);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Resources.IconRed)))
                iconRed = new Icon(ms);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Resources.IconOn)))
                iconOn = new Icon(ms);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Resources.IconOff)))
                iconOff = new Icon(ms);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Resources.SoundMessage))) {
                soundMessage = new SoundPlayer(ms);
                soundMessage.Load();
            }

            GetOptions();

            WindowState = lastState;
            gUtils.UseImmersiveDarkMode(Handle, darkMode);
        }

        private void MainForm_Load(object sender, EventArgs e) {
            appversion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

            Icon = iconMain;

            notifyIcon1.Text = "Mattermost Tray Icon";
            notifyIcon1.Icon = iconMain;

            webView21.Source = new Uri("https://" + HostNames[0]);
        }

        private void MainForm_Resize(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized) {
                if (minimizeTray)
                    Hide();

                ClickSidebarThreads();
            }
            else {
                if (WindowState != lastState) {
                    options.SetAttribute("maximized", WindowState == FormWindowState.Maximized ? "1" : "");

                    SaveOptions();
                }

                /*if (iconState != IconState.Main)
                  ClickUnread(); */
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if ((e.CloseReason == CloseReason.UserClosing) && (minimizeClose)) {
                WindowState = FormWindowState.Minimized;
                Hide();
                ClickSidebarThreads();

                e.Cancel = true;
            }
        }

        public void открытьToolStripMenuItem_Click(object sender, EventArgs e) {
            //if (WindowState != FormWindowState.Minimized)
            ClickUnread();

            Show();
            WindowState = lastState;
            gUtils.SetForegroundWindow(Handle);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void тёмнаяТемаToolStripMenuItem_Click(object sender, EventArgs e) {
            if (gUtils.UseImmersiveDarkMode(Handle, !darkMode)) {
                options.SetAttribute("darkMode", !darkMode ? "1" : "");

                SaveOptions();
            }
        }

        private async void timer1_Tick(object sender, EventArgs e) {
            timer1.Enabled = false;

            try {
                //textbox
                /*if (newLineCtrlEnter) {
                  var count = gUtils.ReadParamInt(gUtils.Unquote(await webView21.ExecuteScriptAsync(@"document.querySelectorAll('textarea').length"))), 0);

                  if (count != lastPostTextboxCount) {
                    await webView21.ExecuteScriptAsync(@"function TextareaKeydown(e) { if ( e.which == 13 ) { if (e.ctrlKey) { var val = this.value; var pos = this.selectionStart; var style = window.getComputedStyle(this); this.value = val.substring(0, pos) + '\n' + val.substring(pos); this.selectionStart = pos + 1; this.selectionEnd = this.selectionStart; var height = style.getPropertyValue('line-height').replace('px', '') * this.value.split('\n').length + style.getPropertyValue('padding-top').replace('px', '') * 1.0 + style.getPropertyValue('padding-bottom').replace('px', '') * 1.0; this.setAttribute('height', height); this.style.height = height + 'px'; e.preventDefault(); } } }");
                    await webView21.ExecuteScriptAsync(@"document.querySelectorAll('textarea').forEach(function(ta) { ta.addEventListener('keydown', TextareaKeydown); })");
                  }

                  lastPostTextboxCount = count;
                } */

                //unread
                {
                    var count = gUtils.ReadParamInt(gUtils.UnQuote(await webView21.ExecuteScriptAsync(@"document.querySelectorAll('.unread').length + Array.from(document.querySelectorAll('.unread #unreadMentions')).map(x => x.innerText).join('')")), 0);

                    var items = (Dictionary<string, string>?)null;

                    if (count != lastUnreadCount) {
                        //<channel_name, user_name>
                        items = gUtils.Split(gUtils.UnQuote(await webView21.ExecuteScriptAsync(@"Array.from(document.querySelectorAll('.unread')).map(x => x.querySelector('.unread-title').id + '=' + x.querySelector('.SidebarChannelLinkLabel').innerText).join('|')")), "|")
                                      .Select(x => gUtils.Split(x, "="))
                                      .ToDictionary(x => { gUtils.Fetch(ref x[0], "_", true); return x[0]; }, x => x[1]); //нельзя по двойному __, т.к. у групп его нет

                        if (items.Count > 0) {
                            //удаляем старые
                            activity = activity.Where(x => items.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                        }
                    }

                    if (count == 0) {
                        if (iconState != IconState.Main) {
                            iconState = IconState.Main;
                            Icon = iconMain;
                            notifyIcon1.Icon = iconMain;
                        }
                    }
                    else {
                        if (count > lastUnreadCount) {
                            soundMessage.Play();

                            gUtils.FlashWindow(Handle, (uint)(FlashWindowFlags.All | FlashWindowFlags.TimerNoForeground));

                            if (items?.Count > 0) {
                                await UpdateMe();
                                await UpdateChannels();

                                //fill activity
                                foreach (var item in items) {
                                    var ch = channels?.Where(x => x.Value.name == item.Key).FirstOrDefault().Value;

                                    var posts = await GetPosts(ch?.id, true);

                                    if (posts?.order?.Count > 0) {
                                        var post = posts.posts?[posts.order[0]];

                                        UpdateUsers(ch!.name, item.Value);

                                        //обновляем
                                        if (activity.ContainsKey(ch!.name))
                                            activity[ch!.name] = post!;
                                        else
                                            activity.Add(ch!.name, post!);

                                        if (history.ContainsKey(ch!.name))
                                            history[ch!.name] = post!;
                                        else
                                            history.Add(ch!.name, post!);
                                    }
                                }

                                //всплывающее уведомление
                                if ((Focused == false) && (showPopup)) {
                                    var last = activity.OrderByDescending(x => x.Value.update_at).FirstOrDefault();

                                    if (last.Key != null)
                                        popupForm.Show(last.Value.user_id!, items[last.Key], (last.Value.message + "\n" +
                                                                                            (last.Value.file_ids?.Count > 0 ?
                                                                                             gUtils.Join("\n", last.Value.metadata!.files!.Select(x => (x.mime_type.StartsWith("image") ? "Картинка " : "Файл ") + x.name).ToArray()) :
                                                                                             "")
                                                                                           ).Trim().ReplaceLineEndings());

                                    //Focus();
                                }
                            }
                        }

                        if (iconState == IconState.Main) {
                            iconState = IconState.Off;
                            Icon = iconRed;
                            notifyIcon1.Icon = blinkTray ? iconOff : iconRed;
                        }

                        if (blinkTray) {
                            if (iconState == IconState.On) {
                                iconState = IconState.Off;
                                notifyIcon1.Icon = iconOff;
                            }
                            else {
                                iconState = IconState.On;
                                notifyIcon1.Icon = iconOn;
                            }
                        }
                    }

                    lastUnreadCount = count;
                }

                //history
                {
                    var hist = gUtils.Join("", history.OrderByDescending(x => x.Value.update_at).Select(x => x.Key).ToArray());
                    var channel = gUtils.Split(gUtils.UnQuote(await webView21.ExecuteScriptAsync(@"document.querySelector('#channel-header').dataset.channelid + '=' + document.querySelector('#channelHeaderTitle').innerText")), "=");

                    if ((hist != lastHistory) || (channel[0] != lastChannel)) {
                        await UpdateMe();
                        await UpdateChannels();

                        _ = webView21.ExecuteScriptAsync(@"document.querySelectorAll('div.channel-header__title > button').forEach(function(el) { if (!el.querySelector('.MenuWrapper')) { el.remove(); } });" +
                                                         gUtils.Join("", history.Where(x => x.Value.channel_id != channel[0]).OrderByDescending(x => x.Value.update_at).Select(x =>
                                                         $"var el_btn = document.createElement('button');" +
                                                         $"el_btn.setAttribute('class', 'channel-header__trigger style--none');" +
                                                         $"el_btn.setAttribute('style', 'margin-left: 20px; max-width: fit-content;');" +
                                                         $"el_btn.innerHTML = '<strong class=\"heading\" style=\"margin: 0 5px 0 10px;\">{users[x.Key]}</strong><i data-id=\"{x.Key}\" class=\"icon-close\">';" +
                                                         $"document.querySelector('div.channel-header__title').append(el_btn);" +
                                                         $"el_btn.addEventListener('click', function(e) {{ this.remove(); document.querySelector('#sidebarItem_{x.Key}').click(); }});" +
                                                         $"el_btn.querySelector('i').addEventListener('click', function(e) {{ this.parentElement.remove(); window.chrome.webview.postMessage(JSON.stringify({{ removeHistory: this.dataset.id }})); e.stopPropagation(); }});").ToArray()));

                        lastHistory = hist;
                        lastChannel = channel[0];
                        lastMessage = await GetLastMessage();
                    }

                    if (channel[0] == lastChannel) {
                        var message = await GetLastMessage();

                        if (message != lastMessage) {
                            await UpdateChannels();

                            var ch = channels?[channel[0]];

                            var posts = await GetPosts(ch?.id, false);

                            if (posts?.order?.Count > 0) {
                                var post = posts.posts?[posts.order[0]];

                                //post!.user_id = ch!.name;

                                UpdateUsers(ch!.name, channel[1]);

                                //обновляем
                                if (history.ContainsKey(ch!.name))
                                    history[ch!.name] = post!;
                                else
                                    history.Add(ch!.name, post!);
                            }

                            lastMessage = message;
                        }
                    }
                }

                //links
                {
                    _ = webView21.ExecuteScriptAsync(@"document.querySelectorAll('a.markdown__link').forEach(function(el) { if (el.getAttribute('data-href') == undefined) { el.addEventListener('click', function(e) { window.chrome.webview.postMessage(JSON.stringify({ link: this.getAttribute('data-href') })); }); el.setAttribute('data-href', el.href); el.setAttribute('href', '#'); el.removeAttribute('target') } })");
                }

                //restore app
                {
                    var appExists = await webView21.ExecuteScriptAsync(@"document.querySelectorAll('body.app__body').length") == "1";
                    if (!appExists)
                        await webView21.ExecuteScriptAsync(@"window.onload();");
                }
            }
            catch (Exception ex) {
            }
            finally {
                timer1.Enabled = true;
            }
        }

        private void UpdateUsers(string channel_name, string user_name) {
            if (users.ContainsKey(channel_name))
                users[channel_name] = user_name;
            else
                users.Add(channel_name, user_name);
        }

        private async Task UpdateMe() {
            me = gUtils.RegexMatchFirst(gUtils.UnQuote(await webView21.ExecuteScriptAsync(@"document.querySelector('header .Avatar').src")), "users/(.+?)/");
        }

        private async Task UpdateChannels() {
            var json = gUtils.UnQuote(await webView21.ExecuteScriptAsync($"var xhttp = new XMLHttpRequest(); xhttp.open('GET', 'https://{HostNames[0]}/api/v4/users/me/teams/{TeamID}/channels?include_deleted=true', false); xhttp.send(); xhttp.responseText"));

            channels = JsonConvert.DeserializeObject<List<Channel>>(json)?.ToDictionary(x => x.id, x => x);
        }

        private async Task<string> GetLastMessage() {
            return
              gUtils.UnQuote(await webView21.ExecuteScriptAsync(@"Array.from(document.querySelectorAll('.post-message__text')).pop().id"));
        }

        private async Task<Posts?> GetPosts(string? channel, bool unread) {
            if (channel == null) { return null; }

            var res = gUtils.UnQuote(await webView21.ExecuteScriptAsync($"var xhttp = new XMLHttpRequest(); xhttp.open('GET', 'https://{HostNames[0]}/api/v4/{(unread ? $"users/{me}/" : "")}channels/{channel}/posts{(unread ? "/unread?limit_before=0" : "?per_page=1")}', false); xhttp.send(); xhttp.responseText"));

            return
              JsonConvert.DeserializeObject<Posts>(res);
        }

        private void webView21_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e) {
            timer1.Enabled = true;
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e) {
            notifyIcon1.ShowBalloonTip(5000, "Mattermost", "Версия " + appversion, ToolTipIcon.Info);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                открытьToolStripMenuItem_Click(sender, e);
        }

        private async void переводСтрокиПоCtrlEnterToolStripMenuItem_Click(object sender, EventArgs e) {
            /*if (newLineCtrlEnter) {
              await webView21.ExecuteScriptAsync(@"document.querySelectorAll('textarea').forEach(function(ta) { ta.removeEventListener('keydown', TextareaKeydown); })");
              lastPostTextboxCount = 0;
            } */

            options.SetAttribute("newLineCtrlEnter", !newLineCtrlEnter ? "1" : "");

            SaveOptions();
        }

        private void webView21_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e) {
            webView21.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e) {
            try {
                if (/*(gUtils.IsUrl(e.Uri)) && */(IsInHostNames(e.Uri) == false)) {
                    gUtils.ExecuteCommand(e.Uri, "", 1000, false, out var stdout, out var sterr, true);
                    e.Handled = true;
                }
            }
            catch { }
        }

        private void webView21_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e) {
            try {
                if (/*(gUtils.IsUrl(e.Uri)) && */IsInHostNames(e.Uri) == false) {
                    gUtils.ExecuteCommand(e.Uri, "", 1000, false, out var stdout, out var sterr, true);
                    e.Cancel = true;
                }
            }
            catch { }
        }

        private bool IsInHostNames(string url) {
            foreach (var host in HostNames)
                if (url.Contains(host))
                    return true;

            return false;
        }

        private void webView21_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e) {
            try {
                dynamic? message = JsonConvert.DeserializeObject(e.TryGetWebMessageAsString());

                //link click
                var link = (string?)message?.link;

                if ((link != null) && /*(gUtils.IsUrl(link)) && */(IsInHostNames(link) == false)) {
                    gUtils.ExecuteCommand(link, "", 1000, false, out var stdout, out var sterr, true);
                }

                //remove history click
                var historyID = (string?)message?.removeHistory;

                if (historyID != null)
                    history.Remove(historyID);
            }
            catch { }
        }

        private void сворачиватьВТрейToolStripMenuItem_Click(object sender, EventArgs e) {
            options.SetAttribute("minimizeTray", !minimizeTray ? "1" : "");

            SaveOptions();
        }

        private void сворачиватьПриЗакрытииОкнаToolStripMenuItem_Click(object sender, EventArgs e) {
            options.SetAttribute("minimizeClose", !minimizeClose ? "1" : "");

            SaveOptions();
        }

        private void моргатьИконкойВТрееToolStripMenuItem_Click(object sender, EventArgs e) {
            options.SetAttribute("blinkTray", !blinkTray ? "1" : "");

            SaveOptions();
        }

        private void показыватьВсплывающееУведомлениеToolStripMenuItem_Click(object sender, EventArgs e) {
            options.SetAttribute("showPopup", !showPopup ? "1" : "");

            SaveOptions();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                WindowState = FormWindowState.Minimized;

                /*if (minimizeTray)
                  Hide(); */

                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.ControlKey)
                controlPressed = true;

            if ((newLineCtrlEnter) && ((e.Control) || (controlPressed)) && (e.KeyCode == Keys.Enter)) {
                SendKeys.Send("+{ENTER}");

                e.SuppressKeyPress = true;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.ControlKey)
                controlPressed = false;
        }

        /*private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs e) {
          try {
            if (e.Error != null)
              throw new Exception(e.Error.Message, e.Error);

            if (e.IsUpdateAvailable) {
              var res = (DialogResult?)null;

              if (e.Mandatory.Value) {
                res = MessageBox.Show(
                        $@"Доступна новая версия приложения {e.CurrentVersion}. Вы используете версию {e.InstalledVersion}. Это обязательное обновление. Нажмите OK для начала процесса обновления.",
                        @"Доступно обновление", MessageBoxButtons.OK, MessageBoxIcon.Information);
              }
              else {
                res = MessageBox.Show(
                        $@"There is new version {e.CurrentVersion} available. You are using version {e.InstalledVersion}. Do you want to update the application now?",
                        @"Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
              }

              if (res.Equals(DialogResult.Yes) || (res.Equals(DialogResult.OK)))
                if (AutoUpdater.DownloadUpdate(e))
                  Application.Exit();
            }
            else {
              MessageBox.Show(@"There is no update available please try again later.", @"No update available",
                  MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
          }
          catch(Exception ex) {
            if (ex is WebException) {
              MessageBox.Show(
                  @"Не удалось подключиться к серверу обновлений. Проверьте подключение и перезапустите приложение.",
                  @"Не удалось проверить обновления", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else {
              MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
          }
        } */
    }
}
