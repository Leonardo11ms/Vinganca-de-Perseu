
using UnityEngine;

public class TriggerDammage : MonoBehaviour
{
    [Header("Referências")]
    public HeartSystem heart;

    [Header("Dano")]
    public int dano = 1;
    public float tempoDano = 1f;

    [Header("Knockback")]
    public float forcaHorizontal = 6f;
    public float forcaVertical = 3f;
    public float duracaoKnockback = 0.25f;

    private NewMonoBehaviourScript controladorPlayer;
    private float contador;
    private bool jogadorEmContato;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        controladorPlayer =
            other.GetComponent<NewMonoBehaviourScript>();

        if (controladorPlayer == null)
            controladorPlayer =
                other.GetComponentInParent<NewMonoBehaviourScript>();

        if (heart == null)
            heart = other.GetComponent<HeartSystem>();

        if (heart == null)
            heart = other.GetComponentInParent<HeartSystem>();

        jogadorEmContato = true;
        contador = 0f;

        AplicarDanoInicial();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        jogadorEmContato = false;
        controladorPlayer = null;
    }

    private void Update()
    {
        if (!jogadorEmContato || heart == null)
            return;

        contador += Time.deltaTime;

        if (contador >= tempoDano)
        {
            heart.ReceberDano(dano);
            contador = 0f;
        }
    }

    private void AplicarDanoInicial()
    {
        heart?.ReceberDano(dano);

        if (controladorPlayer != null)
        {
            controladorPlayer.ReceberKnockback(
                transform.root.position,
                forcaHorizontal,
                forcaVertical,
                duracaoKnockback
            );
        }
    }
}
