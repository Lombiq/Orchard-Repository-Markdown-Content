using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Models
{
    /// <summary>
    /// Context for modified events.
    /// </summary>
    public interface IMarkdownPageModifiedContext
    {
        /// <summary>
        /// The currently modified content item.
        /// </summary>
        IContent ContentItem { get; }
    }
}