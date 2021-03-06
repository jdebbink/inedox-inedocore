﻿using System.ComponentModel;
using System.Linq;
using Inedo.Documentation;
using Inedo.Extensibility;

namespace Inedo.Extensions.VariableFunctions.Server
{
    [ScriptAlias("RoleName")]
    [Description("name of the current server role in context")]
    [Tag("servers")]
    [AppliesTo(InedoProduct.BuildMaster | InedoProduct.Hedgehog | InedoProduct.Otter)]
    public sealed class ServerRoleNameVariableFunction : CommonScalarVariableFunction
    {
        protected override object EvaluateScalar(object context)
        {
            int? roleId = (context as IStandardContext)?.ServerRoleId;
            if (roleId != null)
                return SDK.GetServerRoles().FirstOrDefault(s => s.Id == roleId)?.Name;
            else
                return string.Empty;
        }
    }
}