// ***********************************************************************
// Copyright (c) 2016 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using Mono.Cecil;

namespace NUnit.Engine.Internal
{
    /// <summary>
    /// Extensions on <see cref="Mono.Cecil.AssemblyDefinition"/>
    /// </summary>
    public static class AssemblyDefinitionExtensions
    {
        public static TargetPlatform GetTargetPlatform(this AssemblyDefinition assemblyDefinition)
        {
            foreach(var attrib in assemblyDefinition.CustomAttributes)
            {
                if (attrib.AttributeType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute")
                {
                    foreach (var prop in attrib.Properties)
                    {
                        if (prop.Name == "FrameworkDisplayName")
                        {
                            return ParseFrameworkName(prop.Argument.Value as string);
                        }
                    }
                }
            }
            // .NET Core does not have the TargetFrameworkAttribute, but it does have AssemblyInformationalVersionAttribute
            // TODO: Is there a better way to identify .NET Core assemblies?
            foreach (var attrib in assemblyDefinition.CustomAttributes)
            {
                if (attrib.AttributeType.FullName == "System.Reflection.AssemblyInformationalVersionAttribute")
                {
                    return TargetPlatform.NetCore;
                }
            }

            // .NET 4.0 and earlier do not have a TargetFrameworkAttribute
            return TargetPlatform.Desktop;
        }

        static TargetPlatform ParseFrameworkName(string frameworkName)
        {
            if (frameworkName == null)
                return TargetPlatform.Unknown;

            if (frameworkName.StartsWith("Xamarin.Android", StringComparison.InvariantCultureIgnoreCase))
                return TargetPlatform.Android;
            if (frameworkName.StartsWith("Xamarin.iOS", StringComparison.InvariantCultureIgnoreCase))
                return TargetPlatform.Ios;
            if (frameworkName.StartsWith(".NET Framework", StringComparison.InvariantCultureIgnoreCase))
                return TargetPlatform.Desktop;
            if (frameworkName.StartsWith(".NET Portable", StringComparison.InvariantCultureIgnoreCase))
                return TargetPlatform.Portable;
            if (frameworkName.StartsWith("Silverlight", StringComparison.InvariantCultureIgnoreCase))
                return TargetPlatform.Silverlight;
            if (frameworkName.StartsWith(".NET for Windows Universal", StringComparison.InvariantCultureIgnoreCase))
                return TargetPlatform.UniversalWindows;
            if (frameworkName.StartsWith(".NET for Windows Store", StringComparison.InvariantCultureIgnoreCase))
                return TargetPlatform.Win81;
            if (frameworkName.StartsWith("Windows Phone 8.0", StringComparison.InvariantCultureIgnoreCase))
                return TargetPlatform.WinPhone80;
            if (frameworkName.StartsWith("Windows Phone 8.1", StringComparison.InvariantCultureIgnoreCase))
                return TargetPlatform.WinPhone81;

            return TargetPlatform.Unknown;
        }
    }
}
