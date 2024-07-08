using Unity.Entities;

namespace Pantomime.Aspects
{
    public readonly partial struct PantomimeAspect : IAspect
    {
        private readonly DynamicBuffer<PantomimeTriggerElement> _triggerElements;
        private readonly RefRW<PantomimeFlags> _flags;

        public void Trigger(int trigger, float duration = 0f)
        {
            if (trigger < 0) return;
            _triggerElements.Add(
                new PantomimeTriggerElement()
                {
                    type = trigger,
                    fixedDuration = duration,
                });
        }

        public void Set(int flag)
                 {
                     var flags = _flags.ValueRO.flags;
                     flags |= (uint)flag;
                     _flags.ValueRW.flags = flags;
                 }
         
                 public void UnSet(int flag)
                 {
                     var flags = _flags.ValueRO.flags;
                     flags &= ~(uint)flag;
                     _flags.ValueRW.flags = flags;
                 }
    }
}