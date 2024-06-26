namespace MMTrayIcon.Models {
    internal class Posts {
        public List<string>? order { get; set; }
        public Dictionary<string, Post>? posts { get; set; }
    }

    internal class Post {
        public long update_at { get; set; }
        public string user_id { get; set; } = null!;
        public string channel_id { get; set; } = null!;
        public string message { get; set; } = null!;
        public List<string>? file_ids { get; set; }
        public MetaData metadata { get; set; } = null!;
    }

    internal class MetaData {
        public List<File>? files { get; set; }
    }

    internal class File {
        public string id { get; set; } = null!;
        public string name { get; set; } = null!;
        public string mime_type { get; set; } = null!;
    }
}
