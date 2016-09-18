using Lombiq.RepositoryMarkdownContent.Constants;
using Lombiq.RepositoryMarkdownContent.Helpers;
using Lombiq.RepositoryMarkdownContent.Models;
using Lombiq.RepositoryMarkdownContent.Models.NonPersistent;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks.Scheduling;
using Piedone.HelpfulLibraries.Tasks;
using System;

namespace Lombiq.RepositoryMarkdownContent.Services
{
    public class MarkdownContentUpdaterScheduledTask : IScheduledTaskHandler, IOrchardShellEvents
    {
        private readonly IGitHubRepoService _gitHubRepoService;
        private readonly IContentManager _contentManager;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private readonly IClock _clock;

        public ILogger Logger { get; set; }


        public MarkdownContentUpdaterScheduledTask(IGitHubRepoService gitHubRepoService, IContentManager contentManager, IClock clock, IScheduledTaskManager scheduledTaskManager)
        {
            _gitHubRepoService = gitHubRepoService;
            _contentManager = contentManager;
            _clock = clock;
            _scheduledTaskManager = scheduledTaskManager;

            Logger = NullLogger.Instance;
        }


        public void Process(ScheduledTaskContext context)
        {
            if (!context.Task.TaskType.StartsWith(TaskTypes.MarkdownContentUpdaterPrefix)) return;

            Renew(true, context.Task.ContentItem);

            var markdownRepoContentItem = context.Task.ContentItem;
            if (markdownRepoContentItem == null || markdownRepoContentItem.ContentType != ContentTypes.MarkdownRepo) return;

            var markdownRepoPart = markdownRepoContentItem.As<MarkdownRepoPart>();
            // This is the first run, so we want to create content items from all the md files.
            if (string.IsNullOrEmpty(markdownRepoPart.LatestProcessedCommitHash))
            {
                var initResult = new InitContentItemsResult { };
                if (RepoIsGitHubRepo(markdownRepoPart.RepoUrl))
                {
                    var initContentItemsTask = _gitHubRepoService.InitContentItems(markdownRepoContentItem);
                    initContentItemsTask.Wait();
                    initResult = initContentItemsTask.Result;
                }

                if (initResult.Success)
                {
                    if (!string.IsNullOrEmpty(initResult.LatestProcessedCommitHash))
                    {
                        markdownRepoPart.LatestProcessedCommitHash = initResult.LatestProcessedCommitHash;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(initResult.ErrorMessage))
                    {
                        Logger.Error("Error during initializing repo content items. Message: " + initResult.ErrorMessage);
                    }
                }
            }
            else
            {
                var updateResult = new UpdateContentItemsResult { };
                if (RepoIsGitHubRepo(markdownRepoPart.RepoUrl))
                {
                    var updateContentItemsTask = _gitHubRepoService.UpdateContentItems(markdownRepoContentItem);
                    updateContentItemsTask.Wait();
                    updateResult = updateContentItemsTask.Result;
                }

                if (updateResult.Success)
                {
                    if (!string.IsNullOrEmpty(updateResult.LatestProcessedCommitHash))
                    {
                        markdownRepoPart.LatestProcessedCommitHash = updateResult.LatestProcessedCommitHash;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(updateResult.ErrorMessage))
                    {
                        Logger.Error("Error during updating repo content items. Message: " + updateResult.ErrorMessage);
                    }
                }
            }
        }

        public void Activated()
        {
            // Renewing all tasks.
            foreach (var contentItem in _contentManager.Query(ContentTypes.MarkdownRepo).List())
            {
                Renew(false, contentItem);
            }
        }

        public void Terminating() { }


        private void Renew(bool calledFromTaskProcess, ContentItem contentItem)
        {
            _scheduledTaskManager
                .CreateTaskIfNew(
                    TaskNameHelper.GetMarkdownContentUpdaterTaskName(contentItem),
                    _clock.UtcNow.AddMinutes(Convert.ToDouble(contentItem.As<MarkdownRepoPart>().MinutesBetweenChecks)),
                    contentItem,
                    calledFromTaskProcess);
        }

        private static bool RepoIsGitHubRepo(string repoUrl)
        {
            return !string.IsNullOrEmpty(repoUrl) && repoUrl.Contains(@"https://github.com/");
        }
    }
}