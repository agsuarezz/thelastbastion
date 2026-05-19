using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Efecto de cadena eléctrica. Al activarse salta desde el enemigo impactado
/// hacia los N enemigos más cercanos en un radio determinado, aplicándoles daño
/// y mostrando un rayo visual temporal con LineRenderer.
///
/// El daño se reduce un % por cada salto (falloff) para que el primer objetivo
/// siempre reciba más daño que los secundarios.
/// </summary>
public class ChainLightningEffect : MonoBehaviour
{
    // ── Parámetros inyectados ────────────────────────────────────────────────
    private float _damage;
    private float _radius;
    private int   _maxJumps;
    private float _damageFalloff; // multiplicador por salto, ej: 0.6 → cada salto hace 60% del anterior

    // ── Constantes visuales ──────────────────────────────────────────────────
    private const float LineDisplayTime   = 0.12f; // segundos que se ve el rayo
    private const int   ZigzagSegments    = 8;     // más segmentos = más detalle
    private const float ZigzagAmplitude   = 0.35f; // desplazamiento máximo perpendicular
    private const float LineWidthMain     = 0.10f; // grosor del rayo principal
    private const float LineWidthCore     = 0.04f; // grosor del núcleo blanco interior

    // ── Configuración ────────────────────────────────────────────────────────

    public void Configure(float damage, float radius, int maxJumps, float damageFalloff)
    {
        _damage       = damage;
        _radius       = radius;
        _maxJumps     = maxJumps;
        _damageFalloff = damageFalloff;
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia la cadena desde el enemigo inicial. Llamado por ChainLightningOnHitEffect.
    /// </summary>
    public void Activate(Enemy originEnemy)
    {
        StartCoroutine(ChainRoutine(originEnemy));
    }

    // ── Lógica interna ───────────────────────────────────────────────────────

    private IEnumerator ChainRoutine(Enemy originEnemy)
    {
        HashSet<Enemy> hit = new HashSet<Enemy> { originEnemy };
        Enemy current      = originEnemy;
        float currentDamage = _damage;

        for (int jump = 0; jump < _maxJumps; jump++)
        {
            Enemy next = FindNearestEnemy(current.transform.position, hit);
            if (next == null) break;

            hit.Add(next);

            currentDamage *= _damageFalloff;
            next.TakeDamage(currentDamage);

            DrawLightningArc(current.transform.position, next.transform.position);
            Debug.Log($"[Cadena] Salto {jump + 1}: '{current.name}' → '{next.name}' | daño: {currentDamage:F1}");

            current = next;
            yield return new WaitForSeconds(0.05f); // pequeño delay entre saltos para que se vea
        }

        Destroy(this);
    }

    private Enemy FindNearestEnemy(Vector2 origin, HashSet<Enemy> exclude)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, _radius);

        Enemy nearest      = null;
        float nearestDist  = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            if (!col.CompareTag("Enemy")) continue;

            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy == null || enemy.IsDead || exclude.Contains(enemy)) continue;

            float dist = Vector2.Distance(origin, col.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest     = enemy;
            }
        }

        return nearest;
    }

#if UNITY_EDITOR
    private void DrawLightningArc(Vector3 from, Vector3 to)
    {
        Vector3[] points = BuildZigzagPoints(from, to);

        // Capa exterior: rayo azul ancho (glow)
        SpawnLineRenderer(
            "[VFX] LightningOuter",
            points,
            LineWidthMain,
            LineWidthMain * 0.5f,
            new Color(0.3f, 0.6f, 1f, 0.85f)
        );

        // Capa interior: núcleo blanco fino para dar sensación de intensidad
        SpawnLineRenderer(
            "[VFX] LightningCore",
            points,
            LineWidthCore,
            LineWidthCore * 0.5f,
            new Color(0.85f, 0.95f, 1f, 1f)
        );
    }
#else
    private void DrawLightningArc(Vector3 from, Vector3 to)
    {
        Vector3[] points = BuildZigzagPoints(from, to);

        SpawnLineRenderer(
            "[VFX] LightningOuter",
            points,
            LineWidthMain,
            LineWidthMain * 0.5f,
            new Color(0.3f, 0.6f, 1f, 0.85f)
        );

        SpawnLineRenderer(
            "[VFX] LightningCore",
            points,
            LineWidthCore,
            LineWidthCore * 0.5f,
            new Color(0.85f, 0.95f, 1f, 1f)
        );
    }
#endif

    /// <summary>
    /// Genera los puntos del zigzag entre 'from' y 'to'.
    /// Divide el segmento en N partes y desplaza cada punto intermedio
    /// aleatoriamente en la dirección perpendicular al trazo.
    /// </summary>
    private Vector3[] BuildZigzagPoints(Vector3 from, Vector3 to)
    {
        int totalPoints = ZigzagSegments + 1;
        Vector3[] points = new Vector3[totalPoints];

        // Dirección perpendicular al rayo (en 2D = rotar 90°)
        Vector3 dir        = (to - from).normalized;
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0f);

        for (int i = 0; i < totalPoints; i++)
        {
            float t = (float)i / ZigzagSegments;
            Vector3 basePoint = Vector3.Lerp(from, to, t);

            // Los extremos no se desplazan para que el rayo llegue al objetivo
            if (i == 0 || i == ZigzagSegments)
            {
                points[i] = basePoint;
            }
            else
            {
                float offset = Random.Range(-ZigzagAmplitude, ZigzagAmplitude);
                points[i]   = basePoint + perpendicular * offset;
            }
        }

        return points;
    }

    private void SpawnLineRenderer(string goName, Vector3[] points, float startWidth, float endWidth, Color color)
    {
        GameObject go = new GameObject(goName);
        LineRenderer lr = go.AddComponent<LineRenderer>();

        lr.positionCount = points.Length;
        lr.SetPositions(points);
        lr.startWidth    = startWidth;
        lr.endWidth      = endWidth;
        lr.useWorldSpace = true;
        lr.numCapVertices = 3; // extremos redondeados

        var mat   = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        lr.material = mat;

        Destroy(go, LineDisplayTime);
    }
}