using Lombiq.RepositoryMarkdownContent.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Lombiq.RepositoryMarkdownContent.Migrations
{
    public class MarkdownPagePartMigrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder
                .CreateTable(typeof(MarkdownPagePartRecord).Name,
                   table => table
                       .ContentPartRecord()
                       .Column<string>("MarkdownFilePath")
                       .Column<int>("MarkdownRepoId")
                   )
                .AlterTable(typeof(MarkdownPagePartRecord).Name,
                   table =>
                    {
                        table.CreateIndex("MarkdownFilePath", "MarkdownFilePath");
                        table.CreateIndex("MarkdownRepoId", "MarkdownRepoId");
                    });

            ContentDefinitionManager.AlterPartDefinition(typeof(MarkdownPagePart).Name,
                part => part
                    .Attachable(true)
                    .WithDescription("If you attach this to a type then that type will be selectable during MarkdownRepo creation."));

            return 1;
        }
    }
}