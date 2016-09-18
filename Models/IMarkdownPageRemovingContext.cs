using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Models
{
    /// <summary>
    /// Context for removing events.
    /// </summary>
    public interface IMarkdownPageRemovingContext
    {
        /// <summary>
        /// The currently removed content item.
        /// </summary>
        IContent ContentItem { get; }
    }
}