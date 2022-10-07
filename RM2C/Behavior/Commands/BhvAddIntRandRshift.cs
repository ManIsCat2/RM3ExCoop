﻿namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvAddIntRandRshift : BehaviorCommand
    {
        // Gets a random short, right shifts it the specified amount, and adds min to it, then adds the value to the specified field. Unused.
        public BhvAddIntRandRshift() : base(0x17, "ADD_INT_RAND_RSHIFT", BehaviorParamType.FIELD) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte field = bin.ReadByte();
            ushort min = bin.ReadUInt16();
            ushort rshift = bin.ReadUInt16();
            _ = bin.ReadUInt16();

            return new dynamic[] { field, min, rshift };
        }
    }
}
