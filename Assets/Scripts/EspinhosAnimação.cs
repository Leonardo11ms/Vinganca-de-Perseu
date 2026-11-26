
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class EspinhosAnimacao : MonoBehaviour
{
    [Header("Tempos")]
    public float tempoAparecer = 1f;
    public float tempoDesaparecer = 1f;

    [Header("Jogador")]
    public Transform player;

    [Header("Objeto Filho")]
    public GameObject espinhosDA;

    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider;
    private BoxCollider2D boxColliderPai;

    private Collider2D[] colisoresPlayer;

    private void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
        boxColliderPai = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject objetoPlayer =
                GameObject.FindGameObjectWithTag("Player");

            if (objetoPlayer != null)
                player = objetoPlayer.transform;
        }

        if (player != null)
        {
            colisoresPlayer =
                player.GetComponentsInChildren<Collider2D>();
        }
        else
        {
            Debug.LogError("Player não encontrado!");
        }

        StartCoroutine(CicloEspinhos());
    }

    private IEnumerator CicloEspinhos()
    {
        while (true)
        {
            AlterarEstado(true);

            yield return new WaitForSeconds(tempoAparecer);

            AlterarEstado(false);

            yield return new WaitForSeconds(tempoDesaparecer);
        }
    }

    private void AlterarEstado(bool ativo)
    {
        if (tilemapRenderer != null)
            tilemapRenderer.enabled = ativo;

        if (espinhosDA != null)
            espinhosDA.SetActive(ativo);

        // O pai permanece com seus colisores ativos.
        // Apenas ignoramos a colisão com o jogador.
        if (colisoresPlayer != null)
        {
            foreach (Collider2D colisorPlayer in colisoresPlayer)
            {
                if (colisorPlayer == null)
                    continue;

                if (boxColliderPai != null)
                {
                    Physics2D.IgnoreCollision(
                        boxColliderPai,
                        colisorPlayer,
                        !ativo
                    );
                }

                if (tilemapCollider != null)
                {
                    Physics2D.IgnoreCollision(
                        tilemapCollider,
                        colisorPlayer,
                        !ativo
                    );
                }
            }
        }
    }
}
