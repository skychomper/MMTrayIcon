namespace MMTrayIcon.Models {
    public enum IconState {
        Main,
        On,
        Off
    }

    public readonly struct PopupColor {
        public static Color LightForeColorTitle = Color.FromArgb(245, 245, 245);
        public static Color LightForeColorMessage = Color.FromArgb(192, 195, 201);
        public static Color LightBackColor = Color.FromArgb(255, 255, 255);
        public static Color DarkForeColorTitle = Color.FromArgb(34, 34, 34);
        public static Color DarkForeColorMessage = Color.FromArgb(153, 153, 153);
        public static Color DarkBackColor = Color.FromArgb(23, 33, 43);
    }
}
