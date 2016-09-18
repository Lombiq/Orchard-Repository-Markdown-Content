using Lombiq.RepositoryMarkdownContent.Constants;
using Lombiq.RepositoryMarkdownContent.Models;
using Lombiq.RepositoryMarkdownContent.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Lombiq.RepositoryMarkdownContent.Drivers
{
    public class MarkdownPagePartDriver : ContentPartDriver<MarkdownPagePart>
    {
        private readonly IContentManager _contentManager;


        public MarkdownPagePartDriver(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }


        protected override DriverResult Editor(MarkdownPagePart part, dynamic shapeHelper)
        {
            return ContentShape(
                "Parts_MarkdownPage_Edit",
                () =>
                {
                    var markdownRepo = _contentManager.Get(part.MarkdownRepoId);
                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts.MarkdownPage",
                        Model: new MarkdownPagePartEditorViewModel
                        {
                            MarkdownPagePart = part,
                            MarkdownRepoUrl = markdownRepo != null && markdownRepo.ContentType == ContentTypes.MarkdownRepo
                            ? markdownRepo.As<MarkdownRepoPart>().RepoUrl
                            : ""
                        },
                        Prefix: Prefix);
                });
        }

        protected override DriverResult Editor(MarkdownPagePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}