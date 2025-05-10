using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Blade.Core.Dependencies
{
    [DefaultExecutionOrder(-10)] //가장 빨리 실행되게 기본은 5
    public class Injector : MonoBehaviour
    {
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly Dictionary<Type, object> _regiestry = new Dictionary<Type, object>();

        private void Awake()
        {
            IEnumerable<IDependencyProvider> providers = GetMonoBehaviour().OfType<IDependencyProvider>();

            foreach (var provider in providers)
            {
                RegisterProvider(provider);
            }
            
            IEnumerable<MonoBehaviour> injectables = GetMonoBehaviour().OfType<MonoBehaviour>().Where(IsInjectable);

            foreach (var injectable in injectables)
            {
                Inject(injectable);
            }
        }

        private void Inject(MonoBehaviour injectableMono)
        {
            Type type = injectableMono.GetType();
            IEnumerable<FieldInfo> injectableFields = type.GetFields(_bindingFlags)
                .Where(field => Attribute.IsDefined(field, typeof(InjectAttribute)));

            foreach (FieldInfo field in injectableFields)
            {
                Type fieldType = field.FieldType;
                object instance = Resolve(fieldType);
                Debug.Assert(instance != null, $"Inject Instance not found for {fieldType}");
                field.SetValue(injectableMono, instance);
            }
            
            IEnumerable<MethodInfo> injectableMethods = type.GetMethods(_bindingFlags)
                .Where(field => Attribute.IsDefined(field, typeof(InjectAttribute)));

            foreach (var method in injectableMethods)
            {
                Type[] requiredParams = method.GetParameters().Select(p => p.ParameterType).ToArray();
                object[] paramValues = requiredParams.Select(Resolve).ToArray();
                method.Invoke(injectableMono, paramValues);
            }
        }

        private object Resolve(Type fieldType)
        {
            _regiestry.TryGetValue(fieldType, out object instance);
            return instance;
        }

        private bool IsInjectable(MonoBehaviour mono)
        {
            MemberInfo[] members = mono.GetType().GetMembers(_bindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }

        private void RegisterProvider(IDependencyProvider provider)
        {
            //클래스 그 자체에 Provide 어트리뷰트가 붙어있는 경우 별도로 찾을 피룡 없이 해당 클래스를 그냥 넣는다.
            if (Attribute.IsDefined(provider.GetType(), typeof(ProvideAttribute)))
            {
                _regiestry.Add(provider.GetType(), provider);
                return;
            }
            
            MethodInfo[] methods = provider.GetType().GetMethods(_bindingFlags);
            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;
                Type returnType = method.ReturnType;
                object instance = method.Invoke(provider, null);
                Debug.Assert(instance != null, $"Provided method {method.Name} returned null.");
                
                _regiestry.Add(returnType, instance);
            }
        }

        private IEnumerable<MonoBehaviour> GetMonoBehaviour()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        }
    }
}