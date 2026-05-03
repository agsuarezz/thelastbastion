using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class EnemyTimeStopAbility : MonoBehaviour
{
    public static bool IsTimeStopped;
    [Header("Música durante Time Stop")]
public AudioSource musicSource;
public float normalMusicPitch = 1f;
public float stoppedMusicPitch = 0.35f;

    [Header("Configuración de Tiempos")]
    public float delayBeforeAnimation = 3f;
    public float timeStopDuration = 5f;
    public float fadeDuration = 0.5f;

    [Header("Animación")]
    public string castTriggerName = "Cast";
    public string castEndTriggerName = "CastEnd";
    private Animator animator;

    [Header("Sonido")]
    public AudioClip castSound;
    public AudioSource audioSource;

    [Header("Efectos Visuales")]
    public VolumeProfile volumeProfile;
    public float normalSaturation = 0f;
    public float desaturatedSaturation = -100f;

    private ColorAdjustments colorAdjustments;

    private Enemy enemy;
    private float originalSpeed;
    private bool hasCast;
    private bool isCasting;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<Enemy>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 🌟 EL TRUCO ESTRELLA: Le decimos al AudioSource de ESTE enemigo 
        // que ignore las pausas globales de Unity. ¡Es inmune al silencio!
        if (audioSource != null)
        {
            audioSource.ignoreListenerPause = true;
        }
         if (musicSource == null)
    {
        musicSource = GameObject.Find("MusicMenuMain").GetComponent<AudioSource>();
    }
    }

    private void OnEnable()
    {
        hasCast = false;
        isCasting = false;

        if (enemy != null)
            originalSpeed = enemy.enemyData.speed;

        if (volumeProfile != null && volumeProfile.TryGet(out colorAdjustments))
        {
            colorAdjustments.saturation.value = normalSaturation;
        }

        StartCoroutine(WaitAndTriggerAnimation());
    }

    private IEnumerator WaitAndTriggerAnimation()
    {
        yield return new WaitForSeconds(delayBeforeAnimation);

        StartCasting();

        if (animator != null)
            animator.SetTrigger(castTriggerName);
    }

    private void StartCasting()
    {
        isCasting = true;

        if (enemy != null)
        {
            originalSpeed = enemy.currentSpeed;

            if (originalSpeed <= 0f && enemy.enemyData != null)
                originalSpeed = enemy.enemyData.speed;

            enemy.currentSpeed = 0f;
        }

        // Si tu GameManager dice que hay música, lanzamos el sonido (que es inmune a pausas)
        if (audioSource != null && castSound != null && GameManager.musicaActiva)
            audioSource.PlayOneShot(castSound);
    }

    public void ExecuteTimeStop()
    {
        if (hasCast) return;
        hasCast = true;

        StartCoroutine(TimeStopSequence());
    }

    private IEnumerator TimeStopSequence()
    {
        // ⏸️ 1. Pausamos absolutamente TODO el sonido del juego (música, ataques, explosiones)
        // (Pero el sonido de ESTE enemigo se seguirá escuchando gracias al Awake)
       if (musicSource != null)
    musicSource.pitch = stoppedMusicPitch;

        yield return StartCoroutine(FadeSaturation(normalSaturation, desaturatedSaturation));

        IsTimeStopped = true;

       yield return new WaitForSecondsRealtime(timeStopDuration);

        IsTimeStopped = false;

        yield return StartCoroutine(FadeSaturation(desaturatedSaturation, normalSaturation));

        // ▶️ 2. Reanudamos el audio de todo el juego 
        if (musicSource != null)
    musicSource.pitch = normalMusicPitch;
    }

    public void FinishCasting()
    {
        isCasting = false;

        if (enemy != null)
            enemy.currentSpeed = enemy.enemyData.speed;

        if (animator != null)
            animator.SetTrigger(castEndTriggerName);
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
        IsTimeStopped = false;

        // Seguridad: Si este mago muere de repente, quitamos la pausa global sí o sí
        if (musicSource != null)
    musicSource.pitch = normalMusicPitch;

        if (enemy != null && isCasting)
            enemy.currentSpeed = originalSpeed;

        if (colorAdjustments != null)
            colorAdjustments.saturation.value = normalSaturation;
    }
}