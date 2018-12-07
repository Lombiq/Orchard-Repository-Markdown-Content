using Lombiq.RepositoryMarkdownContent.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Services
{
    /// <summary>
    /// Service for managing MarkdownRepo content items. This is independent from the repo type.
    /// </summary>
    public interface IMarkdownContentItemManager : IDependency
    {
        /// <summary>
        /// Creates and sets the content item that contains a MarkdownPagePart.
        /// Also fires custom events.
        /// </summary>
        /// <param name="markdownRepoPart">The corresponding MarkdownRepoPart.</param>
        /// <param name="markdownText">The text for the BodyPart, this comes from a file in the repo.</param>
        /// <param name="mdFileRelativePath">The relative path of the .md file in the repo.</param>
        void Create(MarkdownRepoPart markdownRepoPart, string markdownText, string mdFileRelativePath);

        /// <summary>
        /// Deletes the content item. Also fires custom events.
        /// </summary>
        /// <param name="markdownRepoPart">The corresponding MarkdownRepoPart.</param>
        /// <param name="mdFileRelativePath">The relative path of the .md file in the repo.</param>
        void Delete(MarkdownRepoPart markdownRepoPart, string mdFileRelativePath);

        /// <summary>
        /// Modifies the content item that contains a MarkdownPagePart.
        /// Also fires custom events.
        /// </summary>
        /// <param name="markdownRepoPart">The corresponding MarkdownRepoPart.</param>
        /// <param name="markdownText">The new text for the BodyPart, this comes from a file in the repo.</param>
        /// <param name="mdFileRelativePath">The relative path of the .md file in the repo.</param>
        void Modify(MarkdownRepoPart markdownRepoPart, string markdownText, string mdFileRelativePath);

        /// <summary>
        /// Rename the .md file.
        /// Also fires custom events.
        /// </summary>
        /// <param name="markdownRepoPart">The corresponding MarkdownRepoPart.</param>
        /// <param name="mdFileRelativePath">The new relative path of the .md file in the repo.</param>
        /// <param name="previousMdFileRelativePath">The previous relative path of the .md file in the repo.</param>
        void Rename(MarkdownRepoPart markdownRepoPart, string mdFileRelativePath, string previousMdFileRelativePath);

        /// <summary>
        /// Deletes all corresponding content items with MarkdownPagePart.
        /// </summary>
        /// <param name="markdownRepoPart">The MarkdownRepoPart.</param>
        void DeleteAll(MarkdownRepoPart markdownRepoPart);
    }
}