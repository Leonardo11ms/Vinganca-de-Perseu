
using UnityEngine;

public class InimigoSpawner : MonoBehaviour
{
    [Header("Configuração")]
    public GameObject inimigoPrefab;
    public Transform pontoSpawn;
    public Transform player;

    [Header("Raio de ativação")]
    public float raioAtivacao = 5f;

    [Header("Raio de destruição")]
    public Transform pontoDestruicao;
    public float raioDestruicao = 2f;

    [Header("Cooldown")]
    public float cooldownSpawn = 2f;

    private GameObject inimigoAtual;

    private bool ativado;
    private bool jaTeveInimigo;
    private bool aguardandoCooldown;

    private float proximoSpawn;

    void Start()
    {
        if (player == null)
        {
            GameObject objetoPlayer =
                GameObject.FindGameObjectWithTag("Player");

            if (objetoPlayer != null)
                player = objetoPlayer.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // ATIVAÇÃO
        if (!ativado)
        {
            float distancia = Vector2.Distance(
                player.position,
                transform.position
            );

            if (distancia <= raioAtivacao)
                ativado = true;
        }

        if (!ativado)
            return;

        // DESTRUIÇÃO
        if (pontoDestruicao != null)
        {
            float distanciaDestruicao = Vector2.Distance(
                player.position,
                pontoDestruicao.position
            );

            if (distanciaDestruicao <= raioDestruicao)
            {
                Destroy(gameObject);
                return;
            }
        }

        // CONTROLE DOS INIMIGOS
        if (inimigoAtual != null)
            return;

        if (jaTeveInimigo && !aguardandoCooldown)
        {
            proximoSpawn = Time.time + cooldownSpawn;
            aguardandoCooldown = true;
        }

        if (aguardandoCooldown && Time.time < proximoSpawn)
            return;

        if (inimigoPrefab == null || pontoSpawn == null)
            return;

        inimigoAtual = Instantiate(
            inimigoPrefab,
            pontoSpawn.position,
            pontoSpawn.rotation
        );

        jaTeveInimigo = true;
        aguardandoCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            raioAtivacao
        );

        if (pontoDestruicao != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                pontoDestruicao.position,
                raioDestruicao
            );
        }
    }
}
