// Infraestructura — NO modificar
using UnityEngine;

namespace MazeLab.BT
{
    // Datos compartidos entre los nodos del BT
    public class Blackboard
    {
        public Transform playerTransform;
        public bool playerDetected;
        public int  currentWaypointIndex;
    }
}
