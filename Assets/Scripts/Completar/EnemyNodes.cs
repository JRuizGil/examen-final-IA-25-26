// ★ EXAMEN — Implementa los nodos del Behaviour Tree del enemigo
using UnityEngine;
using UnityEngine.AI;
using MazeLab.BT;

// ════════════════════════════════════════════════════════════════════════════
// TODO A — DetectPlayerNode
//   Condición: el jugador está dentro del radio de detección del enemigo.
//
//   Datos disponibles:
//     bb.playerTransform   — Transform del jugador (buscado por tag "Player")
//     detectionRadius      — radio de detección (float)
//     _enemy               — Transform del enemigo (campo privado del nodo)
//
//   Lógica esperada:
//     1. Si playerTransform es null → return Failure
//     2. Calcula la distancia entre el enemigo y el jugador.
//     3. Si distancia <= detectionRadius:
//          bb.playerDetected = true  → return Success
//        Si no:
//          bb.playerDetected = false → return Failure
// ════════════════════════════════════════════════════════════════════════════
public class DetectPlayerNode : BTNode
{
    private readonly Blackboard  _bb;
    private readonly Transform   _enemy;
    private readonly float       _radius;

    public DetectPlayerNode(Blackboard bb, Transform enemy, float radius)
    {
        _bb     = bb;
        _enemy  = enemy;
        _radius = radius;
    }

    public override NodeResult Tick()
    {
        // TODO: implementar según las instrucciones del encabezado

        return NodeResult.Failure; // placeholder
    }
}

// ════════════════════════════════════════════════════════════════════════════
// TODO B — ChasePlayerNode
//   Acción: mueve el NavMeshAgent hacia el jugador mientras esté detectado.
//
//   Datos disponibles:
//     bb.playerDetected    — bool
//     bb.playerTransform   — destino
//     agent                — NavMeshAgent del enemigo
//
//   Lógica esperada:
//     1. Si !bb.playerDetected → return Failure
//        (el Selector ya no ejecutará este nodo si DetectPlayerNode falla,
//         pero la comprobación hace el nodo robusto de forma independiente)
//     2. agent.SetDestination(bb.playerTransform.position)
//     3. return Running
// ════════════════════════════════════════════════════════════════════════════
public class ChasePlayerNode : BTNode
{
    private readonly Blackboard  _bb;
    private readonly NavMeshAgent _agent;

    public ChasePlayerNode(Blackboard bb, NavMeshAgent agent)
    {
        _bb    = bb;
        _agent = agent;
    }

    public override NodeResult Tick()
    {
        // TODO: implementar según las instrucciones del encabezado
        return NodeResult.Failure; // placeholder
    }
}

// ════════════════════════════════════════════════════════════════════════════
// TODO C — PatrolNode
//   Acción: recorre los waypoints en orden cíclico.
//
//   Datos disponibles:
//     bb.currentWaypointIndex — índice del waypoint actual
//     patrolPoints[]          — array de Transforms
//     agent                   — NavMeshAgent
//     waypointReachedDist     — distancia para considerar que llegó (ej: 0.5f)
//
//   Lógica esperada:
//     1. Si patrolPoints está vacío → return Failure
//     2. Obtiene el waypoint destino: patrolPoints[bb.currentWaypointIndex]
//     3. agent.SetDestination(destino.position)
//     4. Si el agente ha llegado al destino:
//          !agent.pathPending && agent.remainingDistance <= waypointReachedDist
//          → bb.currentWaypointIndex = (bb.currentWaypointIndex + 1) % patrolPoints.Length
//     5. return Running
// ════════════════════════════════════════════════════════════════════════════
public class PatrolNode : BTNode
{
    private readonly Blackboard   _bb;
    private readonly NavMeshAgent _agent;
    private readonly Transform[]  _points;
    private readonly float        _reached;

    public PatrolNode(Blackboard bb, NavMeshAgent agent, Transform[] points, float reachedDist = 0.5f)
    {
        _bb      = bb;
        _agent   = agent;
        _points  = points;
        _reached = reachedDist;
    }

    public override NodeResult Tick()
    {
        for (int i = 0; i < _points.Length; i++)
        {
            if (_points[i] == null)
            {
                Debug.LogWarning($"[PatrolNode] Waypoint {i} es null. Ignorando este waypoint.");
                return NodeResult.Failure;
            }
        }
        if (_points.Length == 0)
            return NodeResult.Failure;
        Transform dest = _points[_bb.currentWaypointIndex];
        _agent.SetDestination(dest.position);
        if (!_agent.pathPending && _agent.remainingDistance <= _reached)
            _bb.currentWaypointIndex = (_bb.currentWaypointIndex + 1) % _points.Length;
        return NodeResult.Running;// placeholder

    }
}
