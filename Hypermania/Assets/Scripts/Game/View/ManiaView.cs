using System;
using System.Collections.Generic;
using Game.Sim;
using UnityEngine;
using Utils;

namespace Game.View
{
    [Serializable]
    public struct ManiaViewConfig
    {
        public float Width;
        public float Height;
        public float Border;
        public float Gap;
        public float HitLine;
        public float ScrollSpeed;
        public float NoteHeight;
        public Sprite Background;
        public Sprite Note;
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class ManiaView : MonoBehaviour
    {
        private ManiaViewConfig _config;
        private SpriteRenderer _spriteRenderer;
        private static readonly float[] _channelGapsToCenter = { -0.5f, 0.5f, -1.5f, 1.5f, -2.5f, 2.5f };
        private List<GameObject> _activeNotes;

        public void Init(Vector2 center, in ManiaViewConfig config)
        {
            _config = config;
            transform.position = center;
            _activeNotes = new List<GameObject>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _config.Background;
            gameObject.SetActive(false);
        }

        public void DeInit()
        {
            for (int i = 0; i < _activeNotes.Count; i++)
            {
                Destroy(_activeNotes[i]);
            }
            _activeNotes = null;
            _spriteRenderer.sprite = null;
            _spriteRenderer = null;
        }

        public void Render(Frame frame, in ManiaState state)
        {
            gameObject.SetActive(state.EndFrame != Frame.NullFrame);
            int viewId = 0;
            for (int i = 0; i < state.Config.NumKeys; i++)
            {
                for (int j = 0; j < state.Channels[i].Notes.Count; j++)
                {
                    if (!RenderNote(frame, state.Config.NumKeys, i, viewId, state.Channels[i].Notes[j]))
                    {
                        break;
                    }
                    viewId++;
                }
            }

            // if we rendered fewer notes this frame compared to last, set the entity to not active
            for (int i = viewId; i < _activeNotes.Count; i++)
            {
                _activeNotes[i].SetActive(false);
            }
        }

        private bool RenderNote(Frame frame, int numChannels, int channel, int viewId, in ManiaNote note)
        {
            // should only add a single new note to the view
            while (_activeNotes.Count <= viewId)
            {
                GameObject noteView = new GameObject("Mania Note");
                noteView.transform.SetParent(transform);
                SpriteRenderer sp = noteView.AddComponent<SpriteRenderer>();
                sp.sprite = _config.Note;
                _activeNotes.Add(noteView);
            }
            GameObject view = _activeNotes[viewId];
            view.SetActive(true);

            float x = ChannelX(numChannels, channel);
            float y = (note.Tick - frame) * _config.ScrollSpeed + _config.HitLine - _config.Height / 2;
            if (y > _config.Height / 2 - _config.NoteHeight / 2)
            {
                return false;
            }
            view.transform.SetLocalPositionAndRotation(new Vector3(x, y, -1), Quaternion.identity);
            return true;
        }

        private float ChannelWidth(int numChannels) =>
            (_config.Width - _config.Gap * (numChannels - 1) - 2 * _config.Border) / numChannels;

        private float ChannelX(int numChannels, int channel) =>
            _channelGapsToCenter[channel] * (ChannelWidth(numChannels) + _config.Gap);
    }
}
