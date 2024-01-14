using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public static IEnumerable AsyncLoad(string asset)
    {
        throw new NotImplementedException();
        //ResourceLoader.ThreadLoadStatus Status;
        //do
        //{
        //    Status = ResourceLoader.LoadThreadedGetStatus(asset);
        //    switch (Status)
        //    {
        //        case ResourceLoader.ThreadLoadStatus.Failed:
        //            GD.PrintErr($"Failed to load asset {asset}");
        //            yield break;

        //        case ResourceLoader.ThreadLoadStatus.InvalidResource:
        //            GD.PrintErr($"Invalid resource {asset}");
        //            yield break;
        //    }
        //    if(Status != ResourceLoader.ThreadLoadStatus.Loaded)
        //    {
        //        yield return NextFrame();
        //    }
        //} while (Status != ResourceLoader.ThreadLoadStatus.Loaded);
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
            
            do
            {
                //Try to see if we have a corotuine to run.  If not, we break the loop.
                if(Stack.TryPeek(out IEnumerator? coro))
                {
                    //If our coro is waiting on a waiter, check if that waiter is ready.  If it is, move forward 
                    if (coro.Current is ICoroutineWaiter waiter)
                    {
                        //If we're done with this coro, pop off the top of the stack and see what we continue to next time around this loop.
                        if (waiter.Terminate)
                        {
                            Stack.Pop();
                        }

                        if (waiter.Ready)
                        {
                            //Try to progress the coro.  If we progress, we come back around to see if the next part is a waiter.
                            //It could be a subcoro, or something random.  Either way, try to find out whats next.
                            //If we can't continue, we're done with this coro so see what the parent does. 
                            if (!coro.MoveNext()) 
                            {
                                Stack.Pop();
                            }
                        }
                        //If we're not ready, break the loop.  We'll come back around next frame.
                        else
                        {
                            break;
                        }
                    }
                    //If we have a subcoro, lets see what we've got for us in the coro.
                    else if (coro.Current is IEnumerator sub_coro)
                    {
                        do
                        {
                            //If we're null, lets check if we can move forward.  If we can, push the subcoro onto the stack.
                            if (sub_coro.Current == null)
                            {
                                if (sub_coro.MoveNext())
                                {
                                    Stack.Push(sub_coro);
                                    break;
                                }
                                else
                                {
                                    //If we're over the end of the subcoro, just progress the parent and go back around the loop.
                                    if(!coro.MoveNext())                                  
                                    {
                                        //If the parent is done, pop it off the stack.
                                        Stack.Pop();
                                    }
                                    break;
                                }
                            }
                            //If we have a value here, put the subcoro on the stack and go around the loop again (so it's checked).
                            else if (sub_coro.Current is IEnumerator || sub_coro.Current is ICoroutineWaiter)
                            {
                                Stack.Push(sub_coro);
                                break;
                            }
                            //if we're not a subcoro or a waiter, we're a value.  Progress the subcoro check what we get next.
                            else
                            {
                                if (!sub_coro.MoveNext())
                                {
                                    break;
                                }
                            }
                        } while (true);                     
                       
                    }
                    else
                    {
                        Stack.Pop();
                    }
                }
                else
                {
                    break;
                }

            } while (true);           
           

            return Stack.Count != 0;
        }
    }
    private static List<Runner> coroutineQueue = new();
    private static Queue<Action> actions = new Queue<Action>();

    public static void Start(IEnumerator coroutine)
    {        
        if(!coroutine.MoveNext())
        {
            return;
        }

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
