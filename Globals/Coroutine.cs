using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSSForts;

public interface ICoroutineWaiter
{
    bool Ready
    {
        get;
    }  

    bool Terminate
    {
        get;
    }
}

public static class Wait
{
    private class WaitFrames : ICoroutineWaiter
    {
        public WaitFrames(int frames)
        {
            Frames = frames;
        }

        public int Frames;
        public bool Ready => Frames-- <= 0;
        public bool Terminate => false;
    }
    public static ICoroutineWaiter Frames(int frames)
    {
        return new WaitFrames(frames);
    }

    public static ICoroutineWaiter NextFrame()
    {
        return new WaitFrames(1);
    }

    private class Timer : ICoroutineWaiter
    {
        private DateTime End;
        private DateTime Start;
        public Timer(TimeSpan time)
        {            
            Start = DateTime.Now;
            End = Start.Add(time);
        }

        public bool Ready => DateTime.Now >= End;
        public bool Terminate => false;
    }

    public static ICoroutineWaiter ForSeconds(double time)
    {
        return new Timer(TimeSpan.FromSeconds(time));
    }

    public static ICoroutineWaiter ForTimespan(TimeSpan time)
    {
        return new Timer(time);
    }
}

public static class Coroutine
{
    private class Runner
    {
        public Runner(IEnumerator coroutine)
        {
            Stack.Push(coroutine);
        }
        public Stack<IEnumerator> Stack = new();
        public bool ShouldCancel = false;

        public bool RunOnce()
        {
            if(ShouldCancel)
            {
                return false;
            }

            if(Stack.Count == 0)
            {
                return false;
            }

            var coro = Stack.Peek();
            if(coro.Current == null)
            {
                coro.MoveNext();
            }

            if (coro.Current is ICoroutineWaiter waiter)
            {
                if (waiter.Terminate)
                {
                    Stack.Pop();
                }
                else if (waiter.Ready)
                {
                    if (coro.MoveNext())
                    {
                        if(coro.Current is IEnumerator sub_coro)
                        {
                            Stack.Push(sub_coro);
                        }
                        return true;
                    }
                    else
                    {
                        Stack.Pop();
                        if(Stack.TryPeek(out IEnumerator? parentCoro))
                        {
                            parentCoro.MoveNext();
                            if (coro.Current is IEnumerator sub_coro)
                            {
                                Stack.Push(sub_coro);
                            }
                            return true;
                        }
                    }
                }
            }           

            return Stack.Count != 0;
        }
    }
    private static List<Runner> coroutineQueue = new();
    private static Queue<Action> actions = new Queue<Action>();

    public static void Start(IEnumerator coroutine)
    {
        coroutineQueue.Add(new(coroutine));
    }

    public static void NextFrame(Action action)
    {
        actions.Enqueue(action);
    }    

    public static void Process(double delta_time)
    {
        for(int i = 0; i < coroutineQueue.Count; i++)
        {
            Runner CoroRun = coroutineQueue[i];         

            if (!CoroRun.RunOnce())
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
