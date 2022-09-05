namespace Configurations.Models
{
    public class GitApiConfigurationModel
    {
        public string Url { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public List<string> BranchNames { get; set; } = new List<string>();

        /// <summary>
        /// If commit's comment is started with these values - it will be ignored
        /// </summary>
        public List<string> IgnoreComments { get; set; } = new List<string>();

        /// <summary>
        /// If commit's committer email is started with these values - it will be ignored
        /// </summary>
        public List<string> IgnoreCommitterEmails { get; set; } = new List<string>();

        /// <summary>
        /// Limit of commits per request
        /// </summary>
        public int CommitsPerPage { get; set; } = 200;

        /// <summary>
        /// Limit for simultaneous operations (avoid Git overload)
        /// </summary>
        public int TasksLimit { get; set; } = 32;
    }
}
