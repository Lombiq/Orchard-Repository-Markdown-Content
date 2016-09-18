using Lombiq.RepositoryMarkdownContent.Models;
using Orchard.ContentManagement;
using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Events
{
    /// <summary>
    /// Custom event handlers in the <see cref="MarkdownContentItemManager"/> class, for the content items what contains MarkdownPagePart.
    /// </summary>
    public interface IMarkdownPageEventHandler : IEventHandler
    {
        /// <summary>
        /// Called after a content item was created in the MarkdownContentItemManager.
        /// </summary>
        /// <param name="markdownPageDraftCreatedContext">The created context.</param>
        void MarkdownPageDraftCreated(IMarkdownPageDraftCreatedContext markdownPageDraftCreatedContext);

        /// <summary>
        /// Called before a content item removal in the MarkdownContentItemManager.
        /// </summary>
        /// <param name="markdownPageRemovingContext">The removing context.</param>
        void MarkdownPageRemoving(IMarkdownPageRemovingContext markdownPageRemovingContext);

        /// <summary>
        /// Called after a content item was modified in the MarkdownContentItemManager.
        /// </summary>
        /// <param name="markdownPageModifiedContext">The modified context.</param>
        void MarkdownPageModified(IMarkdownPageModifiedContext markdownPageModifiedContext);
    }
}