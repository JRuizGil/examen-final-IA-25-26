// Infraestructura del Behaviour Tree — NO modificar
// Los alumnos solo trabajan en EnemyBT.cs y EnemyNodes.cs

namespace MazeLab.BT
{
    public enum NodeResult { Success, Failure, Running }

    public abstract class BTNode
    {
        public abstract NodeResult Tick();
    }

    // Ejecuta hijos en orden; falla en cuanto uno falla
    public class Sequence : BTNode
    {
        private readonly BTNode[] _children;
        public Sequence(params BTNode[] children) { _children = children; }

        public override NodeResult Tick()
        {
            foreach (var child in _children)
            {
                var r = child.Tick();
                if (r != NodeResult.Success) return r;
            }
            return NodeResult.Success;
        }
    }

    // Ejecuta hijos en orden; tiene exito en cuanto uno tiene exito
    public class Selector : BTNode
    {
        private readonly BTNode[] _children;
        public Selector(params BTNode[] children) { _children = children; }

        public override NodeResult Tick()
        {
            foreach (var child in _children)
            {
                var r = child.Tick();
                if (r != NodeResult.Failure) return r;
            }
            return NodeResult.Failure;
        }
    }
}
