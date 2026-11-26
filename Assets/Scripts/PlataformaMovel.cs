
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlataformaTransportadora))]
public class PlataformaMovel : MonoBehaviour
{
    [Header("Pontos de movimento")]
    public Transform pontoInicial;
    public Transform pontoFinal;

    [Header("Velocidade")]
    public float velocidade = 2f;

    private Rigidbody2D rb;

    public Vector2 VelocidadeAtual { get; private set; }
    public float VelocidadeVerticalAtual => VelocidadeAtual.y;

    private Vector2 destino;
    private bool indoParaFinal;
    private bool movimentoPausado;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (pontoInicial == null || pontoFinal == null)
        {
            Debug.LogError("Configure os pontos inicial e final da plataforma.");
            enabled = false;
            return;
        }

        rb.position = pontoInicial.position;
        destino = pontoFinal.position;
        indoParaFinal = true;
    }

    void FixedUpdate()
    {
        if (movimentoPausado)
        {
            VelocidadeAtual = Vector2.zero;
            return;
        }

        Vector2 posicaoAnterior = rb.position;

        Vector2 novaPosicao = Vector2.MoveTowards(
            posicaoAnterior,
            destino,
            velocidade * Time.fixedDeltaTime
        );

        Vector2 deslocamento = novaPosicao - posicaoAnterior;

        VelocidadeAtual = deslocamento / Time.fixedDeltaTime;

        rb.MovePosition(novaPosicao);

        if (Vector2.Distance(novaPosicao, destino) < 0.01f)
        {
            indoParaFinal = !indoParaFinal;

            destino = indoParaFinal
                ? pontoFinal.position
                : pontoInicial.position;
        }
    }

    public void PausarMovimento()
    {
        movimentoPausado = true;
        VelocidadeAtual = Vector2.zero;
    }

    public void RetomarMovimento()
    {
        movimentoPausado = false;
    }
}
