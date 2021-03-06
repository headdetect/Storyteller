﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FubuCore;
using FubuCore.CommandLine;
using StoryTeller.Engine;

namespace StoryTeller
{

    [Serializable]
    public class Project
    {
        public static string CurrentProfile
        {
            get { return CurrentProject == null ? null : CurrentProject.Profile; }
        }

        public static int CurrentMaxRetries
        {
            get { return CurrentProject == null ? 0 : CurrentProject.MaxRetries; }
        }

        public static readonly string FILE = "storyteller.config";

        public string SystemTypeName { get; set; }
        public int TimeoutInSeconds { get; set; }
        public string TracingStyle { get; set; }
        public string ConfigFile { get; set; }
        public string Profile { get; set; }
        public static Project CurrentProject { get; set; }
        public int MaxRetries { get; set; }

        public StopConditions StopConditions = new StopConditions();

        public static Project LoadForFolder(string folder)
        {
            var system = new FileSystem();
            var file = folder.AppendPath(FILE);

            if (system.FileExists(file))
            {
                return system.LoadFromFile<Project>(file);
            }

            return new Project();
        }

        public Type DetermineSystemType()
        {
            if (SystemTypeName.IsNotEmpty() && SystemTypeName.Contains(',')) return Type.GetType(SystemTypeName);

            var types = FindSystemTypesInCurrentAssembly();

            if (types.Count() == 1)
            {
                return types.Single();
            }

            if (SystemTypeName.IsNotEmpty())
            {
                return types.FirstOrDefault(x => x.Name == SystemTypeName);
            }

            if (!types.Any())
            {
                return typeof (NulloSystem);
            }

            
            throw new InDeterminateSystemTypeException(types);
        }

        private static Type[] FindSystemTypesInCurrentAssembly()
        {
            var assemblyName = Path.GetFileName(AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                
                var assembly = Assembly.Load(assemblyName);
                return FindSystemTypes(assembly).ToArray();
            }
            catch (Exception e)
            {
                ConsoleWriter.Write("Error trying to find types in an assembly named '{0}'", assemblyName);
                ConsoleWriter.Write(ConsoleColor.Yellow, e.ToString());
                return new Type[0];
            }
        }

        public static bool IsSystemTypeCandidate(Type type)
        {
            return type.CanBeCastTo<ISystem>() && type.IsConcreteWithDefaultCtor() &&
                   !type.IsOpenGeneric();
        }


        public static IEnumerable<Type> FindSystemTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes().Where(IsSystemTypeCandidate);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to scan types in assembly " + assembly.GetName().FullName);
                Console.WriteLine(e);

                return new Type[0];
            }
        }
    }
}