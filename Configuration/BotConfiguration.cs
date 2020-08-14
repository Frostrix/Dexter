﻿using System.Collections.Generic;

namespace Dexter.Core.Configuration {
    public class BotConfiguration : AbstractConfiguration {
        public string Token { get; set; }

        public string Bot_Name { get; set; }

        public List<string> ThumbnailURLs { get; set; }
    }
}