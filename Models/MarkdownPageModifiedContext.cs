using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Models
{
    public class MarkdownPageModifiedContext : IMarkdownPageModifiedContext
    {
        public IContent ContentItem { get; set; }
    }
}