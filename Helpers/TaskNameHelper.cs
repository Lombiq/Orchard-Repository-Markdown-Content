using Lombiq.RepositoryMarkdownContent.Constants;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.RepositoryMarkdownContent.Helpers
{
    public static class TaskNameHelper
    {
        public static string GetMarkdownContentUpdaterTaskName(ContentItem contentItem)
        {
            return TaskTypes.MarkdownContentUpdaterPrefix + contentItem.Id.ToString();
        }
    }
}