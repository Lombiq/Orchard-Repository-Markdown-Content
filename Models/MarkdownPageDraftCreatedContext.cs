using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Lombiq.RepositoryMarkdownContent.Models
{
    public class MarkdownPageDraftCreatedContext : IMarkdownPageDraftCreatedContext
    {
        public IContent ContentItem { get; set; }
    }
}