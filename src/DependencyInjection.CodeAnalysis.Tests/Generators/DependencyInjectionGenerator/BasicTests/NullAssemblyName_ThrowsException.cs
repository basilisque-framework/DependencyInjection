﻿/*
   Copyright 2023 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using Microsoft.CodeAnalysis.Testing;

namespace Basilisque.DependencyInjection.CodeAnalysis.Tests.Generators.DependencyInjectionGenerator.BasicTests;

[TestClass]
public class NullAssemblyName_ThrowsException : BaseDependencyInjectionGeneratorTest<NullProjectnameIncrementalSourceGeneratorVerifier>
{
    [TestMethod]
    public override async Task Test()
    {
        var verifier = GetVerifier();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => verifier.RunAsync());
    }

    protected override void AddSourcesUnderTest(SourceFileList sources)
    {
    }
}

