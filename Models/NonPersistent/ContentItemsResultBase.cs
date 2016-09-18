using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Models.NonPersistent
{
    public class ContentItemsResultBase
    {
        public bool Success { get; set; }
        public string LatestProcessedCommitHash { get; set; }
        public string ErrorMessage { get; set; }
    }
}