using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Models
{
    public class MarkdownPagePart : ContentPart<MarkdownPagePartRecord>
    {
        /// <summary>
        /// The relative path of the .md file in the repo.
        /// </summary>
        public string MarkdownFilePath
        {
            get { return Retrieve(x => x.MarkdownFilePath); }
            set { Store(x => x.MarkdownFilePath, value); }
        }

        /// <summary>
        /// The corresponding repo content item id. 
        /// This and the MarkdownFilePath identifies the content item from a repository's perspective.
        /// </summary>
        public int MarkdownRepoId
        {
            get { return Retrieve(x => x.MarkdownRepoId); }
            set { Store(x => x.MarkdownRepoId, value); }
        }

        /// <summary>
        /// This indicates if the content item is allowed to be deleted.
        /// If this property is false, then the transaction will be rolled back.
        /// This is necessary because we want to deny the deletion of these items by the user.
        /// </summary>
        public bool DeletionAllowed
        {
            get { return this.Retrieve(x => x.DeletionAllowed); }
            set { this.Store(x => x.DeletionAllowed, value); }
        }
    }

    public class MarkdownPagePartRecord : ContentPartRecord
    {
        public virtual string MarkdownFilePath { get; set; }
        public virtual int MarkdownRepoId { get; set; }
    }
}