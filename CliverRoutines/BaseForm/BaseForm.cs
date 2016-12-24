﻿//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        27 February 2007
//Copyright: (C) 2013, Sergey Stoyan
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;

namespace Cliver
{
    public partial class BaseForm : Form
    {
        public BaseForm()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
        }
    }

    public static class ControlRoutines
    {
        /// <summary>
        /// Set text to the control thread-safely.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="m"></param>
        public static void SetText(this Control c, string m)
        {
            if (c.InvokeRequired)
                c.BeginInvoke(new MethodInvoker(() => { c.Text = m; }));
            else
                c.Text = m;
        }

        public static object Invoke(this Control c, Func<object> function)
        {
            return c.Invoke(function);
        }

        public static void Invoke(this Control c, MethodInvoker code)
        {
            c.Invoke(code);
        }

        public static void BeginInvoke(this Control c, MethodInvoker code)
        {
            c.BeginInvoke(code);
        }

        public static object InvokeFromUiThread(Delegate d)
        {
            return Application.OpenForms[0].Invoke(d);
        }

        public static Thread SlideVertically(this Control c, double pixelsPerMss, int p2, MethodInvoker finished = null)
        {
            lock (c)
            {
                Thread t = null;
                //if (controls2sliding_thread.TryGetValue(c, out t) && t.IsAlive)
                //    return t;

                int delta = c.Top > p2 ? -1 : 1;
                double time = Math.Abs(p2 - c.Top) / pixelsPerMss;
                int sleep = (int)(time / ((p2 - c.Top) / delta));
                t = ThreadRoutines.Start(() =>
                {
                    try
                    {
                        while (c.Visible && !(bool)ControlRoutines.Invoke(c, () =>
                            {
                                c.Top = c.Top + delta;
                                return delta < 0 ? c.Top <= p2 : c.Top >= p2;
                            })
                        )
                            System.Threading.Thread.Sleep(sleep);
                        ControlRoutines.Invoke(c, () =>
                            {
                                finished?.Invoke();
                            });
                    }
                    catch(Exception e)//control disposed
                    {
                    }
                });
                //controls2sliding_thread[c] = t;
                return t;
            }
        }
        //static readonly  Dictionary<Control, Thread> controls2sliding_thread = new Dictionary<Control, Thread>();

        public static Thread Condense(this Form f, double centOpacityPerMss, double o2, MethodInvoker finished = null)
        {
            lock (f)
            {
                Thread t = null;
                //if (controls2condensing_thread.TryGetValue(f, out t) && t.IsAlive)
                //    return t;

                double delta = f.Opacity < o2 ? 0.01 : -0.01;
                double time = Math.Abs(o2 - f.Opacity) / (centOpacityPerMss / 100);
                int sleep = (int)(time / ((o2 - f.Opacity) / delta));
                t = ThreadRoutines.Start(() =>
                {
                    try
                    {
                        while (!(bool)ControlRoutines.Invoke(f, () =>
                            {
                                f.Opacity = f.Opacity + delta;
                                return delta > 0 ? f.Opacity >= o2 : f.Opacity <= o2;
                            })
                        )
                            System.Threading.Thread.Sleep(sleep);
                        ControlRoutines.Invoke(f, () =>
                        {
                            finished?.Invoke();
                        });
                    }
                    catch (Exception e)//control disposed
                    {
                    }
                });
                //controls2condensing_thread[f] = t;
                return t;
            }
        }
        //static readonly Dictionary<Form, Thread> controls2condensing_thread = new Dictionary<Form, Thread>();
    }
}

