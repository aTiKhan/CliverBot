using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web.Script.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Reflection;
using Newtonsoft.Json;

namespace Cliver
{
    public class Serializable
    {
        static public T Load<T>(string file) where T : Serializable, new()
        {
            T t = get<T>(file, InitMode.LOAD);
            t.Loaded();
            return t;
        }

        static public T LoadOrCreate<T>(string file) where T : Serializable, new()
        {
            T t = get<T>(file, InitMode.LOAD_OR_CREATE);
            t.Loaded();
            return t;
        }

        static public T Create<T>(string file) where T : Serializable, new()
        {
            T t = get<T>(file, InitMode.CREATE);
            t.Loaded();
            return t;
        }

        static T get<T>(string file, InitMode init_mode) where T : Serializable, new()
        {
            if (!file.Contains(":"))
                file = Log.AppCommonDataDir + "\\" + file;
            T s;
            if (init_mode == InitMode.CREATE || (init_mode == InitMode.LOAD_OR_CREATE && !File.Exists(file)))
                s = new T();
            else
                s = Cliver.SerializationRoutines.Json.Load<T>(file);
            s.__File = file;
            return s;
        }

        static public Serializable Load(Type serializable_type, string file)
        {
            Serializable t = get(serializable_type, file, InitMode.LOAD);
            t.Loaded();
            return t;
        }

        static public Serializable LoadOrCreate(Type serializable_type, string file)
        {
            Serializable t = get(serializable_type, file, InitMode.LOAD_OR_CREATE);
            t.Loaded();
            return t;
        }

        static public Serializable Create(Type serializable_type, string file)
        {
            Serializable t = get(serializable_type, file,  InitMode.CREATE);
            t.Loaded();
            return t;
        }

        static Serializable get(Type serializable_type, string file, InitMode init_mode)
        {
            if (!file.Contains(":"))
                file = Log.AppCommonDataDir + "\\" + file;
            Serializable s;
            if (init_mode == InitMode.CREATE || (init_mode == InitMode.LOAD_OR_CREATE  && !File.Exists(file)))
                s = (Serializable)Activator.CreateInstance(serializable_type);
            else                   
                s = (Serializable)Cliver.SerializationRoutines.Json.Load(serializable_type, file);
            s.__File = file;
            return s;
        }

        public enum InitMode
        {
            LOAD,
            LOAD_OR_CREATE,
            CREATE
        }

        //[ScriptIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string __File { get; private set; }

        public void Save(string file = null)
        {
            lock (this)
            {
                if (file != null)
                    __File = file;
                Saving();
                Cliver.SerializationRoutines.Json.Save(__File, this);
            }
        }

        virtual public void Loaded()
        {

        }

        virtual public void Saving()
        {

        }
    }
}