using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyTimeStopAbility : MonoBehaviour
{
    public static bool IsTimeStopped;

    [Header("Configuración de Tiempos")]
    [Tooltip("Tiempo que espera desde que aparece hasta que INICIA la animación")]
    public float delayBeforeAnimation = 3f;
    public float timeStopDuration = 5f;
    public float fadeDuration = 0.5f;

    [Header("Animación")]
    public string castTriggerName = "Cast";
    private Animator animator;

    [Header("Efectos Visuales")]
    [Tooltip("Arrastra aquí el archivo Profile de Volume (el cuadradito azul) desde tu carpeta de Proyecto")]
    public VolumeProfile volumeProfile;
    public float normalSaturation = 0f;
    public float desaturatedSaturation = -100f; 

    private ColorAdjustments colorAdjustments;
    private bool hasCast;

    private void Awake()
    {
        // Pilla el Animator automáticamente del mismo GameObject
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        hasCast = false; // Reiniciamos por si el enemigo respawnea

        if (volumeProfile != null)
        {
            if (volumeProfile.TryGet(out colorAdjustments))
            {
                colorAdjustments.saturation.value = normalSaturation;
            }
        }

        // Empezamos solo la cuenta atrás para activar la animación
        StartCoroutine(WaitAndTriggerAnimation());
    }

    private IEnumerator WaitAndTriggerAnimation()
    {
        yield return new WaitForSeconds(delayBeforeAnimation);

        // ¡LANZAMOS LA ANIMACIÓN! Y aquí termina el trabajo automático de este script por ahora.
        if (animator != null)
        {
            animator.SetTrigger(castTriggerName);
        }
    }

    // =========================================================================
    // ESTE ES EL MÉTODO PÚBLICO QUE DEBES LLAMAR DESDE EL ANIMATION EVENT
    // =========================================================================
    public void ExecuteTimeStop()
    {
        // Seguridad: evitamos que el evento se llame dos veces por error
        if (hasCast) return;
        hasCast = true;

        // Arrancamos la secuencia de los efectos visuales y la congelación de torres
        StartCoroutine(TimeStopSequence());
    }

    private IEnumerator TimeStopSequence()
    {
        // 1. Desteñimos la pantalla a blanco y negro
        yield return StartCoroutine(FadeSaturation(normalSaturation, desaturatedSaturation));

        // 2. ¡PARAMOS EL TIEMPO PARA LAS TORRES!
        IsTimeStopped = true;

        yield return new WaitForSeconds(timeStopDuration);

        // 3. ¡RESTAURAMOS EL TIEMPO!
        IsTimeStopped = false;

        // 4. Devolvemos el color a la pantalla
        yield return StartCoroutine(FadeSaturation(desaturatedSaturation, normalSaturation));
    }

    private IEnumerator FadeSaturation(float start, float end)
    {
        if (colorAdjustments == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            colorAdjustments.saturation.value = Mathf.Lerp(start, end, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        colorAdjustments.saturation.value = end;
    }

    // Seguridad: Si este enemigo muere o desaparece, restauramos todo a la normalidad
    private void OnDisable()
    {
        if (IsTimeStopped)
        {
            IsTimeStopped = false;
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = normalSaturation;
            }
        }
    }
}