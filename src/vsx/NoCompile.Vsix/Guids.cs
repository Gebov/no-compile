// Guids.cs
// MUST match guids.h
using System;

namespace NoCompile.NoCompile_Vsix
{
    static class GuidList
    {
        public const string guidNoCompile_VsixPkgString = "0ebe3ae7-f991-43da-babd-ee9ea63bede6";
        public const string guidNoCompile_VsixCmdSetString = "1c6cb63d-37a5-4a3a-b50f-e147fc85e14d";
        public const string guidToolWindowPersistanceString = "bae7257b-3b42-4105-ae4e-9b136bb23037";

        public static readonly Guid guidNoCompile_VsixCmdSet = new Guid(guidNoCompile_VsixCmdSetString);
    };
}