using System;
using Game.Sim;
using Netcode.Rollback;
using UnityEngine;
using Utils;

namespace Game
{
    public class GameState : IState<GameState>
    {
        public Frame Frame;
        public FighterState[] Fighters;
        // public HitboxState[] Hitboxes;     
        // public ProjectileState[] Projectiles; 

        public static GameState New()
        {
            GameState state = new GameState();
            state.Frame = Frame.FirstFrame;
            state.Fighters[0] = new FighterState(new Vector2(-7, -4.5f), 7f, Vector2.right);
            state.Fighters[1] = new FighterState(new Vector2(7, -4.5f), 7f, Vector2.left);
            return state;
        }

        public void Advance((Input input, InputStatus status)[] inputs)
        {
            Fighters[0].ApplyInputs(inputs[0].input);
            Fighters[1].ApplyInputs(inputs[1].input);
            Frame += 1;
        }

        public int Deserialize(ReadOnlySpan<byte> inBytes)
        {
            int ptr = 0;
            ptr += Frame.Deserialize(inBytes[ptr..]);
            ptr += Fighters[0].Deserialize(inBytes[ptr..]);
            ptr += Fighters[1].Deserialize(inBytes[ptr..]);
            return ptr;
        }
        public int Serialize(Span<byte> outBytes)
        {
            int ptr = 0;
            ptr += Frame.Serialize(outBytes[ptr..]);
            ptr += Fighters[0].Serialize(outBytes[ptr..]);
            ptr += Fighters[1].Serialize(outBytes[ptr..]);
            return ptr;
        }
        public int SerdeSize()
        {
            int cnt = 0;
            cnt += Frame.SerdeSize();
            return cnt;
        }
        public ulong Checksum()
        {
            // TODO
            return 0;
        }
    }
}