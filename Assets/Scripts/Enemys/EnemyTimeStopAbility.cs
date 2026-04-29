using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyTimeStopAbility : MonoBehaviour
{
    public static bool IsTimeStopped;

    [Header("Configuración")]
    public float delayAfterSpawn = 3f;
    public float timeStopDuration = 5f;
    public float fadeDuration = 0.5f;

    [Tooltip("Arrastra aquí el archivo Profile de Volume (el cuadradito azul) desde tu carpeta de Proyecto")]
    public VolumeProfile volumeProfile;
    public float normalSaturation = 0f;
    public float desaturatedSaturation = -100f; 

    private ColorAdjustments colorAdjustments;
    private bool hasCast;

    private void OnEnable()
    {
        // CHIVATO 1: Saber si el script está encendido
        Debug.Log("<color=yellow>1. El enemigo ha spawneado y OnEnable se ha ejecutado.</color>");
        
        hasCast = false; 

        if (volumeProfile != null)
        {
            if (volumeProfile.TryGet(out colorAdjustments))
            {
                colorAdjustments.saturation.value = normalSaturation;
            }
            else
            {
                Debug.LogError("Error: El VolumeProfile no tiene un efecto de 'Color Adjustments'.");
            }
        }
        else
        {
            Debug.LogError("Error: No has arrastrado el VolumeProfile al inspector del enemigo.");
        }

        StartCoroutine(CastAfterDelay());
    }

    private IEnumerator CastAfterDelay()
    {
        // CHIVATO 2: Saber si la cuenta atrás empieza
        Debug.Log("<color=orange>2. Empieza la cuenta atrás de " + delayAfterSpawn + " segundos...</color>");

        yield return new WaitForSeconds(delayAfterSpawn);

        // CHIVATO 3: Saber si llega a terminar la cuenta atrás
        Debug.Log("<color=green>3. ¡Cuenta atrás terminada! Lanzando hechizo...</color>");

        if (hasCast) yield break;
        hasCast = true;

        yield return StartCoroutine(FadeSaturation(normalSaturation, desaturatedSaturation));

        IsTimeStopped = true;

        yield return new WaitForSeconds(timeStopDuration);

        IsTimeStopped = false;
        
        yield return StartCoroutine(FadeSaturation(desaturatedSaturation, normalSaturation));
        
        Debug.Log("<color=cyan>4. Hechizo terminado con éxito.</color>");
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

    private void OnDisable()
    {
        // CHIVATO 5: Saber si el enemigo se está desactivando antes de tiempo
        Debug.Log("<color=red>5. El enemigo se ha desactivado (OnDisable).</color>");
        
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