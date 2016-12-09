using System.Collections.Generic;

namespace GithubTelemetryParse.Models
{
    public class Views
    {
        public int count { get; set; }
        public int uniques { get; set; }
        public IEnumerable<View> views { get; set; }
    }
}
