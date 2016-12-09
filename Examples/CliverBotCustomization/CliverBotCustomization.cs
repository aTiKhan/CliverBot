﻿//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey_stoyan@yahoo.com
//        http://www.cliversoft.com
//Copyright: (C) 2006-2013, Sergey Stoyan
//********************************************************************************************

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Text;
using System.Threading;
using Cliver.Bot;
using Cliver.BotGui;
using System.Windows.Forms;

namespace CliverBotCustomization
{
    /// <summary>
    /// Most important interface that defines certain routines of CliverBot customization.
    /// </summary>
    public class CustomBot : Cliver.Bot.Bot
    {
        [STAThread]
        static void Main()
        {
            Cliver.Config.Initialize(new string[] { "Engine", "Input", "Output", "Web", "Spider", "Log" });
            Cliver.BotGui.BotGui.ConfigControlSections = new string[] { "Engine", "Input", "Output", "Web", "Spider", "Log", };
            Cliver.BotGui.BotGui.BotThreadControlType = typeof(WebRoutineBotThreadControl);

            //Cliver.Bot.Program.Run();//It is the entry when the app runs as a console app.
            Cliver.BotGui.Program.Run();//It is the entry when the app uses the default GUI.
        }

        new static public string GetAbout()
        {
            return @"
Created: " + Cliver.Bot.Program.GetCustomizationCompiledTime().ToString() + @"
Developed by: www.cliversoft.com";
        }

        new static public void FatalError(string message)
        {
        }
    }

    public class CustomSession : Session
    {
        public override void CREATING()
        {
            //Set the order which queues are to be processed by. When it is not set, it is built automatically along LIFO rule.
            Session.SetInputItemQueuesOrder(typeof(Product), typeof(Category), typeof(Site));

            //Set the queue which the progress bar will reflect. 
            Cliver.BotGui.Program.BindProgressBar2InputItemQueue<Product>();

            //It is possible to add InputItems to queues before BotCycle started            
            Session.Add<Site>(new { Url = "www.google.com" });

            counters["product"] = 0;
        }

        public override void CLOSING()
        {
        }

        /// <summary>
        /// Custom InputItem types are defined as classes based on InputItem
        /// </summary>
        public class Site : InputItem
        {
            /// <summary>
            /// Attribute KeyField can be set to indicate what fields participate in the key. When it is not used, all public types in the class participate in the key.
            ///Otherwise, attribute NotKeyField can be set to exclude the marked field from the key. 
            ///KeyField and NotKeyField cannot be used at the same time within the same InputItem class.
            /// </summary>
            [KeyField]
            //Fields must be public value or string type.
            readonly public string Url;

            #region non needed for logic - only as a demo of the paramterless constructor
            /// <summary>
            /// a sample of field that must be set by constructor explicitly
            /// </summary>
            [ConstructedField]
            readonly public string Test;

            /// <summary>
            /// The parameterless constructor is invoked AFTER all fields are set excluding those atributed with ConstructedField
            /// </summary>
            public Site()
            {
                Test = "It is only for test: " + Url;
            }
            #endregion

            /// <summary>
            /// InputItem can define a method to process it. 
            /// When it is not defined within InputItem class, it must be defined in CustomBot as PROCESSOR_[InputItem class name]
            /// </summary>
            /// <param name="bc">BotCycle that keeps the current thread</param>
            override public void PROCESSOR(BotCycle bc)
            {
                bc.Add(new Category(url: Url + "?q=1", t: new Category.Tag(name: "fff", description: "ttttt")));

                //it is possible to get the current CustomBot when access to common members is needed
                ((CustomSession)bc.Session).counter++;
            }
        }
        int counter = 0;

        public class Category : InputItem
        {
            readonly public string Url;

            /// <summary>
            /// Custom TagItem is designed to be linked by many InputItem objects, to save memory.
            /// It is not key field.
            /// It is serialized automatically just as InputItem's.
            /// </summary>
            public class Tag : TagItem
            {
                readonly public string Name;
                readonly public string Description;

                /// <summary>
                /// Constructor is not a must but makes creating/adding a custom InputItem handier.
                /// Be sure it does no action that can influence on session restoring.
                /// </summary>
                public Tag(string name, string description)
                {
                    Name = name;
                    Description = description;
                }
            }
            readonly public Tag T;

            /// <summary>
            /// Constructor is not a must but makes creating/adding a custom InputItem handier.
            /// Be sure it does no action that can influence on session restoring.
            /// </summary>
            public Category(string url, Tag t)
            {
                Url = url;
                T = t;
            }

            override public void PROCESSOR(BotCycle bc)
            {
                for (int i = 0; i < 3; i++)
                    bc.Add(new Product(url: i + Url));

                //list next page
                bc.Add(new Category(url: "qqqqq2", t: T));
            }
        }

        public class Product : InputItem
        {
            //It is possible to declare property that returns the parent for this item.
            //When defined, they are set by the system automatically.
            //Parent InputItem is the item that is current when new items are added to the system.
            //Also, can be defined not direct parent but grand-parents also.
            //As not always parent item types are the same, these memebers can be null and so should be checked for null and can be used as flags.
            public Category Category { get { return (Category)__ParentItem; } }

            [KeyField]
            readonly public string Url;

            /// <summary>
            /// Constructor is not a must but makes creating/adding a custom InputItem handier.
            /// Be sure it does no action that can influence on session restoring.
            /// </summary>
            public Product(string url)
            {
                Url = url;
            }
        }

        /// <summary>
        /// When PROCESSOR is not defined within InputItem class, it must be defined in CustomSession
        /// It is handier when access to CustomSession members is needed.
        /// </summary>
        /// <param name="item">custom InputItem</param>
        public void PROCESSOR(Product item)
        {
            counters["product"] = counters["product"] + 1;
            if (counters["product"] == 3)
                //ProcessorException has a flag that specifies how to restore the current item.
                throw new ProcessorException(ProcessorExceptionType.RESTORE_AS_NEW, "Could not get product: " + item.Url);
        }

        /// <summary>
        /// A demo of WorkItem and SingleValueWorkItem
        /// </summary>
        static SingleValueWorkItemDictionary<Counter, int> counters = Session.GetSingleValueWorkItemDictionary<Counter, int>();

        /// <summary>
        /// A demo of WorkItem and SingleValueWorkItem
        /// </summary>
        public class Counter : SingleValueWorkItem<int>
        {
        }
    }

    public class CustomBotCycle : BotCycle
    {
        /// <summary>
        /// Invoked by BotCycle thread as it has been started.
        /// </summary>
        public override void STARTING()
        {
        }

        /// <summary>
        /// Invoked by BotCycle thread when it is exiting.
        /// </summary>
        public override void EXITING()
        {
        }
    }
}