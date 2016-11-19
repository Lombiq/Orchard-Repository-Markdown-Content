using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Orchard.ContentManagement;
using Octokit;
using Lombiq.RepositoryMarkdownContent.Models;
using System.Text;
using Orchard.Security;
using Lombiq.RepositoryMarkdownContent.Constants;
using Lombiq.RepositoryMarkdownContent.Models.NonPersistent;
using Lombiq.RepositoryMarkdownContent.Helpers;
using Orchard.Logging;

namespace Lombiq.RepositoryMarkdownContent.Services
{
    public class GitHubRepoService : IGitHubRepoService
    {
        private readonly IMarkdownContentItemManager _markdownContentItemManager;

        public ILogger Logger { get; set; }


        public GitHubRepoService(IMarkdownContentItemManager markdownContentItemManager)
        {
            _markdownContentItemManager = markdownContentItemManager;

            Logger = NullLogger.Instance;
        }


        public async Task<InitContentItemsResult> InitContentItems(ContentItem markdownRepoContentItem)
        {
            var initContentItemsResult = new InitContentItemsResult();

            var markdownRepoPart = markdownRepoContentItem.As<MarkdownRepoPart>();
            if (markdownRepoPart == null)
            {
                initContentItemsResult.ErrorMessage = @"Can't initialize the content items because the repo content item doesn't contain MarkdownRepoPart.";
                return initContentItemsResult;
            }

            var repoOwner = "";
            if (!TryGetRepoOwner(markdownRepoPart.RepoUrl, out repoOwner))
            {
                initContentItemsResult.ErrorMessage = @"Can't initialize the content items because can't get the repo owner from the given GitHub url. Set a vaild GitHub repo url. E.g. https://github.com/username/reponame";
                return initContentItemsResult;
            }

            var repoName = "";
            if (!TryGetRepoName(markdownRepoPart.RepoUrl, out repoName))
            {
                initContentItemsResult.ErrorMessage = @"Can't initialize the content items because can't get the repo name from the given GitHub url. Set a vaild GitHub repo url. E.g. https://github.com/username/reponame";
                return initContentItemsResult;
            }

            try
            {
                var gitHubClient = GetGitHubClient(markdownRepoPart);

                // This is needed after the init, so we'll know what was the latest commit hash.
                var headCommitInBranch = await GetHeadCommit(repoOwner, repoName, markdownRepoPart.BranchName, gitHubClient);

                if (headCommitInBranch == null)
                {
                    initContentItemsResult.ErrorMessage = @"Can't initialize the content items because there's no commit in the given branch: " + markdownRepoPart.BranchName;
                    return initContentItemsResult;
                }

                var treeResponse = await gitHubClient
                    .Git
                    .Tree
                    .GetRecursive(repoOwner, repoName, markdownRepoPart.BranchName);

                var filesInFolder = markdownRepoPart.FolderName == "\\"
                    ? treeResponse.Tree
                    : treeResponse
                        .Tree
                        .Where(t =>
                            t.Path.StartsWith(markdownRepoPart.FolderName) &&
                            t.Path != markdownRepoPart.FolderName &&
                            t.Path.EndsWith(".md"));

                foreach (var treeItem in filesInFolder)
                {
                    var decodedString = await GetFileContentByFileSha(treeItem.Sha, repoOwner, repoName, gitHubClient);

                    _markdownContentItemManager.Create(
                        markdownRepoPart,
                        decodedString,
                        treeItem.Path);
                }

                initContentItemsResult.LatestProcessedCommitHash = headCommitInBranch.Sha;
                initContentItemsResult.Success = true;
                return initContentItemsResult;
            }
            catch (AuthorizationException ex)
            {
                Logger.Error(ex, "Can't initialize the content items because the given GitHub credentials are invalid.");
                throw;
            }
            catch (NotFoundException ex)
            {
                Logger.Error(
                    ex,
                    string.Format(
                        "Can't initialize the content items because can't get the repository with the given data. Repo owner: {0}, repo name: {1}, branch name: {2}.",
                        repoOwner,
                        repoName,
                        markdownRepoPart.BranchName));
                throw;
            }
        }

        public async Task<UpdateContentItemsResult> UpdateContentItems(ContentItem markdownRepoContentItem)
        {
            var updateContentItemsResult = new UpdateContentItemsResult();

            var markdownRepoPart = markdownRepoContentItem.As<MarkdownRepoPart>();
            if (markdownRepoPart == null)
            {
                updateContentItemsResult.ErrorMessage = "Can't update the content items because the repo content item doesn't contain MarkdownRepoPart.";
                return updateContentItemsResult;
            }

            var repoOwner = "";
            if (!TryGetRepoOwner(markdownRepoPart.RepoUrl, out repoOwner))
            {
                updateContentItemsResult.ErrorMessage = @"Can't update the content items because can't get the repo owner from the given GitHub url. Set a vaild GitHub repo url. E.g. https://github.com/username/reponame";
                return updateContentItemsResult;
            }

            var repoName = "";
            if (!TryGetRepoName(markdownRepoPart.RepoUrl, out repoName))
            {
                updateContentItemsResult.ErrorMessage = @"Can't update the content items because can't get the repo name from the given GitHub url. Set a vaild GitHub repo url. E.g. https://github.com/username/reponame";
                return updateContentItemsResult;
            }

            try
            {
                var gitHubClient = GetGitHubClient(markdownRepoPart);

                var headCommitInBranch = await GetHeadCommit(repoOwner, repoName, markdownRepoPart.BranchName, gitHubClient);

                if (headCommitInBranch == null) return updateContentItemsResult;

                // The latest commit is the latest processed commit.
                if (headCommitInBranch.Sha == markdownRepoPart.LatestProcessedCommitHash)
                {
                    updateContentItemsResult.Success = true;
                    return updateContentItemsResult;
                }

                var commits = await gitHubClient
                    .Repository
                    .Commit
                    .GetAll(repoOwner, repoName, new CommitRequest { Sha = markdownRepoPart.BranchName });

                var newCommits = new List<GitHubCommit>();
                foreach (var commit in commits)
                {
                    if (commit.Sha == markdownRepoPart.LatestProcessedCommitHash) break;

                    // So the latest will be the last.
                    newCommits.Insert(0, commit);
                }

                var changedFiles = new List<GitHubCommitFile>();
                foreach (var commit in newCommits)
                {
                    var detailedCommit = await gitHubClient.Repository.Commit.Get(repoOwner, repoName, commit.Sha);

                    foreach (var file in detailedCommit.Files)
                    {
                        if (!MarkdownRepoPartHelpers.FolderOrFileIsInRepoFolder(markdownRepoPart.FolderName, file.Filename)) continue;

                        // If the file was modifyed in a previous commit, then this logic will take care of the status changes.
                        var sameFileThatChangedPreviously = changedFiles
                            .FirstOrDefault(changedFile => changedFile.Sha == file.Sha);
                        if (sameFileThatChangedPreviously != null)
                        {
                            if (file.Status == GitHubCommitStatus.Removed)
                            {
                                if (sameFileThatChangedPreviously.Status == GitHubCommitStatus.Added)
                                {
                                    changedFiles.Remove(sameFileThatChangedPreviously);
                                }
                                else if (sameFileThatChangedPreviously.Status == GitHubCommitStatus.Modified)
                                {
                                    changedFiles.Remove(sameFileThatChangedPreviously);
                                    changedFiles.Add(file);
                                }
                            }
                            else if (file.Status == GitHubCommitStatus.Added)
                            {
                                changedFiles.Remove(sameFileThatChangedPreviously);
                                changedFiles.Add(file);
                            }
                            else if (file.Status == GitHubCommitStatus.Renamed)
                            {
                                changedFiles.Add(file);
                            }

                        }
                        else
                        {
                            changedFiles.Add(file);
                        }
                    }
                }

                foreach (var changedFile in changedFiles)
                {
                    var decodedString = await GetFileContentByFileSha(changedFile.Sha, repoOwner, repoName, gitHubClient);

                    switch (changedFile.Status)
                    {
                        case GitHubCommitStatus.Added:
                            _markdownContentItemManager.Create(
                                markdownRepoPart,
                                decodedString,
                                changedFile.Filename);
                            break;
                        case GitHubCommitStatus.Removed:
                            _markdownContentItemManager.Delete(markdownRepoPart, changedFile.Filename);
                            break;
                        case GitHubCommitStatus.Modified:
                            _markdownContentItemManager.Modify(markdownRepoPart, decodedString, changedFile.Filename);
                            break;
                        case GitHubCommitStatus.Renamed:
                            _markdownContentItemManager.Rename(markdownRepoPart, changedFile.Filename, changedFile.PreviousFileName);
                            _markdownContentItemManager.Modify(markdownRepoPart, decodedString, changedFile.Filename);
                            break;
                    }
                }

                updateContentItemsResult.LatestProcessedCommitHash = headCommitInBranch.Sha;
                updateContentItemsResult.Success = true;
                return updateContentItemsResult;
            }
            catch (AuthorizationException ex)
            {
                Logger.Error(ex, "Can't update the content items because the given GitHub credentials are invalid.");
                throw;
            }
            catch (NotFoundException ex)
            {
                Logger.Error(
                    ex,
                    string.Format(
                        "Can't update the content items because can't get the repository with the given data. Repo owner: {0}, repo name: {1}, branch name: {2}.",
                        repoOwner,
                        repoName,
                        markdownRepoPart.BranchName));
                throw;
            }
        }


        private async Task<string> GetFileContentByFileSha(string fileSha, string repoOwner, string repoName, GitHubClient gitHubClient)
        {
            var blob = await gitHubClient.Git.Blob.Get(repoOwner, repoName, fileSha);
            return Encoding.UTF8.GetString(Convert.FromBase64String(blob.Content));
        }

        private bool TryGetRepoName(string repoUrl, out string repoName)
        {
            return TryGetUrlSegmentFromGitHubUrl(repoUrl, 1, out repoName);
        }

        private bool TryGetRepoOwner(string repoUrl, out string repoOwner)
        {
            return TryGetUrlSegmentFromGitHubUrl(repoUrl, 0, out repoOwner);
        }

        private bool TryGetUrlSegmentFromGitHubUrl(string repoUrl, int segmentIndex, out string urlSegment)
        {
            urlSegment = "";
            if (!repoUrl.StartsWith(@"https://github.com/")) return false;
            var urlSegments = new Uri(repoUrl).Segments.ToList();
            urlSegments.RemoveAll(segment => segment == "/");
            if (urlSegments.Count != 2) return false;

            urlSegment = urlSegments[segmentIndex].Replace("/", "");
            return true;
        }

        private GitHubClient GetGitHubClient(MarkdownRepoPart markdownRepoPart)
        {
            var gitHubClient = new GitHubClient(new ProductHeaderValue("Lombiq.RepositoryMarkdownContent"));
            if (!string.IsNullOrEmpty(markdownRepoPart.AccessToken))
            {
                gitHubClient.Connection.Credentials = new Credentials(markdownRepoPart.AccessToken);
            }
            else if (!string.IsNullOrEmpty(markdownRepoPart.Password) &&
                !string.IsNullOrEmpty(markdownRepoPart.Username))
            {
                gitHubClient.Connection.Credentials = new Credentials(
                    markdownRepoPart.Username,
                    markdownRepoPart.Password);
            }

            return gitHubClient;
        }

        private Task<GitHubCommit> GetHeadCommit(string repoOwner, string repoName, string branchName, GitHubClient gitHubClient)
        {
            return gitHubClient.Repository.Commit.Get(repoOwner, repoName, branchName);
        }
    }
}