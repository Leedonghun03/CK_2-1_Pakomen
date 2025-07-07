using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hedonism
{
    public interface IRunnable
    {
        void Start();       //작업을 맡겼을 때, 가장 처음 호출되는 함수.
                            //초기화 작업 권장.
        void Run();         //Start() 이후 호출될 함수.
                            //실질 작업 권장.
        void Exit();        //Run의 작업이 모두 끝난 후 호출될 함수.
    } 


    public class ThreadManager : Singleton<ThreadManager>
    {
        private volatile bool isRunning = true;

        private readonly ConcurrentQueue<IRunnable> taskQueue = new ConcurrentQueue<IRunnable>();
        private Thread[] workerThreads;

        //===================================================================================================================================================
        //▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        /*
        1. IRunnable을 상속받고 
        2. 인터페이스인 Start(), Run(), Exit() 함수를 정의한 후, 
        3. RunTask에 해당 오브젝트를 넣어주기.
         */
        public void RunTask(IRunnable task)
        {
            taskQueue.Enqueue(task);

        }

        //이 아래로는 무시.
        //▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        //===================================================================================================================================================

        public void Initialize(int defaultThreadCount)
        {
            workerThreads = new Thread[defaultThreadCount];

            for (int i = 0; i < defaultThreadCount; ++i) 
            {
                workerThreads[i] = new Thread(ThreadsLoop);
                workerThreads[i].IsBackground = true;
                workerThreads[i].Start();

            }

        }

        public void Shutdown()
        {
            isRunning = false;

        }

        private void ThreadsLoop()
        {
            while (true == isRunning)
            {
                if (true == taskQueue.TryDequeue(out IRunnable task))
                {
                    task.Start();
                    task.Run();
                    task.Exit();
                }
                else
                {
                    Thread.Sleep(1);    //when threads are lightly utilized
                    //Thread.Yield();   //when threads are heavily utilized
                }
            }
        }


    }


}
