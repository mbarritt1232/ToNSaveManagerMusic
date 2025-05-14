using Newtonsoft.Json;

namespace ToNSaveManager.Models
{
    internal class Music
    {
        [JsonIgnore] public string? DisplayName { get; set; }
        [JsonIgnore] public string? Tooltip { get; set; }

        [JsonIgnore] public bool IsCompleted {
            get => !IsSeparator && MainWindow.SaveData.GetCompleted(Name);
            set
            {
                if (!IsSeparator) MainWindow.SaveData.SetCompleted(Name, value);
            }
        }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public bool IsSeparator { get; set; } = false;
        public Color DisplayColor { get; set; }

        [JsonConstructor]
        public Music() {}

        /// <param name="name">Music name</param>
        /// <param name="reference">Music link to wiki</param>
        public Music(string name, string description, string reference, Color color)
        {
            Name = name;
            Description = description;
            Reference = reference;
            DisplayColor = color.A > 0 ? color : Color.White;
        }

        public void Toggle()
        {
            IsCompleted = !IsCompleted;
        }

        public static Music Separator(string name)
        {
            return new Music()
            {
                Name = name,
                IsSeparator = true
            };
        }


        public override string ToString()
        {
            return string.IsNullOrEmpty(DisplayName) ? Name : DisplayName;
        }

        public static List<Music> ImportFromMemory()
        {
            using (Stream? stream = Program.GetEmbededResource("music.json"))
            {
                if (stream == null) return new List<Music>();

                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<List<Music>>(json) ?? new List<Music>();
                }
            }
        }
    }
}
