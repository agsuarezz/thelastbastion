using TMPro;
using UnityEngine;

public class GameOverRandomPhrase : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI phraseText;

    [TextArea(2, 4)]
    [SerializeField] private string[] phrases =
    {
        "El bastión cayó, pero al menos lo intentaste.",
        "Los enemigos han pedido repetir la partida.",
        "El castillo resistió menos que tus excusas.",
        "Derrota épica. Estrategia cuestionable.",
        "Los enemigos agradecen tu generosidad.",
        "Los enemigos entraron como Pedro por su casa.",
        "No era un tower defense, era un tower suggestion.",
        "Tu estrategia será estudiada. Como advertencia.",
        "Buen intento. Los enemigos se rieron muchísimo.",
        "Las murallas eran más emocionales que físicas.",
        "Has desbloqueado el rango: amenaza para el reino.",
        "Tus torres y tú habéis decidido tomar caminos separados.",
        "El bastión encontró a alguien que sí sabe defenderlo.",
        "Tus murallas dijeron: 'no eres tú, soy yo'",
        "La conexión entre tú y el reino se perdió hace varias rondas.",
        "El castillo ya venía viendo red flags desde la ronda 3.",
        "Tu estrategia fue un 'tenemos que hablar'",
        "¡Madre mía, qué defensa más blandita!",
        "¡Pero ciérrale el paso hombre!"
    };

    private void Start()
    {
        if (phraseText == null)
            phraseText = GetComponent<TextMeshProUGUI>();

        if (phrases.Length > 0 && phraseText != null)
        {
            int randomIndex = Random.Range(0, phrases.Length);
            phraseText.text = phrases[randomIndex];
        }
    }
}