using Lombiq.RepositoryMarkdownContent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lombiq.RepositoryMarkdownContent.ViewModels
{
    public class MarkdownRepoPartEditorViewModel
    {
        public MarkdownRepoPart MarkdownRepoPart { get; set; }
        public IEnumerable<SelectListItem> AccessibleContentTypes { get; set; }
    }
}