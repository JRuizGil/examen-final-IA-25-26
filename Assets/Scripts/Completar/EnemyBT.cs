// ★ EXAMEN — Conecta los nodos y construye el árbol de comportamiento del enemigo
using UnityEngine;
using UnityEngine.AI;
using MazeLab.BT;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBT : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;

    [Header("Detection")]
    public float detectionRadius = 7f;

    // ── Privados ─────────────────────────────────────────────────────────────
    private NavMeshAgent _agent;
    private Blackboard   _bb;
    private BTNode       _tree;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _bb    = new Blackboard();

        // Busca al jugador por tag al inicio
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            _bb.playerTransform = playerGO.transform;
        else
            Debug.LogWarning("[EnemyBT] No se encontró objeto con tag 'Player'.");
    }

    void Start()
    {
        _tree = BuildTree();
    }

    void Update()
    {
        _tree?.Tick();
    }

    // ════════════════════════════════════════════════════════════════════════
    // TODO D — BuildTree()
    //   Construye y devuelve la raíz del Behaviour Tree.
    //
    //   Estructura esperada (Selector raíz):
    //
    //     Selector
    //     ├─ Sequence  (perseguir)
    //     │   ├─ DetectPlayerNode
    //     │   └─ ChasePlayerNode
    //     └─ PatrolNode  (patrullar si no detecta)
    //
    //   Pistas:
    //     - new Selector( nodo1, nodo2, ... )
    //     - new Sequence( nodo1, nodo2, ... )
    //     - Los nodos necesitan _bb, _agent, transform, detectionRadius, patrolPoints
    // ════════════════════════════════════════════════════════════════════════
    BTNode BuildTree()
    {
        // TODO: implementar y devolver la raíz del árbol
        _tree = new Selector(new Sequence(new DetectPlayerNode(_bb, transform, detectionRadius),new ChasePlayerNode(_bb, _agent)), new PatrolNode(_bb, _agent, patrolPoints));
        return _tree; // placeholder — el enemigo no se moverá hasta que lo implementes
        

    }

    // Gizmo para visualizar el radio de detección en la Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
