using Lombiq.RepositoryMarkdownContent.Constants;
using Lombiq.RepositoryMarkdownContent.Helpers;
using Lombiq.RepositoryMarkdownContent.Models;
using Lombiq.RepositoryMarkdownContent.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Security;
using Orchard.Services;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Handlers
{
    public class MarkdownRepoPartHandler : ContentHandler
    {
        public MarkdownRepoPartHandler(
            IMarkdownContentItemManager markdownContentItemManager,
            IRepository<MarkdownRepoPartRecord> repository,
            IScheduledTaskManager scheduledTaskManager,
            IClock clock,
            IContentManager contentManager,
            IEncryptionService encryptionService)
        {
            Filters.Add(StorageFilter.For(repository));

            OnActivated<MarkdownRepoPart>((context, part) =>
            {
                part.AccessTokenField.Loader(() =>
                {
                    return string.IsNullOrEmpty(part.EncodedAccessToken)
                        ? ""
                        : Encoding.UTF8.GetString(encryptionService.Decode(Convert.FromBase64String(part.EncodedAccessToken)));
                });

                part.AccessTokenField.Setter((value) =>
                {
                    part.EncodedAccessToken = string.IsNullOrEmpty(value)
                        ? ""
                        : Convert.ToBase64String(encryptionService.Encode(Encoding.UTF8.GetBytes(value)));

                    return value;
                });

                part.PasswordField.Loader(() =>
                {
                    return string.IsNullOrEmpty(part.EncodedPassword)
                        ? ""
                        : Encoding.UTF8.GetString(encryptionService.Decode(Convert.FromBase64String(part.EncodedPassword)));
                });

                part.PasswordField.Setter((value) =>
                {
                    part.EncodedPassword = string.IsNullOrEmpty(value)
                        ? ""
                        : Convert.ToBase64String(encryptionService.Encode(Encoding.UTF8.GetBytes(value)));

                    return value;
                });
            });

            OnRemoved<MarkdownRepoPart>((ctx, part) =>
            {
                scheduledTaskManager.DeleteTasks(part.ContentItem);

                if (part.DeleteMarkdownPagesOnRemoving == true)
                {
                    markdownContentItemManager.DeleteAll(part);
                }
                // Since the repo is deleted we doesn't want to prevent the deletion of the markdown pages.
                else
                {
                    var correspondingMarkdownPages = contentManager
                        .Query(part.ContentType)
                        .Where<MarkdownPagePartRecord>(record => record.MarkdownRepoId == part.ContentItem.Id)
                        .List();

                    foreach (var correspondingMarkdownPage in correspondingMarkdownPages)
                    {
                        correspondingMarkdownPage.As<MarkdownPagePart>().DeletionAllowed = true;
                    }
                }
            });

            OnPublished<MarkdownRepoPart>((ctx, part) =>
            {
                if (ctx.PreviousItemVersionRecord != null)
                {
                    scheduledTaskManager.DeleteTasks(part.ContentItem);
                }

                scheduledTaskManager
                        .CreateTask(
                            TaskNameHelper.GetMarkdownContentUpdaterTaskName(part.ContentItem),
                            clock.UtcNow.AddMinutes(1),
                            part.ContentItem);
            });
        }
    }
}