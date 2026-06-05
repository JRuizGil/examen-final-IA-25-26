using UnityEngine;
using UnityEngine.AI;

public class GoalTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[MazeLab] ¡El jugador ha llegado al Goal! Nivel completado.");

        foreach (var agent in FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None))
            agent.isStopped = true;

        foreach (var enemy in FindObjectsByType<EnemyBT>(FindObjectsSortMode.None))
            enemy.enabled = false;
    }
}
