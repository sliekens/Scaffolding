// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Spectre.Console.Cli;

namespace Microsoft.DotNet.Tools.Scaffold
{
    public class Program
    {
        private const string SCAFFOLD_COMMAND = "scaffold";
        private const string AREA_COMMAND = "--area";
        private const string CONTROLLER_COMMAND = "--controller";
        private const string IDENTITY_COMMAND = "--identity";
        private const string RAZORPAGE_COMMAND = "--razorpage";
        private const string VIEW_COMMAND = "--view";
        /*
        dotnet scaffold [generator] [-p|--project] [-n|--nuget-package-dir] [-c|--configuration] [-tfm|--target-framework] [-b|--build-base-path] [--no-build]

        This commands supports the following generators :
            Area
            Controller
            Identity
            Razorpage
            View

        e.g: dotnet scaffold area <AreaNameToGenerate>
             dotnet scaffold identity
             dotnet scaffold razorpage

        */

        public static int Main(string[] args)
        {
            var rootCommand = ScaffoldCommand();

            rootCommand.AddCommand(ScaffoldAreaCommand());
            rootCommand.AddCommand(ScaffoldControllerCommand());
            rootCommand.AddCommand(ScaffoldRazorPageCommand());
            rootCommand.AddCommand(ScaffoldViewCommand());
            rootCommand.AddCommand(ScaffoldIdentityCommand());
            //msidentity commands
            //new BinderBase for System.Commandline update, new way to bind handlers to commands.
            var provisioningToolBinder = new ProvisioningToolOptionsBinder(
                MsIdentity.JsonOption,
                MsIdentity.EnableIdTokenOption,
                MsIdentity.EnableAccessToken,
                MsIdentity.CallsGraphOption,
                MsIdentity.CallsDownstreamApiOption,
                MsIdentity.UpdateUserSecretsOption,
                MsIdentity.ConfigUpdateOption,
                MsIdentity.CodeUpdateOption,
                MsIdentity.PackagesUpdateOption,
                MsIdentity.ClientIdOption,
                MsIdentity.AppDisplayName,
                MsIdentity.ProjectType,
                MsIdentity.ClientSecretOption,
                MsIdentity.RedirectUriOption,
                MsIdentity.ProjectFilePathOption,
                MsIdentity.ClientProjectOption,
                MsIdentity.ApiScopesOption,
                MsIdentity.HostedAppIdUriOption,
                MsIdentity.ApiClientIdOption,
                MsIdentity.SusiPolicyIdOption,
                MsIdentity.TenantOption,
                MsIdentity.UsernameOption);

            //internal commands
            var listAadAppsCommand = MsIdentity.ListAADAppsCommand();
            var listServicePrincipalsCommand = MsIdentity.ListServicePrincipalsCommand();
            var listTenantsCommand = MsIdentity.ListTenantsCommand();
            var createClientSecretCommand = MsIdentity.CreateClientSecretCommand();

            //exposed commands
            var registerApplicationCommand = MsIdentity.RegisterApplicationCommand();
            var unregisterApplicationCommand = MsIdentity.UnregisterApplicationCommand();
            var updateAppRegistrationCommand = MsIdentity.UpdateAppRegistrationCommand();
            var updateProjectCommand = MsIdentity.UpdateProjectCommand();
            var createAppRegistration = MsIdentity.CreateAppRegistrationCommand();

            //hide internal commands.
            listAadAppsCommand.IsHidden = true;
            listServicePrincipalsCommand.IsHidden = true;
            listTenantsCommand.IsHidden = true;
            updateProjectCommand.IsHidden = true;
            createClientSecretCommand.IsHidden = true;

            listAadAppsCommand.SetHandler(MsIdentity.HandleListApps, provisioningToolBinder);
            listServicePrincipalsCommand.SetHandler(MsIdentity.HandleListServicePrincipals, provisioningToolBinder);
            listTenantsCommand.SetHandler(MsIdentity.HandleListTenants, provisioningToolBinder);
            registerApplicationCommand.SetHandler(MsIdentity.HandleRegisterApplication, provisioningToolBinder);
            unregisterApplicationCommand.SetHandler(MsIdentity.HandleUnregisterApplication, provisioningToolBinder);
            updateAppRegistrationCommand.SetHandler(MsIdentity.HandleUpdateApplication, provisioningToolBinder);
            updateProjectCommand.SetHandler(MsIdentity.HandleUpdateProject, provisioningToolBinder);
            createClientSecretCommand.SetHandler(MsIdentity.HandleClientSecrets, provisioningToolBinder);
            createAppRegistration.SetHandler(MsIdentity.HandleCreateAppRegistration, provisioningToolBinder);

            rootCommand.AddCommand(listAadAppsCommand);
            rootCommand.AddCommand(listServicePrincipalsCommand);
            rootCommand.AddCommand(listTenantsCommand);
            rootCommand.AddCommand(registerApplicationCommand);
            rootCommand.AddCommand(unregisterApplicationCommand);
            rootCommand.AddCommand(createAppRegistration);
            rootCommand.AddCommand(updateAppRegistrationCommand);
            rootCommand.AddCommand(updateProjectCommand);
            rootCommand.AddCommand(createClientSecretCommand);

            rootCommand.Description = "dotnet scaffold [command] [-p|--project] [-n|--nuget-package-dir] [-c|--configuration] [-tfm|--target-framework] [-b|--build-base-path] [--no-build] ";
            if (args.Length == 0)
            {
                // config.AddCommand<AddCommand>("add");
                // config.AddCommand<CommitCommand>("commit");
                // config.AddCommand<RebaseCommand>("rebase");
            });
            //default commands are going to be { install, uninstall, command }
            //command will have the default scaffolders { area, controller, minimalapi, identity, razorpage, view }
            //find what if its install or uninstall
            //
            return 0;
        }

        private static object LoadCommands()
        {
            throw new NotImplementedException();
        }
    }
}
