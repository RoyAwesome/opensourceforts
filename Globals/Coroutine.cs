using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSSForts
{
    public static class Coroutine
    {
        private static List<IEnumerator> coroutineQueue = new List<IEnumerator>();
        private static Queue<Action> actions = new Queue<Action>();

        public static void Add(IEnumerator coroutine)
        {
            coroutineQueue.Add(coroutine);
        }

        public static void NextFrame(Action action)
        {
            actions.Enqueue(action);
        }    

        public static void Process()
        {
            for(int i = 0; i < coroutineQueue.Count; i++)
            {
                if (!coroutineQueue[i].MoveNext())
                {
                    coroutineQueue.RemoveAt(i);
                    i--;
                }
            }

            while(actions.TryDequeue(out var action))
            {
                action();
            }   
        }
    }
}
