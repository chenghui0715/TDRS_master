using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.ICMS.Common
{
    public class SingletonBase<T> where T : class, new()
    {
        protected static T m_Instance;
        protected static object m_Lock = new object();

        protected SingletonBase() { }

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (m_Lock)
                    {
                        if (m_Instance == null)
                            m_Instance = new T();
                    }
                }

                return m_Instance;
            }
        }
    }
}
