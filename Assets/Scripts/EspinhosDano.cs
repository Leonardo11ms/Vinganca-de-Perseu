
using UnityEngine;

public class EspinhosDano : MonoBehaviour
{
    public HeartSystem heart;
    public Transform player;

    private Collider2D areaDano;

    private float contador = 0f;
    private float tempoDano = 1f;

    private bool primeiroContato = false;

    private void Awake()
    {
        areaDano = GetComponent<Collider2D>();
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
    }

    private void Update()
    {
        if (areaDano == null || heart == null || player == null)
            return;

        Collider2D colisorPlayer =
            player.GetComponent<Collider2D>();

        if (colisorPlayer == null)
            return;

        bool emContato = areaDano.bounds.Intersects(
            colisorPlayer.bounds
        );

        if (!emContato)
        {
            contador = 0f;
            primeiroContato = false;
            return;
        }

        if (!primeiroContato)
        {
            heart.ReceberDano(1);
            primeiroContato = true;
            contador = 0f;
            return;
        }

        contador += Time.deltaTime;

        if (contador >= tempoDano)
        {
            heart.ReceberDano(1);
            contador = 0f;
        }
    }
}
