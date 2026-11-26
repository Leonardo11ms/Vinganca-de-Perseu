
using UnityEngine;
using System.Collections.Generic;

public class PlataformaTransportadora : MonoBehaviour
{
    private List<Rigidbody2D> personagens =
        new List<Rigidbody2D>();

    private HashSet<Rigidbody2D> personagensSuspensos =
        new HashSet<Rigidbody2D>();

    public void Transportar(Vector2 deslocamento)
    {
        for (int i = personagens.Count - 1; i >= 0; i--)
        {
            Rigidbody2D personagem = personagens[i];

            if (personagem == null)
            {
                personagens.RemoveAt(i);
                continue;
            }

            if (personagensSuspensos.Contains(personagem))
                continue;

            // Aplica exatamente o deslocamento da plataforma.
            personagem.position += deslocamento;
        }
    }

    public void RegistrarPersonagem(Rigidbody2D personagem)
    {
        if (personagem == null)
            return;

        if (personagensSuspensos.Contains(personagem))
            return;

        if (!personagens.Contains(personagem))
            personagens.Add(personagem);
    }

    public void RemoverPersonagem(Rigidbody2D personagem)
    {
        if (personagem == null)
            return;

        personagens.Remove(personagem);
        personagensSuspensos.Add(personagem);
    }

    public void LiberarPersonagem(Rigidbody2D personagem)
    {
        if (personagem == null)
            return;

        personagensSuspensos.Remove(personagem);
    }

    void FixedUpdate()
    {
        personagens.RemoveAll(
            personagem => personagem == null
        );
    }

    bool EstaSobrePlataforma(Collision2D collision)
    {
        Rigidbody2D personagem = collision.rigidbody;

        if (personagem == null)
            return false;

        Collider2D colisorPlataforma = GetComponent<Collider2D>();

        Collider2D colisorPersonagem =
            personagem.GetComponent<CapsuleCollider2D>();

        if (colisorPlataforma == null ||
            colisorPersonagem == null)
            return false;

        Bounds playerBounds = colisorPersonagem.bounds;
        Bounds plataformaBounds = colisorPlataforma.bounds;

        return
            playerBounds.min.y >= plataformaBounds.max.y - 0.2f &&
            playerBounds.min.y <= plataformaBounds.max.y + 0.2f &&
            playerBounds.max.x > plataformaBounds.min.x &&
            playerBounds.min.x < plataformaBounds.max.x;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        RegistrarPorColisao(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        RegistrarPorColisao(collision);
    }

    void RegistrarPorColisao(Collision2D collision)
    {
        Rigidbody2D personagem = collision.rigidbody;

        if (personagem == null)
            return;

        if (EstaSobrePlataforma(collision))
        {
            RegistrarPersonagem(personagem);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D personagem = collision.rigidbody;

        if (personagem == null)
            return;

        if (personagens.Contains(personagem))
        {
            personagens.Remove(personagem);
        }
    }
}
