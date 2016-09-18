using Lombiq.RepositoryMarkdownContent.Models;
using Lombiq.RepositoryMarkdownContent.Models.NonPersistent;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Services
{
    /// <summary>
    /// Service for creating and updating content items from .md files in a GitHub repo.
    /// </summary>
    public interface IGitHubRepoService : IDependency
    {
        /// <summary>
        /// Initializes the content items, this runs only once per repo.
        /// </summary>
        /// <param name="markdownRepoContentItem"></param>
        /// <returns></returns>
        Task<InitContentItemsResult> InitContentItems(ContentItem markdownRepoContentItem);

        /// <summary>
        /// Updates the content items by the changes made in the repo, this runs all the time after the init.
        /// The update will happen in a batch i.e. gets all the commits, analyze them and adds only the latest changes.
        /// E.g. if a modify then a remove happens in the same file, then this only removes it because modifying
        /// a later removed file is pointless.
        /// </summary>
        /// <param name="markdownRepoContentItem"></param>
        /// <returns></returns>
        Task<UpdateContentItemsResult> UpdateContentItems(ContentItem markdownRepoContentItem);
    }
}