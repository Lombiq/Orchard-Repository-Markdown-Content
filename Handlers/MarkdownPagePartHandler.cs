using Lombiq.RepositoryMarkdownContent.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Handlers
{
    public class MarkdownPagePartHandler : ContentHandler
    {
        public Localizer T { get; set; }

        public MarkdownPagePartHandler(
            ITransactionManager transactionManager,
            IRepository<MarkdownPagePartRecord> repository,
            INotifier notifier)
        {
            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(repository));

            OnRemoving<MarkdownPagePart>((ctx, part) =>
            {
                if (!part.DeletionAllowed)
                {
                    transactionManager.Cancel();
                }
            });

            OnRemoved<MarkdownPagePart>((ctx, part) =>
            {
                if (!part.DeletionAllowed)
                {
                    notifier.Warning(T("This item can't be removed because it is synchronized automatically from a repository."));
                }
            });
        }
    }
}