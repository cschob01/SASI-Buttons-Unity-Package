using System;
using UnityEngine;

namespace Unity.SASIButtons
{
    [Serializable]
    public class SASIButtons_Call
    {
        public string typeName;
        public string methodName;

        public bool isSingleton;

        [SerializeReference]
        public SASI_Parameter param;
    }

    [Serializable]
    public abstract class SASI_Parameter
    {
        public abstract object GetValue();
    }

    [Serializable]
    public class SASI_Parameter<T> : SASI_Parameter
    {
        [SerializeReference]
        public T value;

        public override object GetValue()
        {
            return value;
        }
    }
}