using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace ZECS.Common.Facility
{
    public class AsynchronousCall
    {
        public delegate void AsyncCaller(Object input, out Object output);

        public bool StartAsynchronousCall(AsyncCaller caller, Object input)
        {
            m_output = null;
            AsyncCaller callDelegate = new AsyncCaller(caller);
            // Asynchronously invoke the Factorize method.
            Object output = null;
            AsyncCallback callBack = new AsyncCallback(this.GetResultsCallback);
            //Object temp = null;
            IAsyncResult result = callDelegate.BeginInvoke(
                                 input,
                                 out output,
                                 callBack, callDelegate);
            return true;
        }
        public bool GetResult(out Object output)
        {
            output = null;
            if (!m_waiter.WaitOne(0))
                return false;
            else
            {
                output = m_output;
                return true;
            }
        }
        public bool IsAsynchronousCallRunning()
        {
            return !m_waiter.WaitOne(0);
        }
        void GetResultsCallback(IAsyncResult result)
        {
            //result.AsyncState as AsyncCaller;
            AsyncCaller asyncDelegate = result.AsyncState as AsyncCaller;//(AsyncCaller)((AsyncResult)result).AsyncDelegate;
            if (asyncDelegate != null)
                asyncDelegate.EndInvoke(out m_output, result);
            m_waiter.Set();
        }

        ManualResetEvent m_waiter = new ManualResetEvent(false);
        Object m_output = null;
    }
    public class AsynchronousCall<T1, T2>
    {
        public delegate void AsyncCaller(T1 input, out T2 output);

        public bool StartAsynchronousCall(AsyncCaller caller, T1 input)
        {
            m_waiter.Reset();
            m_output = default(T2);
            AsyncCaller callDelegate = new AsyncCaller(caller);
            // Asynchronously invoke the Factorize method.
            T2 output = default(T2);
            AsyncCallback callBack = new AsyncCallback(this.GetResultsCallback);
            Object temp = null;
            IAsyncResult result = callDelegate.BeginInvoke(
                                 input,
                                 out output,
                                 callBack, temp);
            return true;
        }
        public bool GetResult(out T2 output)
        {
            output = default(T2);
            if (!m_waiter.WaitOne(0))
                return false;
            else
            {
                output = m_output;
                return true;
            }
        }
        void GetResultsCallback(IAsyncResult result)
        {
            AsyncCaller asyncDelegate = (AsyncCaller)((AsyncResult)result).AsyncDelegate;
            asyncDelegate.EndInvoke(out m_output, result);

            m_waiter.Set();
        }

        ManualResetEvent m_waiter = new ManualResetEvent(false);
        T2 m_output = default(T2);
    }


    public class Example
    {
        class InputClass
        {
            public int n = 0;
        }
        class OutputClass
        {
            public int n = 0;
        }
        /// <summary>
        /// 异步调用MyFunc，在GetResult获取MyFunc的执行结果
        /// </summary>
        public void AsyncCallMyFunc()
        {
            InputClass input = new InputClass();
            input.n = 1;
            AsynchronousCall<InputClass, OutputClass> call = new AsynchronousCall<InputClass, OutputClass>();

            call.StartAsynchronousCall(MyFunc, input);
            OutputClass output;

            while (!call.GetResult(out output))
            {
                System.Diagnostics.Debug.Print("fail\n");
                System.Threading.Thread.Sleep(1000);
            }

            System.Diagnostics.Debug.Print("ok\n");
        }

        void MyFunc(InputClass input, out OutputClass output)
        {
            output = new OutputClass();
            output.n = input.n;
            System.Threading.Thread.Sleep(10000);
            //return true;
        }
    }
}
