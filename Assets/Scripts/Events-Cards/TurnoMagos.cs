using System.Collections;
using UnityEngine;

public class TurnoMagos : MonoBehaviour
{
    public Animator mago1;
    public Animator mago2;
    public Animator mago3;

    [Tooltip("Pon aquí los segundos exactos que dura tu animación Action")]
    public float tiempoPorTurno = 1.5f;

    void OnEnable()
    {
        // Esto evita que se congelen si pausas el juego
        mago1.updateMode = AnimatorUpdateMode.UnscaledTime;
        mago2.updateMode = AnimatorUpdateMode.UnscaledTime;
        mago3.updateMode = AnimatorUpdateMode.UnscaledTime;

        StartCoroutine(AnimarPorTurnos());
    }

    IEnumerator AnimarPorTurnos()
    {
        while (true)
        {
            // TURNO 1: El Mago 1 ataca, el 2 y el 3 esperan
            mago1.Play("Action");
            mago2.Play("Idle");
            mago3.Play("Idle");
            yield return new WaitForSecondsRealtime(tiempoPorTurno);

            // TURNO 2: El Mago 2 ataca, el 1 y el 3 esperan
            mago1.Play("Idle");
            mago2.Play("Action");
            mago3.Play("Idle");
            yield return new WaitForSecondsRealtime(tiempoPorTurno);

            // TURNO 3: El Mago 3 ataca, el 1 y el 2 esperan
            mago1.Play("Idle");
            mago2.Play("Idle");
            mago3.Play("Action");
            yield return new WaitForSecondsRealtime(tiempoPorTurno);
        }
    }
}