﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.Extensibility;
using Inedo.Extensibility.Credentials;
using Inedo.Extensibility.Operations;
using Inedo.Extensions.SuggestionProviders;
using Inedo.IO;
using Inedo.Web;
using Inedo.Web.Plans.ArgumentEditors;

namespace Inedo.Extensions.Operations.ProGet
{
    [DisplayName("Push Package")]
    [Description("Uploads a zip file containing the contents of a Universal Package to a ProGet feed.")]
    [ScriptAlias("Push-Package")]
    [ScriptNamespace(Namespaces.ProGet)]
    [Tag("ProGet")]
    [Serializable]
#pragma warning disable CS0618 // Type or member is obsolete
    public sealed class PushPackageOperation : RemoteExecuteOperation, IHasCredentials<ProGetCredentials>
#pragma warning restore CS0618 // Type or member is obsolete
        , IHasCredentials<InedoProductCredentials>
    {
        [ScriptAlias("Credentials")]
        [DisplayName("Credentials")]
        public string CredentialName { get; set; }

        [Required]
        [ScriptAlias("Feed")]
        [DisplayName("Feed name")]
        [SuggestableValue(typeof(FeedNameSuggestionProvider))]
        public string FeedName { get; set; }

        [Required]
        [ScriptAlias("FilePath")]
        [DisplayName("Package file path")]
        [FilePathEditor(IncludeFiles = true)]
        public string FilePath { get; set; }

        [ScriptAlias("Group")]
        [DisplayName("Group name")]
        [PlaceholderText("Ungrouped")]
        public string Group { get; set; }

        [ScriptAlias("Name")]
        [DisplayName("Package name")]
        [PlaceholderText("$ApplicationName")]
        [DefaultValue("$ApplicationName")]
        public string Name { get; set; }

        [ScriptAlias("Version")]
        [PlaceholderText("$ReleaseNumber")]
        [DefaultValue("$ReleaseNumber")]
        public string Version { get; set; }

        [ScriptAlias("Description")]
        [FieldEditMode(FieldEditMode.Multiline)]
        [Description("The package description supports Markdown syntax.")]
        public string Description { get; set; }

        [Category("Advanced")]
        [ScriptAlias("Title")]
        public string Title { get; set; }
        [Category("Advanced")]
        [ScriptAlias("Icon")]
        [Description("A string of an absolute url pointing to an image to be displayed in the ProGet UI (at both 64px and 128px); if  package:// is used as the protocol, ProGet will search within the package and serve that image instead")]
        public string Icon { get; set; }
        [Category("Advanced")]
        [ScriptAlias("Dependencies")]
        [FieldEditMode(FieldEditMode.Multiline)]
        [Description(@"Dependencies should be supplied as a list, each consisting of a package identification string; this string is formatted as follows:
                    <ul>
                        <li>«group»:«package-name»</li>
                        <li>«group»:«package-name»:«version»</li>
                    </ul>
                    When the version is not specified, the latest is used.")]
        public IEnumerable<string> Dependencies { get; set; }

        [Category("Connection/Identity")]
        [ScriptAlias("Server")]
        [ScriptAlias("FeedUrl")]
        [DisplayName("ProGet server URL")]
        [PlaceholderText("Use server URL from credential")]
#pragma warning disable CS0618 // Type or member is obsolete
        [MappedCredential(nameof(ProGetCredentials.Url))]
#pragma warning restore CS0618 // Type or member is obsolete
        public string Server { get; set; }

        [Category("Connection/Identity")]
        [ScriptAlias("UserName")]
        [DisplayName("ProGet user name")]
        [Description("The name of a user in ProGet that can access the specified feed.")]
        [PlaceholderText("Use user name from credential")]
#pragma warning disable CS0618 // Type or member is obsolete
        [MappedCredential(nameof(ProGetCredentials.UserName))]
#pragma warning restore CS0618 // Type or member is obsolete
        public string UserName { get; set; }

        [Category("Connection/Identity")]
        [ScriptAlias("Password")]
        [DisplayName("ProGet password")]
        [Description("The password of a user in ProGet that can access the specified feed.")]
        [PlaceholderText("Use password from credential")]
#pragma warning disable CS0618 // Type or member is obsolete
        [MappedCredential(nameof(ProGetCredentials.Password))]
#pragma warning restore CS0618 // Type or member is obsolete
        public string Password { get; set; }

        protected override async Task<object> RemoteExecuteAsync(IRemoteOperationExecutionContext context)
        {
            var client = new ProGetClient(this.Server, this.FeedName, this.UserName, this.Password, this, context.CancellationToken);

            try
            {
                this.LogInformation($"Pushing package {this.Name} to ProGet...");

                string path = context.ResolvePath(this.FilePath);

                this.LogDebug("Using package file: " + path);

                if (!FileEx.Exists(path))
                {
                    this.LogError(this.FilePath + " does not exist.");
                    return null;
                }

                using (var file = FileEx.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var data = new ProGetPackagePushData
                    {
                        Title = this.Title,
                        Description = this.Description,
                        Icon = this.Icon,
                        Dependencies = this.Dependencies?.ToArray()
                    };

                    await client.PushPackageAsync(this.Group, this.Name, this.Version, data, file).ConfigureAwait(false);
                }
            }
            catch (ProGetException ex)
            {
                this.LogError(ex.FullMessage);
                return null;
            }

            this.LogInformation("Package pushed.");
            return null;
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(
                new RichDescription("Push ", new Hilite(config[nameof(Name)]), " Package"),
                new RichDescription("to ProGet feed ", config[nameof(Server)])
            );
        }
    }
}
