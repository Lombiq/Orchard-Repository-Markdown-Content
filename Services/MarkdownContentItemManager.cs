using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.RepositoryMarkdownContent.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement.MetaData;
using Lombiq.RepositoryMarkdownContent.Events;

namespace Lombiq.RepositoryMarkdownContent.Services
{
    public class MarkdownContentItemManager : IMarkdownContentItemManager
    {
        private readonly IMarkdownPageEventHandler _markdownPageEventHandler;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;


        public MarkdownContentItemManager(
            IMarkdownPageEventHandler markdownPageEventHandler,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager)
        {
            _markdownPageEventHandler = markdownPageEventHandler;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
        }


        public void Create(MarkdownRepoPart markdownRepoPart, string markdownText, string mdFileRelativePath)
        {
            var markdownContentItem = _contentManager.New(markdownRepoPart.ContentType);

            if (!markdownContentItem.Has(typeof(BodyPart)))
            {
                _contentDefinitionManager.AlterTypeDefinition(
                    markdownContentItem.ContentType,
                    cfg => cfg
                         .WithPart("BodyPart",
                            part => part
                                .WithSetting("BodyTypePartSettings.Flavor", "markdown")));

                markdownContentItem = _contentManager.New(markdownRepoPart.ContentType);
            }

            markdownContentItem.As<BodyPart>().Text = markdownText;
            var markdownPagePart = markdownContentItem.As<MarkdownPagePart>();
            markdownPagePart.MarkdownFilePath = mdFileRelativePath;
            markdownPagePart.MarkdownRepoId = markdownRepoPart.ContentItem.Id;

            // Creating the content item draft.
            _contentManager.Create(markdownContentItem, VersionOptions.Draft);

            // Firing event so custom extensions can make their own tasks.
            _markdownPageEventHandler
                .MarkdownPageDraftCreated(new MarkdownPageDraftCreatedContext { ContentItem = markdownContentItem });

            // Publishing the content item.
            _contentManager.Publish(markdownContentItem);
        }

        public void Delete(MarkdownRepoPart markdownRepoPart, string mdFileRelativePath)
        {
            var markdownPagesToDelete = _contentManager
                .Query(markdownRepoPart.ContentType)
                .Where<MarkdownPagePartRecord>(
                    record => record.MarkdownRepoId == markdownRepoPart.ContentItem.Id &&
                    record.MarkdownFilePath == mdFileRelativePath);

            // In case of duplicates we delete all of them.
            foreach (var markdownPageToDelete in markdownPagesToDelete.List())
            {
                markdownPageToDelete.As<MarkdownPagePart>().DeletionAllowed = true;
                // Firing event so custom extensions can make their own tasks.
                _markdownPageEventHandler
                    .MarkdownPageRemoving(new MarkdownPageRemovingContext { ContentItem = markdownPageToDelete });

                _contentManager.Remove(markdownPageToDelete);
            }
        }

        public void Modify(MarkdownRepoPart markdownRepoPart, string markdownText, string mdFileRelativePath)
        {
            var markdownPagesToModify = _contentManager
                .Query(markdownRepoPart.ContentType)
                .Where<MarkdownPagePartRecord>(
                    record => record.MarkdownRepoId == markdownRepoPart.ContentItem.Id &&
                    record.MarkdownFilePath == mdFileRelativePath);

            var first = true;
            // In case of duplicates we modify one and remove the duplicates.
            foreach (var markdownPageToModify in markdownPagesToModify.List())
            {
                if (first)
                {
                    markdownPageToModify.As<BodyPart>().Text = markdownText;

                    // Firing event so custom extensions can make their own tasks.
                    _markdownPageEventHandler
                        .MarkdownPageModified(new MarkdownPageModifiedContext { ContentItem = markdownPageToModify });

                    first = false;
                }
                else
                {
                    // Firing event so custom extensions can make their own tasks.
                    _markdownPageEventHandler
                        .MarkdownPageRemoving(new MarkdownPageRemovingContext { ContentItem = markdownPageToModify });

                    _contentManager.Remove(markdownPageToModify);
                }
            }
        }

        public void DeleteAll(MarkdownRepoPart markdownRepoPart)
        {
            var correspondingMarkdownPages = _contentManager
                .Query(markdownRepoPart.ContentType)
                .Where<MarkdownPagePartRecord>(record => record.MarkdownRepoId == markdownRepoPart.ContentItem.Id)
                .List();

            foreach (var correspondingMarkdownPage in correspondingMarkdownPages)
            {
                correspondingMarkdownPage.As<MarkdownPagePart>().DeletionAllowed = true;

                _contentManager.Remove(correspondingMarkdownPage);
            }
        }
    }
}