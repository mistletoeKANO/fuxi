using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace FuXi
{
    internal enum AwaitState
    {
        Fault,
        Succeed,
        Pending,
    }
    [AsyncMethodBuilder(typeof(FAsyncTaskMethodBuilder))]
    public class FTask : ICriticalNotifyCompletion
    {
        private static readonly Queue<FTask> TaskPool = new Queue<FTask>();
        private AwaitState state = AwaitState.Pending;
        private bool fromPool;
        private object action;
        private FTask() { }
        public static FTask Create(bool fromPool = false)
        {
            if (!fromPool) return new FTask();
            return TaskPool.Count == 0 ? new FTask {fromPool = true} : TaskPool.Dequeue();
        }

        private void Recycle()
        {
            if (!this.fromPool) return;
            this.state = AwaitState.Pending;
            this.action = null;
            TaskPool.Enqueue(this);
            if (TaskPool.Count > 1000) TaskPool.Clear();
        }
        
        [DebuggerHidden]
        public FTask GetAwaiter() { return this; }
        [DebuggerHidden]
        public bool IsCompleted => this.state != AwaitState.Pending;
        [DebuggerHidden]
        public void OnCompleted(Action continuation)
        {
            this.UnsafeOnCompleted(continuation);
        }
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action continuation)
        {
            if (this.state != AwaitState.Pending)
            {
                continuation?.Invoke();
                return;
            }
            this.action = continuation;
        }
        [DebuggerHidden]
        public void GetResult()
        {
            switch (this.state)
            {
                case AwaitState.Succeed:
                    this.Recycle();
                    break;
                case AwaitState.Fault:
                    ExceptionDispatchInfo c = this.action as ExceptionDispatchInfo;
                    this.action = default;
                    this.Recycle();
                    c?.Throw();
                    break;
                default:
                    throw new NotSupportedException("Cant get result when no use await in this FTask.");
            }
        }
        [DebuggerHidden]
        public void SetResult()
        {
            if (this.state != AwaitState.Pending)
            {
                throw new InvalidOperationException("This FTask is finished, cant set repeated.");
            }
            this.state = AwaitState.Succeed;
            Action c = this.action as Action;
            this.action = null;
            c?.Invoke();
        }
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (this.state != AwaitState.Pending)
            {
                throw new InvalidOperationException("This FTask is finished, cant set repeated.");
            }
            this.state = AwaitState.Fault;
            Action c = this.action as Action;
            this.action = ExceptionDispatchInfo.Capture(e);
            c?.Invoke();
        }
    }
    
    [AsyncMethodBuilder(typeof(FAsyncTaskMethodBuilder<>))]
    public class FTask<T> : ICriticalNotifyCompletion
    {
        private static readonly Queue<FTask<T>> TaskPool = new Queue<FTask<T>>();
        private AwaitState state = AwaitState.Pending;
        private object action;
        private bool fromPool;
        private T value;

        private FTask() { }
        public static FTask<T> Create(bool fromPool = false)
        {
            if (!fromPool) return new FTask<T>();
            return TaskPool.Count == 0 ? new FTask<T>{fromPool = true} : TaskPool.Dequeue();
        }

        private void Recycle()
        {
            if (!this.fromPool) return;
            this.state = AwaitState.Pending;
            this.action = default;
            this.value = default;
            TaskPool.Enqueue(this);
            if (TaskPool.Count > 1000) TaskPool.Clear();
        }
        [DebuggerHidden]
        public bool IsCompleted => this.state != AwaitState.Pending;
        [DebuggerHidden]
        public FTask<T> GetAwaiter() { return this;}
        [DebuggerHidden]
        public T GetResult()
        {
            switch (this.state)
            {
                case AwaitState.Succeed:
                    var result = this.value;
                    this.Recycle();
                    return result;
                case AwaitState.Fault:
                    ExceptionDispatchInfo c = this.action as ExceptionDispatchInfo;
                    this.action = default;
                    this.value = default;
                    this.Recycle();
                    c?.Throw();
                    return default;
                default:
                    throw new NotSupportedException("Cant get result when no use await in this FTask.");
                
            }
        }
        [DebuggerHidden]
        public void SetResult(T v)
        {
            if (this.state != AwaitState.Pending)
            {
                throw new InvalidOperationException("This FTask is finished, cant set repeated.");
            }
            this.value = v;
            this.state = AwaitState.Succeed;
            var ac = this.action as Action;
            this.action = default;
            ac?.Invoke();
        }
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (this.state != AwaitState.Pending)
            {
                throw new InvalidOperationException("This FTask is finished, cant set repeated.");
            }
            this.state = AwaitState.Fault;
            var ac = this.action as Action;
            this.action = ExceptionDispatchInfo.Capture(e);
            ac?.Invoke();
        }
        [DebuggerHidden]
        public void OnCompleted(Action continuation)
        {
            this.UnsafeOnCompleted(continuation);
        }
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action continuation)
        {
            if (this.state != AwaitState.Pending)
            {
                continuation?.Invoke();
                return;
            }
            this.action = continuation;
        }
    }
}