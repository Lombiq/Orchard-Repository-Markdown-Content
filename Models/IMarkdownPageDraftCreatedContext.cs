using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Models
{
    /// <summary>
    /// Context for draft created events.
    /// </summary>
    public interface IMarkdownPageDraftCreatedContext
    {
        /// <summary>
        /// The currently created content item.
        /// </summary>
        IContent ContentItem { get; }
    }
}