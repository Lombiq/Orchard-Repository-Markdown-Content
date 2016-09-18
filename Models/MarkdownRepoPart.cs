using Lombiq.RepositoryMarkdownContent.Constants;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using Orchard.Core.Common.Fields;
using Orchard.Data.Conventions;
using Orchard.Fields.Fields;
using Piedone.HelpfulLibraries.Contents;
using System.ComponentModel.DataAnnotations;

namespace Lombiq.RepositoryMarkdownContent.Models
{
    public class MarkdownRepoPart : ContentPart<MarkdownRepoPartRecord>
    {
        public string LatestProcessedCommitHash
        {
            get { return this.Retrieve(x => x.LatestProcessedCommitHash); }
            set { this.Store(x => x.LatestProcessedCommitHash, value); }
        }

        public string ContentType
        {
            get { return this.Retrieve(x => x.ContentType); }
            set { this.Store(x => x.ContentType, value); }
        }

        public string Username
        {
            get { return Retrieve(x => x.Username); }
            set { Store(x => x.Username, value); }
        }

        public string EncodedPassword
        {
            get { return Retrieve(x => x.EncodedPassword); }
            set { Store(x => x.EncodedPassword, value); }
        }

        private readonly LazyField<string> _password = new LazyField<string>();
        internal LazyField<string> PasswordField { get { return _password; } }
        public string Password { get { return _password.Value; } set { _password.Value = value; } }

        public string EncodedAccessToken
        {
            get { return Retrieve(x => x.EncodedAccessToken); }
            set { Store(x => x.EncodedAccessToken, value); }
        }

        private readonly LazyField<string> _accessToken = new LazyField<string>();
        internal LazyField<string> AccessTokenField { get { return _accessToken; } }
        public string AccessToken { get { return _accessToken.Value; } set { _accessToken.Value = value; } }

        public decimal? MinutesBetweenChecks
        {
            get
            {
                return this.AsField<NumericField>(
                    typeof(MarkdownRepoPart).Name,
                    FieldNames.MinutesBetweenChecks).Value;
            }
        }

        public string RepoUrl
        {
            get
            {
                return this.AsField<TextField>(
                    typeof(MarkdownRepoPart).Name,
                    FieldNames.RepoUrl).Value;
            }

            set
            {
                if (value != null)
                    this.AsField<TextField>(
                        typeof(MarkdownRepoPart).Name,
                        FieldNames.RepoUrl).Value = value;
            }
        }

        public string FolderName
        {
            get
            {
                return this.AsField<TextField>(
                    typeof(MarkdownRepoPart).Name,
                    FieldNames.FolderName).Value;
            }
        }

        public string BranchName
        {
            get
            {
                return this.AsField<TextField>(
                    typeof(MarkdownRepoPart).Name,
                    FieldNames.BranchName).Value;
            }
        }

        public bool? DeleteMarkdownPagesOnRemoving
        {
            get
            {
                return this.AsField<BooleanField>(
                    typeof(MarkdownRepoPart).Name,
                    FieldNames.DeleteMarkdownPagesOnRemoving).Value;
            }
        }
    }

    public class MarkdownRepoPartRecord : ContentPartRecord
    {
        public virtual string Username { get; set; }
        [DataType(DataType.Password), StringLengthMax]
        public virtual string EncodedPassword { get; set; }
        [DataType(DataType.Password), StringLengthMax]
        public virtual string EncodedAccessToken { get; set; }
    }
}