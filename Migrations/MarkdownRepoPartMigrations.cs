using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Lombiq.RepositoryMarkdownContent.Models;
using Lombiq.RepositoryMarkdownContent.Constants;

namespace Lombiq.RepositoryMarkdownContent.Migrations
{
    public class MarkdownRepoPartMigrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(MarkdownRepoPartRecord).Name,
                table => table
                    .ContentPartRecord()
                    .Column<string>("Username")
                    .Column<string>("EncodedPassword", column => column.Unlimited())
                    .Column<string>("EncodedAccessToken", column => column.Unlimited()));

            ContentDefinitionManager.AlterPartDefinition(
                typeof(MarkdownRepoPart).Name,
                part => part
                    .Attachable(false)
                    .WithField(FieldNames.RepoUrl, field => field
                        .OfType("TextField")
                        .WithDisplayName("Repo Url")
                        .WithSetting("TextFieldSettings.Required", "True")
                        .WithSetting("TextFieldSettings.Hint", "The Url of the repository with markdown files."))
                    .WithField(FieldNames.FolderName, field => field
                        .OfType("TextField")
                        .WithDisplayName("Folder Name")
                        .WithSetting("TextFieldSettings.Required", "True")
                        .WithSetting("TextFieldSettings.Hint", "The name of the folder inside the repository. The module will search this folder recursively. \"/\" means the root folder."))
                    .WithField(FieldNames.BranchName, field => field
                        .OfType("TextField")
                        .WithDisplayName("Branch Name")
                        .WithSetting("TextFieldSettings.Required", "True")
                        .WithSetting("TextFieldSettings.Hint", "Name of the branch."))
                    .WithField(FieldNames.MinutesBetweenChecks, field => field
                        .OfType("NumericField")
                        .WithDisplayName("Minutes Between Checks")
                        .WithSetting("NumericFieldSettings.Required", "True")
                        .WithSetting("NumericFieldSettings.Hint", "A background task will check the repository for changes.")
                        .WithSetting("NumericFieldSettings.Minimum", "1"))
                     .WithField(FieldNames.DeleteMarkdownPagesOnRemoving, field => field
                        .OfType("BooleanField")
                        .WithDisplayName("Delete Markdown Pages On Removing")
                        .WithSetting("BooleanFieldSettings.Required", "True")
                        .WithSetting("BooleanFieldSettings.Hint", "If checked, on removing this item the corresponding synchronized markdown pages will be removed too.")));

            ContentDefinitionManager.AlterTypeDefinition(
                ContentTypes.MarkdownRepo,
                cfg => cfg
                    .WithPart(typeof(MarkdownRepoPart).Name)
                    .Creatable()
                    .Listable()
                    .Securable()
                    .DisplayedAs("Markdown Repo")
                    .WithPart("CommonPart",
                        part => part
                            .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "False")
                            .WithSetting("DateEditorSettings.ShowDateEditor", "False")));

            return 1;
        }
    }
}