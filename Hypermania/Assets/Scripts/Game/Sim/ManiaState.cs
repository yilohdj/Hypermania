using System.Collections.Generic;
using MemoryPack;
using Utils;
using Utils.SoftFloat;

namespace Game.Sim
{
    [MemoryPackable]
    public partial struct ManiaNote
    {
        public int Id;
        public Frame Tick;
        public int Length;
    }

    [MemoryPackable]
    public partial struct ManiaNoteChannel
    {
        public int NumNotes;
        public ManiaNote[] Notes;
    }

    public enum ManiaEventKind
    {
        Hit,
        Missed,
    }

    [MemoryPackable]
    public partial struct ManiaEvent
    {
        public ManiaEventKind Kind;

        /// <summary>
        /// If the note was hit, how many ticks away from the exact timing it was hit
        /// </summary>
        public int Offset;

        /// <summary>
        /// If the note was missed, if it was hit early or late
        /// </summary>
        public bool Early;

        public ManiaEvent HitEvent(int offset)
        {
            return new ManiaEvent { Offset = offset, Kind = ManiaEventKind.Hit };
        }

        public ManiaEvent MissEvent(bool early)
        {
            return new ManiaEvent { Early = early, Kind = ManiaEventKind.Missed };
        }
    }

    [MemoryPackable]
    public partial struct ManiaConfig
    {
        public int NumKeys;

        /// <summary>
        /// The input must be within this number of ticks of the note to score a good note. Note that this means the
        /// total number of ticks in which the user can get a perfect is 2 * GoodHRange + 1. There is no config for
        /// perfect notes: they are only awarded if the note was hit on the frame exactly.
        /// </summary>
        public int GoodHRange;

        /// <summary>
        /// This should be the additional number of frames outside of the good range that would score an ok hit.
        /// </summary>
        public int OkHRange;
        public int BadHRange;

        /// <summary>
        /// This range is used to guard against accidental key presses: the user should not be able to miss a note that
        /// is eons away. However this value should be > 0 as otherwise the user could just spam notes.
        /// </summary>
        public int MissHRange;

        public int OkTotalRange => GoodHRange + OkHRange;
        public int BadTotalRange => GoodHRange + OkHRange + BadHRange;
        public int MissTotalRange => GoodHRange + OkHRange + BadHRange + MissHRange;
    }

    [MemoryPackable]
    public partial struct ManiaSim
    {
        public const int MAX_NOTES = 200;
        private ManiaConfig _config;
        private ManiaNoteChannel[] _notes;
        private static readonly InputFlags[] _channelInput =
        {
            InputFlags.Mania1,
            InputFlags.Mania2,
            InputFlags.Mania3,
            InputFlags.Mania4,
            InputFlags.Mania5,
            InputFlags.Mania6,
        };

        public static ManiaSim Create(in ManiaConfig config)
        {
            ManiaSim sim = new ManiaSim();
            sim._config = config;
            sim._notes = new ManiaNoteChannel[config.NumKeys];
            for (int i = 0; i < config.NumKeys; i++)
            {
                sim._notes[i] = new ManiaNoteChannel { Notes = new ManiaNote[MAX_NOTES], NumNotes = 0 };
            }
            return sim;
        }

        public void Tick(Frame frame, GameInput input, List<ManiaEvent> outEvents)
        {
            for (int i = 0; i < _notes.Length; i++)
            {
                if (_notes[i].NumNotes == 0)
                {
                    continue;
                }
                if (!input.Flags.HasFlag(_channelInput[i]))
                {
                    // no input
                    continue;
                }
                Frame noteTick = _notes[i].Notes[0].Tick;
                if (frame < noteTick - _config.MissTotalRange || frame > noteTick + _config.MissTotalRange)
                {
                    // too far away to hit it
                    continue;
                }

                if (frame < noteTick - _config.BadTotalRange) { }
            }
        }
    }
}
