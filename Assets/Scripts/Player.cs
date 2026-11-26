
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Rigidbody2D rig;
    private CapsuleCollider2D playerCollider;

    [Header("Movimento")]
    public float Speed = 5f;
    public float JumpForce = 10f;
    public float RunMultiplier = 1.5f;
    public float AirSpeedMultiplier = 1.2f;

    public bool isJumping;
    public bool doubleJump;

    [Header("Ataques")]
    public BoxCollider2D swordHitbox1;
    public BoxCollider2D swordHitbox2;
    public BoxCollider2D swordHitbox3;
    public Collider2D shieldHitbox;

    public int danoAtaque1 = 25;
    public int danoAtaque2 = 25;
    public int danoAtaque3 = 50;

    [Header("Knockback")]
    public float forcaHorizontal = 6f;
    public float forcaVertical = 3f;
    public float duracaoKnockback = 0.25f;

    private float gravidadeOriginal;
    private float horizontal;

    private int combo;
    private int direcao = 1;

    private bool grounded;
    private bool isFalling;
    private bool wasGrounded;
    private bool isRunning;
    private bool descendoPlataforma;
    private bool knockbackAtivo;

    private Coroutine rotinaKnockback;

    private ContactPoint2D[] contacts = new ContactPoint2D[32];
    private ContactFilter2D contactFilter;

    private Transform[] hitboxTransforms;
    private Vector3[] hitboxPositions;
    private Vector3[] hitboxScales;

    private PhysicsMaterial2D semAtrito;

    private PlataformaTransportadora plataformaApoiando;
    private Collider2D colisorPlataformaApoiando;

    private HashSet<HeartSystem> inimigosAtingidos =
        new HashSet<HeartSystem>();

    void Awake()
    {
        anim = GetComponent<Animator>();
        rig = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<CapsuleCollider2D>();

        if (anim == null || rig == null ||
            spriteRenderer == null || playerCollider == null)
        {
            Debug.LogError(
                "Player precisa de Animator, Rigidbody2D, SpriteRenderer e CapsuleCollider2D."
            );

            enabled = false;
            return;
        }

        gravidadeOriginal = rig.gravityScale;

        rig.WakeUp();

        contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = false;
        contactFilter.useLayerMask = false;

        hitboxTransforms = new Transform[]
        {
            swordHitbox1 != null ? swordHitbox1.transform : null,
            swordHitbox2 != null ? swordHitbox2.transform : null,
            swordHitbox3 != null ? swordHitbox3.transform : null,
            shieldHitbox != null ? shieldHitbox.transform : null
        };

        hitboxPositions = new Vector3[hitboxTransforms.Length];
        hitboxScales = new Vector3[hitboxTransforms.Length];

        for (int i = 0; i < hitboxTransforms.Length; i++)
        {
            if (hitboxTransforms[i] == null)
                continue;

            hitboxPositions[i] =
                transform.InverseTransformPoint(
                    hitboxTransforms[i].position
                );

            hitboxScales[i] = hitboxTransforms[i].localScale;
        }

        semAtrito = new PhysicsMaterial2D("SemAtritoPersonagem");
        semAtrito.friction = 0f;
        semAtrito.bounciness = 0f;

        playerCollider.sharedMaterial = semAtrito;
    }

    void Start()
    {
        if (swordHitbox1 != null)
            swordHitbox1.enabled = false;

        if (swordHitbox2 != null)
            swordHitbox2.enabled = false;

        if (swordHitbox3 != null)
            swordHitbox3.enabled = false;

        if (shieldHitbox != null)
            shieldHitbox.enabled = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        VerificarChao();

        if (Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.DownArrow))
        {
            TentarDescerPlataforma();
        }

        if (grounded && !wasGrounded)
            ResetarCombo();

        wasGrounded = grounded;

        Move();
        Jump();
        Attack();

        isFalling = !grounded &&
                    rig.linearVelocity.y < -0.1f;

        anim.SetBool("IsGrounded", grounded);
        anim.SetBool("IsJumping", isJumping && !grounded);
        anim.SetBool("IsFalling", isFalling);

        ControlarQueda();
        VirarDirecao();
    }



    void FixedUpdate()
    {
        if (knockbackAtivo)
            return;

        float velocidadeAtual = Speed;

        if (isJumping)
            velocidadeAtual *= AirSpeedMultiplier;
        else if (isRunning)
            velocidadeAtual *= RunMultiplier;

        Vector2 velocidadePlataforma = Vector2.zero;

        if (grounded && plataformaApoiando != null)
        {
            PlataformaMovel movimento =
                plataformaApoiando.GetComponent<PlataformaMovel>();

            if (movimento != null)
                velocidadePlataforma = movimento.VelocidadeAtual;
        }
        float velocidadeVertical = rig.linearVelocity.y;

        if (grounded && plataformaApoiando != null)
        {
            velocidadeVertical = velocidadePlataforma.y;
        }

        rig.linearVelocity = new Vector2(
            horizontal * velocidadeAtual + velocidadePlataforma.x,
            velocidadeVertical
        );
    }



    void Move()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (knockbackAtivo)
            horizontal = 0f;

        isRunning = Input.GetKey(KeyCode.LeftShift)
                    && horizontal != 0
                    && !isJumping
                    && !knockbackAtivo;

        anim.SetFloat("Speed", Mathf.Abs(horizontal));
        anim.SetBool("IsRunning", isRunning);
    }





    void Jump()
    {
        if (knockbackAtivo)
            return;

        if (!Input.GetButtonDown("Jump"))
            return;

        // SEGUNDO PULO
        if (doubleJump && isJumping)
        {
            rig.linearVelocity = new Vector2(
                rig.linearVelocity.x,
                0f
            );

            rig.AddForce(
                Vector2.up * JumpForce,
                ForceMode2D.Impulse
            );

            doubleJump = false;
            return;
        }

        // PRIMEIRO PULO
        if (grounded)
        {
            float velocidadePlataforma = 0f;

            if (plataformaApoiando != null)
            {
                PlataformaMovel movimento =
                    plataformaApoiando.GetComponent<PlataformaMovel>();

                if (movimento != null)
                    velocidadePlataforma =
                        movimento.VelocidadeVerticalAtual;
            }

            if (colisorPlataformaApoiando != null)
            {
                StartCoroutine(
                    IgnorarColisaoAteAterrissar(
                        colisorPlataformaApoiando
                    )
                );
            }

            rig.linearVelocity = new Vector2(
                rig.linearVelocity.x,
                velocidadePlataforma
            );

            rig.AddForce(
                Vector2.up * JumpForce,
                ForceMode2D.Impulse
            );

            isJumping = true;
            doubleJump = true;
            grounded = false;

            plataformaApoiando = null;
            colisorPlataformaApoiando = null;
        }
    }








    void VerificarChao()
    {
        grounded = false;

        int quantidade = rig.GetContacts(
            contactFilter,
            contacts
        );

        for (int i = 0; i < quantidade; i++)
        {
            ContactPoint2D contact = contacts[i];

            if (contact.collider != playerCollider &&
                contact.otherCollider != playerCollider)
                continue;

            if (contact.normal.y > 0f)
            {
                grounded = true;
                break;
            }
        }

        if (grounded && rig.linearVelocity.y <= 0.05f)
        {
            isJumping = false;
            doubleJump = false;
        }
        else
        {
            isJumping = true;
        }
    }

    // PLATAFORMAS MÓVEIS

    void OnCollisionEnter2D(Collision2D collision)
    {
        VerificarPlataforma(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        VerificarPlataforma(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (descendoPlataforma)
            return;

        Collider2D[] candidatos =
        {
            collision.collider,
            collision.otherCollider
        };

        foreach (Collider2D candidato in candidatos)
        {
            if (candidato == null)
                continue;

            if (candidato == colisorPlataformaApoiando)
            {
                if (plataformaApoiando != null)
                    plataformaApoiando.RemoverPersonagem(rig);

                plataformaApoiando = null;
                colisorPlataformaApoiando = null;

                break;
            }
        }
    }



    void VerificarPlataforma(Collision2D collision)
    {
        if (descendoPlataforma)
            return;

        bool apoiadoEmCima = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                apoiadoEmCima = true;
                break;
            }
        }

        if (!apoiadoEmCima)
            return;

        Collider2D[] candidatos =
        {
        collision.collider,
        collision.otherCollider
    };

        foreach (Collider2D candidato in candidatos)
        {
            if (candidato == null || candidato == playerCollider)
                continue;

            PlataformaTransportadora transportadora =
                candidato.GetComponent<PlataformaTransportadora>();

            if (transportadora == null)
                transportadora =
                    candidato.GetComponentInParent<PlataformaTransportadora>();

            if (transportadora == null &&
                candidato.attachedRigidbody != null)
            {
                transportadora =
                    candidato.attachedRigidbody
                    .GetComponent<PlataformaTransportadora>();
            }

            if (transportadora == null)
                continue;

            plataformaApoiando = transportadora;
            colisorPlataformaApoiando = candidato;

            plataformaApoiando.LiberarPersonagem(rig);
            plataformaApoiando.RegistrarPersonagem(rig);

            return;
        }
    }



    void TentarDescerPlataforma()
    {
        if (!grounded || descendoPlataforma)
            return;

        if (plataformaApoiando == null ||
            colisorPlataformaApoiando == null)
            return;

        StartCoroutine(
            DescerPlataforma(
                plataformaApoiando,
                colisorPlataformaApoiando
            )
        );
    }

    IEnumerator DescerPlataforma(
        PlataformaTransportadora transportadora,
        Collider2D colisorPlataforma)
    {
        descendoPlataforma = true;

        transportadora.RemoverPersonagem(rig);

        Rigidbody2D plataformaRb =
            colisorPlataforma.attachedRigidbody;

        Collider2D[] colisoesPlataforma =
            plataformaRb != null
            ? plataformaRb.GetComponents<Collider2D>()
            : colisorPlataforma.GetComponents<Collider2D>();

        foreach (Collider2D colisor in colisoesPlataforma)
        {
            if (colisor != null && !colisor.isTrigger)
            {
                Physics2D.IgnoreCollision(
                    playerCollider,
                    colisor,
                    true
                );
            }
        }

        grounded = false;
        wasGrounded = false;
        isJumping = true;
        doubleJump = false;

        plataformaApoiando = null;
        colisorPlataformaApoiando = null;

        rig.linearVelocity = new Vector2(
            rig.linearVelocity.x,
            -2f
        );

        while (
            playerCollider != null &&
            colisorPlataforma != null &&
            playerCollider.bounds.max.y >=
            colisorPlataforma.bounds.min.y
        )
        {
            yield return new WaitForFixedUpdate();
        }

        foreach (Collider2D colisor in colisoesPlataforma)
        {
            if (colisor != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(
                    playerCollider,
                    colisor,
                    false
                );
            }
        }

        transportadora.LiberarPersonagem(rig);

        descendoPlataforma = false;
    }

    // ATAQUES

    public void Attack()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (combo == 0)
        {
            anim.SetTrigger("Attack 1");
            combo = 1;
        }
        else if (combo == 1 &&
                 anim.GetBool("ComboWindow"))
        {
            if (swordHitbox1 != null)
                swordHitbox1.enabled = false;

            anim.SetBool("ComboWindow", false);
            anim.SetTrigger("Attack 2");
            combo = 2;
        }
        else if (combo == 2 &&
                 anim.GetBool("ComboWindow"))
        {
            if (swordHitbox2 != null)
                swordHitbox2.enabled = false;

            anim.SetBool("ComboWindow", false);
            anim.SetTrigger("Attack 3");
            combo = 3;
        }
    }

    public void AtivarHitbox1()
    {
        if (combo == 1 && swordHitbox1 != null)
        {
            inimigosAtingidos.Clear();
            swordHitbox1.enabled = true;
        }
    }

    public void DesativarHitbox1()
    {
        if (swordHitbox1 != null)
            swordHitbox1.enabled = false;
    }

    public void AtivarHitbox2()
    {
        if (combo == 2 && swordHitbox2 != null)
        {
            inimigosAtingidos.Clear();
            swordHitbox2.enabled = true;
        }
    }

    public void DesativarHitbox2()
    {
        if (swordHitbox2 != null)
            swordHitbox2.enabled = false;
    }

    public void AtivarHitbox3()
    {
        if (combo == 3 && swordHitbox3 != null)
        {
            inimigosAtingidos.Clear();
            swordHitbox3.enabled = true;
        }
    }

    public void DesativarHitbox3()
    {
        if (swordHitbox3 != null)
            swordHitbox3.enabled = false;
    }

    public void AtivarHitboxEscudo()
    {
        if (shieldHitbox != null)
            shieldHitbox.enabled = true;
    }

    public void DesativarHitboxEscudo()
    {
        if (shieldHitbox != null)
            shieldHitbox.enabled = false;
    }

    public void AbrirCombo()
    {
        anim.SetBool("ComboWindow", true);
    }

    public void FecharCombo()
    {
        anim.SetBool("ComboWindow", false);
    }

    public void ResetarCombo()
    {
        combo = 0;
        anim.SetBool("ComboWindow", false);
    }

    public void EncerrarCombo()
    {
        if (combo == 1)
            combo = 0;

        anim.SetBool("ComboWindow", false);
    }

    public void EncerrarComboAtaque2()
    {
        if (combo == 2)
            combo = 0;

        anim.SetBool("ComboWindow", false);
    }

    // DANO RECEBIDO


    public void ReceberKnockback(
        Vector2 origem,
        float forcaHorizontal,
        float forcaVertical,
        float duracao)
    {
        if (rotinaKnockback != null)
            StopCoroutine(rotinaKnockback);

        float direcaoKnockback =
            Mathf.Sign(transform.position.x - origem.x);

        if (direcaoKnockback == 0)
            direcaoKnockback = spriteRenderer.flipX ? 1f : -1f;

        knockbackAtivo = true;

        horizontal = 0f;
        isRunning = false;

        float velocidadeInicial = direcaoKnockback * forcaHorizontal;

        rig.linearVelocity = new Vector2(
            velocidadeInicial,
            forcaVertical
        );

        rotinaKnockback = StartCoroutine(
            FinalizarKnockback(velocidadeInicial, duracao)
        );
    }

    private IEnumerator FinalizarKnockback(
        float velocidadeInicial,
        float duracao)
    {
        float tempo = 0f;

        while (tempo < duracao)
        {
            float progresso = tempo / duracao;

            float velocidadeX = Mathf.Lerp(
                velocidadeInicial,
                0f,
                progresso
            );

            rig.linearVelocity = new Vector2(
                velocidadeX,
                rig.linearVelocity.y
            );

            tempo += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        rig.linearVelocity = new Vector2(
            0f,
            rig.linearVelocity.y
        );

        knockbackAtivo = false;
        rotinaKnockback = null;
    }

    // OUTRAS FUNÇÕES

    void ControlarQueda()
    {
        rig.gravityScale = gravidadeOriginal;
    }

    void VirarDirecao()
    {
        int novaDirecao = direcao;

        if (Input.GetKey(KeyCode.A))
            novaDirecao = -1;
        else if (Input.GetKey(KeyCode.D))
            novaDirecao = 1;

        if (novaDirecao == direcao)
            return;

        direcao = novaDirecao;

        spriteRenderer.flipX = direcao == -1;

        HashSet<Transform> transformacoesInvertidas =
            new HashSet<Transform>();

        for (int i = 0; i < hitboxTransforms.Length; i++)
        {
            Transform alvo = hitboxTransforms[i];

            if (alvo == null || !alvo.IsChildOf(transform))
                continue;

            if (!transformacoesInvertidas.Add(alvo))
                continue;

            Vector3 posicao = hitboxPositions[i];
            posicao.x *= direcao;

            alvo.position = transform.TransformPoint(posicao);

            Vector3 escala = hitboxScales[i];
            escala.x *= direcao;

            alvo.localScale = escala;
        }
    }

    public void MostrarCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void EsconderCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // DETECÇÃO DE ACERTOS

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessarAcerto(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        ProcessarAcerto(other);
    }

    private void ProcessarAcerto(Collider2D other)
    {
        if (other == null)
            return;

        int dano = 0;
        bool acertou = false;

        if (swordHitbox1 != null &&
            swordHitbox1.enabled &&
            swordHitbox1.IsTouching(other))
        {
            dano = danoAtaque1;
            acertou = true;
        }
        else if (swordHitbox2 != null &&
                 swordHitbox2.enabled &&
                 swordHitbox2.IsTouching(other))
        {
            dano = danoAtaque2;
            acertou = true;
        }
        else if (swordHitbox3 != null &&
                 swordHitbox3.enabled &&
                 swordHitbox3.IsTouching(other))
        {
            dano = danoAtaque3;
            acertou = true;
        }

        if (!acertou)
            return;

        HeartSystem vidaInimigo =
            other.GetComponentInParent<HeartSystem>();

        if (vidaInimigo == null)
            return;

        if (inimigosAtingidos.Contains(vidaInimigo))
            return;

        inimigosAtingidos.Add(vidaInimigo);

        vidaInimigo.ReceberDano(dano);
    }

    void OnDestroy()
    {
        if (semAtrito != null)
            Destroy(semAtrito);
    }


    IEnumerator IgnorarColisaoAteAterrissar(
        Collider2D plataforma)
    {
        if (plataforma == null)
            yield break;

        Physics2D.IgnoreCollision(
            playerCollider,
            plataforma,
            true
        );

        PlataformaMovel movimento =
            plataforma.GetComponentInParent<PlataformaMovel>();

        while (playerCollider != null && plataforma != null)
        {
            float velocidadePlataforma = movimento != null
                ? movimento.VelocidadeVerticalAtual
                : 0f;

            bool sobrePlataforma =
                playerCollider.bounds.max.x > plataforma.bounds.min.x &&
                playerCollider.bounds.min.x < plataforma.bounds.max.x;

            bool acimaDaPlataforma =
                playerCollider.bounds.min.y >
                plataforma.bounds.max.y + 0.05f;

            bool descendoRelativo =
                rig.linearVelocity.y <= velocidadePlataforma;

            if (!sobrePlataforma ||
                (descendoRelativo && acimaDaPlataforma))
            {
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        if (playerCollider != null && plataforma != null)
        {
            Physics2D.IgnoreCollision(
                playerCollider,
                plataforma,
                false
            );
        }
    }


}
