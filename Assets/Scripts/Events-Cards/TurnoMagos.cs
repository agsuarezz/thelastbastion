using System.Collections;
using UnityEngine;

public class TurnoMagos : MonoBehaviour
{
    public Animator mago1;
    public Animator mago2;
    public Animator mago3;

    [Tooltip("Segundos que dura cada mago moviéndose")]
    public float tiempoPorTurno = 1.5f;

    void Start()
    {
        // Empezamos la corrutina que hará los turnos infinitamente
        StartCoroutine(AnimarPorTurnos());
    }

    void OnEnable()
    {
        // OnEnable se ejecuta SIEMPRE que el panel se hace visible.
        // Así nos aseguramos de que la rutina empiece de nuevo cada vez que sale una mejora.
        StartCoroutine(AnimarPorTurnos());
    }
    IEnumerator AnimarPorTurnos()
    {
        while (true)
        {
            // Turno del Mago 1 (Se mueve 1, paran 2 y 3)
            mago1.speed = 1f;
            mago2.speed = 0f;
            mago3.speed = 0f;
            yield return new WaitForSecondsRealtime(tiempoPorTurno);

            // Turno del Mago 2 (Se mueve 2, paran 1 y 3)
            mago1.speed = 0f;
            mago2.speed = 1f;
            mago3.speed = 0f;
            yield return new WaitForSecondsRealtime(tiempoPorTurno);

            // Turno del Mago 3 (Se mueve 3, paran 1 y 2)
            mago1.speed = 0f;
            mago2.speed = 0f;
            mago3.speed = 1f;
            yield return new WaitForSecondsRealtime(tiempoPorTurno);
        }
    }
}