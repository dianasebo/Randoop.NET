//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Apache License, Version 2.0.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

namespace RandoopTests
{
    /// <summary>
    /// Run all randoop tests.
    /// </summary>
    class RandoopTests
    {
        public static void Main()
        {
            SimpleMatcherTests.Test();
            SourceCodeGenerationTests.Test();
        }
    }
}
