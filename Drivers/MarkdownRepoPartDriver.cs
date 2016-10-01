using Lombiq.RepositoryMarkdownContent.Models;
using Lombiq.RepositoryMarkdownContent.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement.Handlers;

namespace Lombiq.RepositoryMarkdownContent.Drivers
{
    public class MarkdownRepoPartDriver : ContentPartDriver<MarkdownRepoPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public Localizer T { get; set; }


        public MarkdownRepoPartDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;

            T = NullLocalizer.Instance;
        }


        protected override DriverResult Editor(MarkdownRepoPart part, dynamic shapeHelper)
        {
            return ContentShape(
                "Parts_MarkdownRepo_Edit",
                () =>
                {
                    var accessibleContentTypes =
                        GetTypesWithMarkdownPagePart()
                        .Select(item =>
                            new SelectListItem { Text = item, Value = item, Selected = item == part.ContentType });

                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts.MarkdownRepo",
                        Model: new MarkdownRepoPartEditorViewModel
                        {
                            MarkdownRepoPart = part,
                            AccessibleContentTypes = accessibleContentTypes
                        },
                        Prefix: Prefix);
                });
        }

        protected override DriverResult Editor(MarkdownRepoPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                if (part.RepoUrl.EndsWith("/"))
                {
                    part.RepoUrl = part.RepoUrl.Remove(part.RepoUrl.Length - 1);
                }

                if (!GetTypesWithMarkdownPagePart().Contains(part.ContentType))
                {
                    updater.AddModelError("InvalidContentType", T("Please select a content type with MarkdownPagePart."));
                }
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(MarkdownRepoPart part, ExportContentContext context)
        {
            ExportInfoset(part, context);
        }

        protected override void Importing(MarkdownRepoPart part, ImportContentContext context)
        {
            ImportInfoset(part, context);
        }


        private IEnumerable<string> GetTypesWithMarkdownPagePart()
        {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd =>
                    ctd.Parts.Any(p => p.PartDefinition.Name == typeof(MarkdownPagePart).Name) &&
                    ctd.Settings.GetModel<ContentTypeSettings>().Draftable)
                .Select(ctd => ctd.Name);
        }
    }
}