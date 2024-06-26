namespace MMTrayIcon {
    partial class MainForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.открытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.параметрыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.тёмнаяТемаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.переводСтрокиПоCtrlEnterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сворачиватьВТрейToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сворачиватьПриЗакрытииОкнаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.показыватьВсплывающееУведомлениеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.моргатьИконкойВТрееToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.открытьToolStripMenuItem,
            this.параметрыToolStripMenuItem,
            this.оПрограммеToolStripMenuItem,
            this.выходToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 114);
            // 
            // открытьToolStripMenuItem
            // 
            this.открытьToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.открытьToolStripMenuItem.Name = "открытьToolStripMenuItem";
            this.открытьToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.открытьToolStripMenuItem.Text = "Открыть";
            this.открытьToolStripMenuItem.Click += new System.EventHandler(this.открытьToolStripMenuItem_Click);
            // 
            // параметрыToolStripMenuItem
            // 
            this.параметрыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.тёмнаяТемаToolStripMenuItem,
            this.переводСтрокиПоCtrlEnterToolStripMenuItem,
            this.сворачиватьВТрейToolStripMenuItem,
            this.сворачиватьПриЗакрытииОкнаToolStripMenuItem,
            this.показыватьВсплывающееУведомлениеToolStripMenuItem,
            this.моргатьИконкойВТрееToolStripMenuItem});
            this.параметрыToolStripMenuItem.Name = "параметрыToolStripMenuItem";
            this.параметрыToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.параметрыToolStripMenuItem.Text = "Параметры";
            // 
            // тёмнаяТемаToolStripMenuItem
            // 
            this.тёмнаяТемаToolStripMenuItem.Name = "тёмнаяТемаToolStripMenuItem";
            this.тёмнаяТемаToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
            this.тёмнаяТемаToolStripMenuItem.Text = "Тёмная тема";
            this.тёмнаяТемаToolStripMenuItem.Click += new System.EventHandler(this.тёмнаяТемаToolStripMenuItem_Click);
            // 
            // переводСтрокиПоCtrlEnterToolStripMenuItem
            // 
            this.переводСтрокиПоCtrlEnterToolStripMenuItem.Name = "переводСтрокиПоCtrlEnterToolStripMenuItem";
            this.переводСтрокиПоCtrlEnterToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
            this.переводСтрокиПоCtrlEnterToolStripMenuItem.Text = "Перевод строки по Ctrl+Enter";
            this.переводСтрокиПоCtrlEnterToolStripMenuItem.Click += new System.EventHandler(this.переводСтрокиПоCtrlEnterToolStripMenuItem_Click);
            // 
            // сворачиватьВТрейToolStripMenuItem
            // 
            this.сворачиватьВТрейToolStripMenuItem.Name = "сворачиватьВТрейToolStripMenuItem";
            this.сворачиватьВТрейToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
            this.сворачиватьВТрейToolStripMenuItem.Text = "Сворачивать в трей";
            this.сворачиватьВТрейToolStripMenuItem.Click += new System.EventHandler(this.сворачиватьВТрейToolStripMenuItem_Click);
            // 
            // сворачиватьПриЗакрытииОкнаToolStripMenuItem
            // 
            this.сворачиватьПриЗакрытииОкнаToolStripMenuItem.Name = "сворачиватьПриЗакрытииОкнаToolStripMenuItem";
            this.сворачиватьПриЗакрытииОкнаToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
            this.сворачиватьПриЗакрытииОкнаToolStripMenuItem.Text = "Сворачивать при закрытии окна";
            this.сворачиватьПриЗакрытииОкнаToolStripMenuItem.Click += new System.EventHandler(this.сворачиватьПриЗакрытииОкнаToolStripMenuItem_Click);
            // 
            // показыватьВсплывающееУведомлениеToolStripMenuItem
            // 
            this.показыватьВсплывающееУведомлениеToolStripMenuItem.Name = "показыватьВсплывающееУведомлениеToolStripMenuItem";
            this.показыватьВсплывающееУведомлениеToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
            this.показыватьВсплывающееУведомлениеToolStripMenuItem.Text = "Показывать всплывающее уведомление";
            this.показыватьВсплывающееУведомлениеToolStripMenuItem.Click += new System.EventHandler(this.показыватьВсплывающееУведомлениеToolStripMenuItem_Click);
            // 
            // моргатьИконкойВТрееToolStripMenuItem
            // 
            this.моргатьИконкойВТрееToolStripMenuItem.Name = "моргатьИконкойВТрееToolStripMenuItem";
            this.моргатьИконкойВТрееToolStripMenuItem.Size = new System.Drawing.Size(298, 22);
            this.моргатьИконкойВТрееToolStripMenuItem.Text = "Моргать иконкой в трее";
            this.моргатьИконкойВТрееToolStripMenuItem.Click += new System.EventHandler(this.моргатьИконкойВТрееToolStripMenuItem_Click);
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            this.оПрограммеToolStripMenuItem.Click += new System.EventHandler(this.оПрограммеToolStripMenuItem_Click);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // webView21
            // 
            this.webView21.CreationProperties = null;
            this.webView21.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView21.Location = new System.Drawing.Point(0, 0);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(990, 694);
            this.webView21.TabIndex = 1;
            this.webView21.ZoomFactor = 1D;
            this.webView21.CoreWebView2InitializationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs>(this.webView21_CoreWebView2InitializationCompleted);
            this.webView21.NavigationStarting += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs>(this.webView21_NavigationStarting);
            this.webView21.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this.webView21_NavigationCompleted);
            this.webView21.WebMessageReceived += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs>(this.webView21_WebMessageReceived);
            this.webView21.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.webView21.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(990, 694);
            this.Controls.Add(this.webView21);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mattermost";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem открытьToolStripMenuItem;
        private ToolStripMenuItem выходToolStripMenuItem;
        private ToolStripMenuItem параметрыToolStripMenuItem;
        private ToolStripMenuItem тёмнаяТемаToolStripMenuItem;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private System.Windows.Forms.Timer timer1;
        private ToolStripMenuItem оПрограммеToolStripMenuItem;
        private ToolStripMenuItem переводСтрокиПоCtrlEnterToolStripMenuItem;
        private ToolStripMenuItem сворачиватьВТрейToolStripMenuItem;
        private ToolStripMenuItem сворачиватьПриЗакрытииОкнаToolStripMenuItem;
        private ToolStripMenuItem показыватьВсплывающееУведомлениеToolStripMenuItem;
        private ToolStripMenuItem моргатьИконкойВТрееToolStripMenuItem;
    }
}