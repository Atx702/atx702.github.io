using LastNight.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastNight.Dialogue
{
    /// <summary>
    /// World-space subtitle panel. Head-locked below center by default; moves next to a visible
    /// speaker when the speaker is in view. Colors and sizes follow docs/05_Menus_UI_Settings.md.
    /// </summary>
    public class SubtitleView : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] TMP_Text label;
        [SerializeField] Image background;
        [SerializeField] float headLockedDistance = 1.6f;
        [SerializeField] float headLockedDrop = 0.35f;
        [SerializeField] float followSharpness = 6f;
        [SerializeField] float[] fontSizes = { 0.035f, 0.045f, 0.06f, 0.075f };

        static readonly Color VoiceColor = new Color(1f, 0.96f, 0.9f);
        static readonly Color MarenColor = new Color(1f, 0.6f, 0.6f);
        static readonly Color WrenColor = new Color(1f, 0.9f, 0.4f);
        static readonly Color OtherColor = new Color(0.8f, 0.8f, 0.8f);

        Transform speaker;
        CanvasGroup group;

        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
        }

        void OnEnable()
        {
            if (DialoguePlayer.Instance == null) return;
            DialoguePlayer.Instance.LineStarted += Show;
            DialoguePlayer.Instance.LineEnded += Hide;
        }

        void OnDisable()
        {
            if (DialoguePlayer.Instance == null) return;
            DialoguePlayer.Instance.LineStarted -= Show;
            DialoguePlayer.Instance.LineEnded -= Hide;
        }

        void Show(DialogueLine line, Transform from)
        {
            var s = GameManager.Instance != null ? GameManager.Instance.Settings : new GameSettings();
            if (!s.subtitles) return;

            speaker = from;
            var name = DisplayName(line.speaker);
            var body = line.leak ? $"<i>{line.text}</i>" : line.text;
            label.text = s.speakerNames && name != null ? $"<b>{name}:</b> {body}" : body;
            label.color = ColorFor(line.speaker);
            label.fontSize = fontSizes[Mathf.Clamp(s.subtitleSize, 0, fontSizes.Length - 1)];
            background.color = new Color(0, 0, 0, s.subtitleBackground);
            group.alpha = 1f;
        }

        void Hide()
        {
            group.alpha = 0f;
            speaker = null;
        }

        void LateUpdate()
        {
            if (group.alpha <= 0f) return;
            Vector3 target;
            if (speaker != null && IsInView(speaker.position))
                target = speaker.position + Vector3.up * 0.35f;
            else
                target = head.position + head.forward * headLockedDistance + Vector3.down * headLockedDrop;

            transform.position = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
            transform.rotation = Quaternion.LookRotation(transform.position - head.position, Vector3.up);
        }

        bool IsInView(Vector3 p)
        {
            var dir = (p - head.position).normalized;
            return Vector3.Dot(dir, head.forward) > 0.85f;
        }

        static string DisplayName(string speaker) => speaker switch
        {
            "VOICE" => null, // the Voice is never labeled; it's "you"
            "TOMAS" => "Tomas",
            "MAREN" or "MAREN_MEM" => "Maren",
            "WREN" => "Wren",
            "ELIO" => "Dad",
            "LUCIA" => "Mom",
            "ADAIR" => "Ms. Adair",
            "NURSE" => "Nurse",
            "DISPATCH" => "Dispatcher",
            "OKAFOR" or "LIND" => "Paramedic",
            _ => speaker,
        };

        static Color ColorFor(string speaker) => speaker switch
        {
            "VOICE" or "TOMAS" => VoiceColor,
            "MAREN" or "MAREN_MEM" => MarenColor,
            "WREN" => WrenColor,
            _ => OtherColor,
        };
    }
}
