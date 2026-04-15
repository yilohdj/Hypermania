using System.Collections.Generic;
using Design.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scenes.Menus.CharacterSelect
{
    /// <summary>
    /// One panel per player slot (P1 left/red, P2 right/blue). Watches its
    /// slot in the shared state and runs its own Phase/Character/Skin
    /// transitions — the directory never pushes updates onto it.
    /// </summary>
    public class PlayerPanel : MonoBehaviour
    {
        [SerializeField]
        private int _playerIndex;

        [Header("Preview elements (visible during Character phase)")]
        [SerializeField]
        private GameObject _previewRoot;

        [FormerlySerializedAs("_portrait")] [SerializeField]
        private Image _splash;

        [SerializeField]
        private TMP_Text _characterName;

        [Header("Preview pulse (while slot is in Character phase)")]
        [SerializeField]
        [Tooltip("Transform scaled between 1 and _previewPulseMaxScale while the player is still hovering (Character phase).")]
        private RectTransform _previewPulseTarget;

        [SerializeField]
        [Tooltip("CanvasGroup whose alpha oscillates between _previewPulseMinAlpha and 1 while the player is still hovering.")]
        private CanvasGroup _previewPulseGroup;

        [SerializeField]
        private float _previewPulseSpeed = 3f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _previewPulseMinAlpha = 0.7f;

        [SerializeField]
        private float _previewPulseMaxScale = 1.05f;

        [SerializeField]
        private Image[] _mainImages;

        [SerializeField]
        private Image[] _lightImages;

        [SerializeField]
        private Image[] _accentImages;

        [Header("Options panel (visible during Options / Confirmed phases)")]
        [SerializeField]
        private PlayerOptionsPanel _optionsPanel;

        [SerializeField]
        private FadeToggle _optionsFade;

        [SerializeField]
        private FadeToggle _confirmedBanner;

        [Header("Shared fighter stage (both panels point at the same stage component)")]
        [SerializeField]
        private CharacterFighterStage _fighterStage;

        private PlayerSelectionState _state;
        private CharacterConfig[] _roster;
        private SkinConfig _randomSkin;
        private bool _isLocal;

        private int _lastCharIndex = -1;
        private int _lastSkinIndex = -1;
        private SelectPhase _lastPhase = SelectPhase.Confirmed;

        // Portrait sprites are lazily wrapped from SkinConfig.Portrait (Texture2D)
        // and cached so we don't leak a fresh Sprite on every character/skin change.
        private readonly Dictionary<Texture2D, Sprite> _splashSprites = new();

        public int PlayerIndex => _playerIndex;
        public PlayerOptionsPanel OptionsPanel => _optionsPanel;

        public void Bind(
            PlayerSelectionState state,
            CharacterConfig[] roster,
            ControlsConfig[] controlsPresets,
            SkinConfig randomSkin,
            bool isLocal
        )
        {
            _state = state;
            _roster = roster;
            _randomSkin = randomSkin;
            _isLocal = isLocal;
            _lastCharIndex = -1;
            _lastSkinIndex = -1;

            if (_optionsPanel != null)
            {
                _optionsPanel.Bind(state, roster, controlsPresets, isLocal);
            }

            // Snap fade state to the initial phase so we don't fade in/out
            // from the scene's serialized default on first frame.
            if (_optionsFade != null)
                _optionsFade.SnapTo(_state.Phase == SelectPhase.Options);
            if (_confirmedBanner != null)
                _confirmedBanner.SnapTo(_state.Phase == SelectPhase.Confirmed);
            _lastPhase = _state.Phase;
        }

        private void Update()
        {
            if (_state == null || _roster == null || _roster.Length == 0)
                return;

            if (_state.Phase != _lastPhase)
            {
                ApplyPhaseVisibility(_state.Phase);
                _lastPhase = _state.Phase;
            }

            if (_state.CharacterIndex != _lastCharIndex || _state.SkinIndex != _lastSkinIndex)
            {
                ApplyCharacterPreview();
                _lastCharIndex = _state.CharacterIndex;
                _lastSkinIndex = _state.SkinIndex;
            }

            ApplyPreviewPulse();
            ApplyFighterPreview();
        }

        /// <summary>
        /// Publish this panel's desired fighter state to the shared stage
        /// each frame. The stage is idempotent, so we don't cache anything
        /// here — the slot state on the stage is driven purely by
        /// <see cref="PlayerSelectionState"/>.
        /// </summary>
        private void ApplyFighterPreview()
        {
            if (_fighterStage == null)
                return;

            bool onRandom = _state.CharacterIndex >= _roster.Length;
            bool visible = !onRandom && _state.Phase != SelectPhase.Character;
            CharacterConfig config = visible
                ? _roster[Mathf.Clamp(_state.CharacterIndex, 0, _roster.Length - 1)]
                : null;

            _fighterStage.Render(_playerIndex, config, _state.SkinIndex, visible);
        }

        /// <summary>
        /// While the slot is still in the Character phase (player hasn't
        /// locked in), pulse the preview's alpha and scale so it reads as
        /// "still hovering." Once the player confirms out of Character,
        /// snap both back to their rest values.
        /// </summary>
        private void ApplyPreviewPulse()
        {
            bool pulsing = _state.Phase == SelectPhase.Character;
            float alpha;
            float scale;
            if (pulsing)
            {
                // Map a sine wave to [0, 1] so scale stays at or above 1
                // and alpha stays above the floor. Alpha is inverted
                // against scale so the preview is most transparent at the
                // peak of the pulse (most "zoomed out") and fully opaque
                // at rest scale.
                float t = Mathf.Sin(Time.time * _previewPulseSpeed) * 0.5f + 0.5f;
                scale = Mathf.Lerp(1f, _previewPulseMaxScale, t);
                alpha = Mathf.Lerp(1f, _previewPulseMinAlpha, t);
            }
            else
            {
                alpha = 1f;
                scale = 1f;
            }

            if (_previewPulseGroup != null)
                _previewPulseGroup.alpha = alpha;
            if (_previewPulseTarget != null)
                _previewPulseTarget.localScale = new Vector3(scale, scale, 1f);
        }

        private void ApplyPhaseVisibility(SelectPhase phase)
        {
            if (_optionsFade != null)
            {
                _optionsFade.SetVisible(phase == SelectPhase.Options);
            }
            if (_confirmedBanner != null)
            {
                _confirmedBanner.SetVisible(phase == SelectPhase.Confirmed);
            }
            if (_previewRoot != null)
            {
                _previewRoot.SetActive(true); // preview stays visible in all phases as backdrop
            }
        }

        private void ApplyCharacterPreview()
        {
            // CharacterIndex == _roster.Length indicates the Random grid slot
            // — the concrete character hasn't been rolled yet. Paint the
            // panel with the shared Random skin (same asset the grid uses
            // for the Random tile) so the preview, tints, and ready marker
            // stay visually consistent while the player is on Random.
            if (_state.CharacterIndex >= _roster.Length)
            {
                if (_characterName != null)
                    _characterName.text = "Random";
                if (_splash != null)
                {
                    Sprite sprite = GetOrCreateSprite(_randomSkin.Splash);
                    _splash.sprite = sprite;
                    _splash.enabled = sprite != null;
                }
                ApplyTint(_mainImages, _randomSkin.MainColor);
                ApplyTint(_lightImages, _randomSkin.LightColor);
                ApplyTint(_accentImages, _randomSkin.AccentColor);
                return;
            }

            int charIdx = Mathf.Clamp(_state.CharacterIndex, 0, _roster.Length - 1);
            CharacterConfig config = _roster[charIdx];
            if (config == null)
                return;

            if (_characterName != null)
            {
                _characterName.text = config.Character.ToString();
            }

            SkinConfig skin = default;
            bool hasSkin = config.Skins != null && config.Skins.Length > 0;
            if (hasSkin)
            {
                int skinIdx = Mathf.Clamp(_state.SkinIndex, 0, config.Skins.Length - 1);
                skin = config.Skins[skinIdx];
            }

            if (_splash != null)
            {
                Sprite sprite = hasSkin ? GetOrCreateSprite(skin.Splash) : null;
                _splash.sprite = sprite;
                _splash.enabled = sprite != null;
            }

            if (hasSkin)
            {
                ApplyTint(_mainImages, skin.MainColor);
                ApplyTint(_lightImages, skin.LightColor);
                ApplyTint(_accentImages, skin.AccentColor);
            }
        }

        private static void ApplyTint(Image[] images, Color color)
        {
            if (images == null)
                return;
            foreach (Image img in images)
            {
                if (img != null)
                    img.color = color;
            }
        }

        private Sprite GetOrCreateSprite(Texture2D tex)
        {
            if (tex == null)
                return null;
            if (_splashSprites.TryGetValue(tex, out Sprite cached))
                return cached;
            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
            _splashSprites[tex] = sprite;
            return sprite;
        }

        private void OnDestroy()
        {
            foreach (Sprite sprite in _splashSprites.Values)
            {
                if (sprite != null)
                    Destroy(sprite);
            }
            _splashSprites.Clear();
        }
    }
}
