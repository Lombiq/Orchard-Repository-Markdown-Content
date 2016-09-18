using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Helpers
{
    public static class MarkdownRepoPartHelpers
    {
        public static bool FolderOrFileIsInRepoFolder(string repoFolder, string folderOrFilePath)
        {
            return repoFolder == "//" || folderOrFilePath.StartsWith(repoFolder);
        }
    }
}