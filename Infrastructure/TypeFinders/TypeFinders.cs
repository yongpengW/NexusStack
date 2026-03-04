using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NexusStack.Infrastructure.TypeFinders
{
    /// <summary>
    /// 通过反射获取程序集中的类型
    /// </summary>
    public static class TypeFinders
    {
        /// <summary>
        /// 判断给定的类型是否实现了指定的泛型接口
        /// </summary>
        /// <param name="givenType"></param>
        /// <param name="genericInterfaceType"></param>
        /// <returns></returns>
        public static bool IsAssignableToGenericInterface(Type givenType, Type genericInterfaceType)
        {
            var interfaceTypes = givenType.GetInterfaces();
            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericInterfaceType)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断给定的类型是否实现了指定的泛型接口
        /// </summary>
        /// <param name="givenType"></param>
        /// <param name="genericInterfaceType"></param>
        /// <returns></returns>
        public static bool IsAssignableToInterface(Type givenType, Type genericInterfaceType)
        {
            var interfaceTypes = givenType.GetInterfaces();
            foreach (var it in interfaceTypes)
            {
                if (it == genericInterfaceType)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsAssignableToType(Type givenType, Type genericType)
        {
            return !givenType.IsInterface && !givenType.IsAbstract && givenType.IsAssignableTo(genericType);
        }

        /// <summary>
        /// 反射判断的三种类型
        /// </summary>
        public enum TypeClassification
        {
            Class, // 类
            Interface, //接口
            GenericInterface  // 泛型接口
        }

        /// <summary>
        /// 根据指定类型接口获取所有实现该接口的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Type> SearchTypes(Type type, TypeClassification classification)
        {
            // 通过NexusStack开头的来查找当前使用的程序中的所有程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(item => item.FullName.StartsWith("NexusStack.")).ToList();
            var types = new List<Type>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    IEnumerable<Type> assemblyTypes;
                    try
                    {
                        assemblyTypes = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // 部分类型加载失败时，使用已成功加载的类型继续扫描
                        assemblyTypes = ex.Types.Where(t => t != null)!;
                        Console.WriteLine($"程序集 {assembly.GetName().Name} 部分类型加载失败: {ex.LoaderExceptions.FirstOrDefault()?.Message}");
                    }

                    foreach (var handleType in assemblyTypes)
                    {
                        switch (classification)
                        {
                            case TypeClassification.Class:
                                if (IsAssignableToType(handleType, type))
                                {
                                    types.Add(handleType);
                                }
                                break;
                            case TypeClassification.Interface:
                                if (IsAssignableToInterface(handleType, type))
                                {
                                    types.Add(handleType);
                                }
                                break;
                            case TypeClassification.GenericInterface:
                                if (IsAssignableToGenericInterface(handleType, type))
                                {
                                    types.Add(handleType);
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"扫描程序集 {assembly.GetName().Name} 时出错: {ex.Message}");
                }
            }

            return types;
        }
    }
}
