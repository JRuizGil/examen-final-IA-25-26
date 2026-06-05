using UnityEngine;
using UnityEngine.AI;
using MazeLab.BT;

// ★ EXAMEN
// La trampa usa una Maquina de Estados Finita (FSM) con cuatro estados.
// Lee los comentarios de cada TODO antes de escribir codigo.

public class TrapFSM : MonoBehaviour
{
    // ── Configuracion publica ────────────────────────────────────────────────
    [Header("Timing")]
    public float activeToActivatedDelay = 1.5f;  // segundos en Active antes de disparar
    public float activatedDuration      = 2f;    // segundos que dura el estado Activated
    public float inactiveDuration       = 4f;    // segundos de cooldown en Inactive

    [Header("Damage")]
    public float damageAmount = 20f;

    [Header("Visual")]
    public Renderer trapRenderer;   // asignado desde el Inspector

    // ── Paleta de colores ────────────────────────────────────────────────────
    static readonly Color ColorObserving = Color.green;
    static readonly Color ColorActive    = Color.yellow;
    static readonly Color ColorActivated = Color.red;
    static readonly Color ColorInactive  = Color.gray;

    // ── Estado interno ───────────────────────────────────────────────────────
    public enum State { Observing, Active, Activated, Inactive }
    public State currentState { get; private set; }

    private float _timer;
    private PlayerHealth _playerInTrigger;

    // ════════════════════════════════════════════════════════════════════════
    // TODO 1 — Start()
    //   Inicializa la FSM en el estado Observing.
    //   Llama a EnterState(State.Observing) para que el color se aplique.
    // ════════════════════════════════════════════════════════════════════════
    void Start()
    {
        // TODO: implementar

        EnterState(State.Observing); // placeholder
    }

    // ════════════════════════════════════════════════════════════════════════
    // TODO 2 — Update()
    //   Ejecuta la lógica del estado activo cada frame.
    //   Patrón recomendado: switch(currentState) { case ...: Tick...(); break; }
    // ════════════════════════════════════════════════════════════════════════
    void Update()
    {
        switch (currentState)
        {
            case State.Observing:
                break;
            case State.Active:
                TickActive();
                break;
            case State.Activated:
                TickActivated();
                break;
            case State.Inactive:
                TickInactive();
                break;
        }
        // TODO: implementar
    }

    // ════════════════════════════════════════════════════════════════════════
    // TODO 3 — TickActive()
    //   Incrementa _timer con Time.deltaTime.
    //   Cuando _timer >= activeToActivatedDelay transiciona a Activated.
    // ════════════════════════════════════════════════════════════════════════
    void TickActive()
    {
        // TODO: implementar

        _timer += Time.deltaTime;
        if (_timer >= activeToActivatedDelay)
            EnterState(State.Activated);
        
    }

    // ════════════════════════════════════════════════════════════════════════
    // TODO 4 — TickActivated()
    //   Aplica daño continuo al jugador (damageAmount * Time.deltaTime).
    //   Comprueba que _playerInTrigger != null antes de llamar a TakeDamage
    //   (el jugador puede haber salido del trigger entre frames).
    //   Incrementa _timer; cuando _timer >= activatedDuration va a Inactive.
    // ════════════════════════════════════════════════════════════════════════
    void TickActivated()
    {   
        // aplicar daño continuo

        if(_playerInTrigger != null)
            _playerInTrigger.TakeDamage(damageAmount * Time.deltaTime);


        // TODO: implementar
        _timer += Time.deltaTime;
        if (_timer >= activatedDuration)
            EnterState(State.Inactive);
    }

    // TickInactive — ya implementado como referencia
    void TickInactive()
    {
        _timer += Time.deltaTime;
        if (_timer >= inactiveDuration)
            EnterState(State.Observing);

    }

    // ════════════════════════════════════════════════════════════════════════
    // TODO 5 — EnterState(State next)
    //   Cambia currentState, resetea _timer a 0 y aplica el color correcto
    //   al trapRenderer usando la paleta estatica de arriba.
    //   Pista: trapRenderer.material.color = ColorObserving;  // usa la paleta según el estado
    // ════════════════════════════════════════════════════════════════════════
    void EnterState(State next)
    {
        // TODO: implementar
        currentState = next;
        _timer = 0f;
        switch (currentState)
        {
            case State.Observing:
                trapRenderer.material.color = ColorObserving;
                break;
            case State.Active:
                trapRenderer.material.color = ColorActive;
                break;
            case State.Activated:
                trapRenderer.material.color = ColorActivated;
                break;
            case State.Inactive:
                trapRenderer.material.color = ColorInactive;
                break;
        }


    }

    // ── Deteccion de trigger ─────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (currentState != State.Observing) return;
        if (!other.CompareTag("Player")) return;

        _playerInTrigger = other.GetComponent<PlayerHealth>();

        // ════════════════════════════════════════════════════════════════════
        // TODO 6 — Transiciona al estado Active cuando el jugador entra.
        // ════════════════════════════════════════════════════════════════════
        // TODO: implementar
        EnterState(State.Active);

    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInTrigger = null;

        // Si el jugador sale mientras esta en Active, vuelve a Observing
        if (currentState == State.Active)
            EnterState(State.Observing);
        
    }
}