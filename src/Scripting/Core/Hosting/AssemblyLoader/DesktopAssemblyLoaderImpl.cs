﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting.Hosting
{
    internal sealed class DesktopAssemblyLoaderImpl : AssemblyLoaderImpl
    {
        private readonly Func<string, Assembly, Assembly> _assemblyResolveHandlerOpt;

        public DesktopAssemblyLoaderImpl(InteractiveAssemblyLoader loader)
            : base(loader)
        {
            _assemblyResolveHandlerOpt = loader.ResolveAssembly;
            CoreLightup.Desktop.AddAssemblyResolveHandler(_assemblyResolveHandlerOpt);
        }

        public override void Dispose()
        {
            if (_assemblyResolveHandlerOpt != null)
            {
                CoreLightup.Desktop.RemoveAssemblyResolveHandler(_assemblyResolveHandlerOpt);
            }
        }

        public override Assembly LoadFromStream(Stream peStream, Stream pdbStream)
        {
            byte[] peImage = new byte[peStream.Length];
            peStream.TryReadAll(peImage, 0, peImage.Length);

            if (pdbStream != null)
            {
                byte[] pdbImage = new byte[pdbStream.Length];
                pdbStream.TryReadAll(pdbImage, 0, pdbImage.Length);

                return CoreLightup.Desktop.LoadAssembly(peImage, pdbImage);
            }

            return CoreLightup.Desktop.LoadAssembly(peImage);
        }

        public override AssemblyAndLocation LoadFromPath(string path)
        {
            // An assembly is loaded into CLR's Load Context if it is in the GAC, otherwise it's loaded into No Context via Assembly.LoadFile(string).
            // Assembly.LoadFile(string) automatically redirects to GAC if the assembly has a strong name and there is an equivalent assembly in GAC. 

            var assembly = Assembly.LoadFile(path);
            var location = assembly.Location;
            var fromGac = CoreLightup.Desktop.IsAssemblyFromGlobalAssemblyCache(assembly);
            return new AssemblyAndLocation(assembly, location, fromGac);
        }
    }
}
