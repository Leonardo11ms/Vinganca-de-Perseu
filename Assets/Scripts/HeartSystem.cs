
using UnityEngine;
using UnityEngine.UI;

public class HeartSystem : MonoBehaviour
{
    [Header("Vida")]
    public int vida = 100;
    public int vidaMaxima = 100;

    [Header("Interface de Vida (Opcional)")]
    public Image[] coracao;
    public Sprite cheio;
    public Sprite vazio;

    [Header("Morte")]
    public float tempoParaDestruir = 1f;

    private bool morreu = false;

    private void Start()
    {
        if (vidaMaxima <= 0)
            vidaMaxima = 100;

        if (vida <= 0)
            vida = vidaMaxima;

        vida = Mathf.Clamp(vida, 0, vidaMaxima);

        HealthLogic();
    }

    public void ReceberDano(int quantidade)
    {
        if (morreu || quantidade <= 0)
            return;

        vida -= quantidade;
        vida = Mathf.Max(vida, 0);

        HealthLogic();

        Debug.Log(gameObject.name + " recebeu dano: " + quantidade +
                  " | Vida restante: " + vida);

        if (vida <= 0)
            DeadState();
    }

    // Zera a vida sem destruir o jogador.
    public void MorrerPorQueda()
    {
        if (morreu)
            return;

        vida = 0;
        morreu = true;

        HealthLogic();

        Debug.Log(gameObject.name + " morreu por queda. Todos os corações foram esvaziados!");
    }

    private void Update()
    {
        HealthLogic();
    }

    private void HealthLogic()
    {
        if (coracao == null || cheio == null || vazio == null)
            return;

        for (int i = 0; i < coracao.Length; i++)
        {
            if (coracao[i] == null)
                continue;

            coracao[i].sprite = i < vida ? cheio : vazio;
            coracao[i].enabled = i < vidaMaxima;
        }
    }

    private void DeadState()
    {
        if (morreu)
            return;

        morreu = true;

        Debug.Log(gameObject.name + " foi derrotado!");

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Destroy(gameObject);
    }
}
