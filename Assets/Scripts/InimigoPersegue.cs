
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class InimigoPersegue : MonoBehaviour
{
    [Header("Perseguição")]
    public Transform player;
    public float velocidade = 2f;
    public float distanciaParada = 0.5f;

    [Header("Detecção de borda")]
    public float distanciaBorda = 0.5f;
    public LayerMask camadaChao;

    private Rigidbody2D rig;
    private Collider2D colisor;
    private SpriteRenderer spriteRenderer;

    private int direcao = 1;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        colisor = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        if (player == null)
        {
            GameObject objetoPlayer =
                GameObject.FindGameObjectWithTag("Player");

            if (objetoPlayer != null)
                player = objetoPlayer.transform;
        }

        rig.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            Parar();
            return;
        }

        float distancia =
            player.position.x - transform.position.x;

        if (Mathf.Abs(distancia) <= distanciaParada)
        {
            Parar();
            return;
        }

        direcao = distancia > 0 ? 1 : -1;

        // Atualiza a direção visual.
        if (spriteRenderer != null)
            spriteRenderer.flipX = direcao > 0;

        // Impede que o inimigo avance sobre um buraco.
        if (!TemChaoAFrente())
        {
            Parar();
            return;
        }

        rig.linearVelocity = new Vector2(
            direcao * velocidade,
            rig.linearVelocity.y
        );
    }

    bool TemChaoAFrente()
    {
        Bounds limites = colisor.bounds;

        Vector2 origem = new Vector2(
            limites.center.x +
            direcao * (limites.extents.x + 0.05f),
            limites.min.y + 0.05f
        );

        RaycastHit2D hit = Physics2D.Raycast(
            origem,
            Vector2.down,
            distanciaBorda,
            camadaChao
        );

        return hit.collider != null;
    }

    void Parar()
    {
        rig.linearVelocity = new Vector2(
            0f,
            rig.linearVelocity.y
        );
    }
}
