using System;

namespace Flimsy.Logger {
    public class FlimsyLog {
        public DateTime Created { get; }

        public DateTime Saved { get; set; }

        public string Category { get; set; }

        public string Text { get; set; }

        public string Metadata { get; set; }

        public string Payload { get; set; }

        public FlimsyLog() {
            this.Created = DateTime.Now;
        }
    }
}