using System;
using UnityEngine;

namespace Pantomime.Authoring.So
{
    public interface IPantomimeParams
    {
        public Type GetTriggersType();
        public Type GetFlagsType();

        public Type GeDynamicValuesType();
    }

    public abstract class PantomimeParamsUnified : ScriptableObject, IPantomimeParams
    {
        public virtual Type GetTriggersType()
        {
            throw new NotImplementedException();
        }
        public virtual Type GetFlagsType()
        {
            throw new NotImplementedException();
        }
        public virtual Type GeDynamicValuesType()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class PantomimeParamsEnum<TTriggers, TFlags, TDynamic> : PantomimeParamsUnified where TTriggers : Enum where TFlags : Enum where TDynamic : Enum
    {
        public override Type GetTriggersType()
        {
            return typeof(TTriggers);
        }
        
        public override Type GetFlagsType()
        {
            return typeof(TFlags);
        }

        public override Type GeDynamicValuesType()
        {
            return typeof(TDynamic);
        }


    }
}