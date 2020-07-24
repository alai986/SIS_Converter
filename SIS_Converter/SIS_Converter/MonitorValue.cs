using System;

using System.Timers;

namespace SIS_Converter
{
    public class DoEvent
    {
        //需要监控的当前状态属性
        private static string currentState = "";
        //主动监控的状态保留属性
        private static string oldState = currentState;
        //主动监控的Timer
        Timer timer;
        /// <summary>
        /// 初始化默认值并绑定事件处理函数
        /// </summary>
        /// <param name="interval">默认值可为空（单位为毫秒）</param>
        public DoEvent(int? interval)
        {
            interval = interval ?? 1000;//为空将1000赋值过去，否则不动
            OnMyStateChanged += new MyStateChanged(DoEvent_BeforeStateChanged);
            timer = new Timer((int)interval);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //如果值发生变化则调用事件触发函数
            if (currentState != oldState)
            {
                oldState = currentState;
                timer.Enabled = false;//暂停监控
                DoTaskWork();//做你的任务
                timer.Enabled = true;//继续监控
            }
        }

        //我的延时任务（例如定时到每天晚上4:00生成耗时的索引）
        private void  DoTaskWork()
        {
            Console.WriteLine("1234");
            //do something
        }

        //定义一个委托
        private delegate void MyStateChanged(object sender, EventArgs e);
        //定义一个委托关链的事件
        private event MyStateChanged OnMyStateChanged;

        //事件处理函数，属性值修改前的操作法
        private void DoEvent_BeforeStateChanged(object sender, EventArgs e)
        {
            //Console.WriteLine("开始");
            //do something
        }

        //事件触发函数
        private void WhenMyStateChange()
        {
            if (OnMyStateChanged != null)
            {
                OnMyStateChanged(this, null);
                //任务监听启动
                timer.Enabled = true;
            }
        }
        //属性设置，提供给外部修改触发
        public string MyState
        {
            get { return currentState; }
            set
            {
                //如果值发生变化前则调用事件触发函数（2011.1.7补充说明：大家可以随意调整位置，移下一行代码则就变成改变值后触发。）
                if (currentState != value)
                {
                    WhenMyStateChange();
                }
                currentState = value;
            }
        }

    }
}
