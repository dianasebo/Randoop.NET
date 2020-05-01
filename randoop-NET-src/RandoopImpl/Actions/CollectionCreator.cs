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




using Common;
using System;

namespace Randoop
{

    public abstract class ArrayOrArrayListBuilderTransformer : Transformer
    {
        public readonly Type baseType;
        public readonly int length;


        public override string MemberName
        {
            get
            {
                return "n/a";
            }
        }

        public override Type[] ParameterTypes
        {
            get
            {
                Type[] retval = new Type[length];
                for (int i = 0; i < length; i++)
                    retval[i] = baseType;
                return retval;
            }
        }

        protected ArrayOrArrayListBuilderTransformer(Type arrayBaseType, int arrayLength)
        {
            Util.Assert(arrayBaseType != null && arrayLength >= 0);
            this.baseType = arrayBaseType;
            this.length = arrayLength;
        }
    }

}
