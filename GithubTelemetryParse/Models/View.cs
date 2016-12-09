using System;

namespace GithubTelemetryParse.Models
{
    public class View
    {
        public DateTime timestamp { get; set; }
        public int count { get; set; }
        public int uniques { get; set; }
    }
}
